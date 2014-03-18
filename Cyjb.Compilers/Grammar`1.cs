using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Cyjb.Compilers.Lexers;
using Cyjb.Compilers.Parsers;
using Cyjb.Compilers.RegularExpressions;
using Cyjb.IO;
using Cyjb.Text;

namespace Cyjb.Compilers
{
	/// <summary>
	/// 表示词法分析或语法分析中使用的语法规则。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	/// <remarks>
	/// <para>泛型参数 <typeparamref name="T"/> 必须是一个枚举类型，用于标识词法单元。
	/// 其中包含了所有终结符和非终结符的定义，要求终结符必须先于非终结符定义，
	/// 而且它们的值必须是从零开始的。</para>
	/// <para>对于词法分析中的冲突，总是选择最长的词素。如果最长的词素可以与多个模式匹配，
	/// 则选择最先被定义的模式。关于词法分析的相关信息，请参考我的系列博文
	/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerIntroduce.html">
	/// 《C# 词法分析器（一）词法分析介绍》</see>，词法分析器的使用指南请参见
	/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerSummary.html">
	/// 《C# 词法分析器（七）总结》</see>。</para></remarks>
	/// <para>在语法分析中，认为第一个被定义的非终结符，是文法的起始非终结符。</para>
	/// <example>
	/// 下面简单的构造一个数学算式的词法分析器：
	/// <code>
	/// enum Calc { Id, Add, Sub, Mul, Div, Pow, LBrace, RBrace }
	/// Grammar&lt;Calc&gt; grammar = new Grammar&lt;Calc&gt;();
	/// // 终结符的定义。
	/// grammar.DefineSymbol(Calc.Id, "[0-9]+", c => c.Accept(double.Parse(c.Text)));
	/// grammar.DefineSymbol(Calc.Add, "\\+");
	/// grammar.DefineSymbol(Calc.Sub, "\\-");
	/// grammar.DefineSymbol(Calc.Mul, "\\*");
	/// grammar.DefineSymbol(Calc.Div, "\\/");
	/// grammar.DefineSymbol(Calc.Pow, "\\^");
	/// grammar.DefineSymbol(Calc.LBrace, "\\(");
	/// grammar.DefineSymbol(Calc.RBrace, "\\)");
	/// // 吃掉所有空白。
	/// grammar.DefineSymbol("\\s", c => { });
	/// // 要分析的源文件。
	/// string source = "1 + 20 * 3 / 4*(5+6)";
	/// // 构造词法分析器。
	/// TokenReader&lt;Calc&gt; reader = grammar.GetReader(source);
	/// foreach (Token&lt;Calc&gt; token in reader)
	/// {
	/// 	Console.WriteLine(token);
	/// }
	/// </code>
	/// 下面构造一个相应的语法分析器：
	/// <code>
	/// enum Calc { Id, Add, Sub, Mul, Div, Pow, LBrace, RBrace, E }
	/// // 非终结符的定义。
	/// grammar.DefineSymbol(Calc.E,
	/// 	grammar.Body(Calc.Id).Action(c => c[0].Value),
	/// 	grammar.Body(Calc.E, Calc.Add, Calc.E).Action(c => (double)c[0].Value + (double)c[2].Value),
	/// 	grammar.Body(Calc.E, Calc.Sub, Calc.E).Action(c => (double)c[0].Value - (double)c[2].Value),
	/// 	grammar.Body(Calc.E, Calc.Mul, Calc.E).Action(c => (double)c[0].Value * (double)c[2].Value),
	/// 	grammar.Body(Calc.E, Calc.Div, Calc.E).Action(c => (double)c[0].Value / (double)c[2].Value),
	/// 	grammar.Body(Calc.E, Calc.Pow, Calc.E).Action(c => Math.Pow((double)c[0].Value, (double)c[2].Value)),
	/// 	grammar.Body(Calc.LBrace, Calc.E, Calc.RBrace).Action(c => c[1].Value)
	/// );
	/// // 定义运算符优先级。
	/// grammar.DefineAssociativity(AssociativeType.Left, Calc.Add, Calc.Sub);
	/// grammar.DefineAssociativity(AssociativeType.Left, Calc.Mul, Calc.Div);
	/// grammar.DefineAssociativity(AssociativeType.Right, Calc.Pow);
	/// grammar.DefineAssociativity(AssociativeType.NonAssociate, Calc.Id);
	/// // 构造语法分析器。
	/// TokenParser&lt;Calc&gt; parser = grammar.GetParser();
	/// // 这里需要保证 reader 还并未读取。
	/// parser.Parse(reader);
	/// Console.WriteLine(parser.Result.Value);
	/// </code>
	/// </example>
	/// <seealso cref="Cyjb.Compilers.Lexers.LexerRule&lt;T&gt;"/>
	/// <seealso cref="Cyjb.Compilers.Parsers.ParserRule&lt;T&gt;"/>
	/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerIntroduce.html">
	/// 《C# 词法分析器（一）词法分析介绍》</seealso>
	/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerSummary.html">
	/// 《C# 词法分析器（七）总结》</seealso>
	public sealed class Grammar<T>
		where T : struct
	{

		#region 词法分析器定义

		/// <summary>
		/// 默认的接受动作。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static readonly Action<ReaderController<T>> DefaultAccept = c => c.Accept();
		/// <summary>
		/// 正则表达式列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Dictionary<string, Regex> regexs = new Dictionary<string, Regex>();
		/// <summary>
		/// 终结符列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly List<Terminal<T>> terminals = new List<Terminal<T>>();
		/// <summary>
		/// 词法分析器上下文列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly LexerContextCollection lexerContexts = new LexerContextCollection();
		/// <summary>
		/// 词法分析器的规则。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LexerRule<T> lexerRule;
		/// <summary>
		/// 词法分析器的规则是否发生了改变。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool lexerChanged = true;

		#endregion // 词法分析器定义

		#region 语法分析器定义

		/// <summary>
		/// 非终结符号列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly List<NonTerminal<T>> nonTerminals = new List<NonTerminal<T>>();
		/// <summary>
		/// 定义的产生式的数量，跳过增广文法的产生式。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int productionCount = 1;
		/// <summary>
		/// 终结符的结合性信息。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Dictionary<T, Associativity> associativities =
			new Dictionary<T, Associativity>();
		/// <summary>
		/// 定义的终结符的优先级。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int priority = 0;
		/// <summary>
		/// 语法分析器的规则。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ParserRule<T> parserRule;
		/// <summary>
		/// 语法分析器的规则是否发生了改变。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool parserChanged = true;

		#endregion // 语法分析器定义

		/// <summary>
		/// 终结符和非终结符的标识符列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Dictionary<T, Symbol<T>> symbolIds = new Dictionary<T, Symbol<T>>();
		/// <summary>
		/// 初始化 <see cref="Grammar&lt;T&gt;"/> 类的新实例。
		/// </summary>
		public Grammar()
		{
			this.lexerContexts.DefineInclusiveContext(Constants.InitialContext);
		}

		#region 获取词法分析器

		/// <summary>
		/// 获取定义的正则表达式列表。
		/// </summary>
		/// <value>定义的正则表达式列表。</value>
		public IDictionary<string, Regex> Regexs
		{
			get { return this.regexs; }
		}
		/// <summary>
		/// 获取词法分析器上下文列表。
		/// </summary>
		/// <value>词法分析器的上下文列表。</value>
		public ICollection<string> Contexts
		{
			get { return lexerContexts.Labels; }
		}
		/// <summary>
		/// 获取定义的终结符号的集合。
		/// </summary>
		/// <value>所有定义的终结符号的集合。</value>
		internal List<Terminal<T>> Terminals
		{
			get { return terminals; }
		}
		/// <summary>
		/// 获取识别当前语法规则的词法分析器规则。
		/// </summary>
		/// <value>识别当前语法规则的词法分析器规则。</value>
		public LexerRule<T> LexerRule
		{
			get
			{
				if (lexerChanged)
				{
					this.lexerRule = new LexerRule<T>(this);
					lexerChanged = false;
				}
				return this.lexerRule;
			}
		}
		/// <summary>
		/// 返回指定源文件的词法单元读取器。
		/// </summary>
		/// <param name="source">要读取的源文件。</param>
		/// <returns>指定源文件的词法单元读取器。</returns>
		/// <overloads>
		/// <summary>
		/// 返回指定源文件的词法单元读取器。
		/// </summary>
		/// </overloads>
		public TokenReader<T> GetReader(string source)
		{
			return LexerRule.GetReader(source);
		}
		/// <summary>
		/// 返回指定源文件的词法单元读取器。
		/// </summary>
		/// <param name="source">要读取的源文件。</param>
		/// <returns>指定源文件的词法单元读取器。</returns>
		public TokenReader<T> GetReader(SourceReader source)
		{
			return LexerRule.GetReader(source);
		}
		/// <summary>
		/// 返回指定源文件的允许拒绝的词法单元读取器。
		/// </summary>
		/// <param name="source">要读取的源文件。</param>
		/// <returns>指定源文件的词法单元读取器。</returns>
		/// <overloads>
		/// <summary>
		/// 返回指定源文件的允许拒绝的词法单元读取器。
		/// </summary>
		/// </overloads>
		public TokenReader<T> GetRejectableReader(string source)
		{
			return LexerRule.GetRejectableReader(source);
		}
		/// <summary>
		/// 返回指定源文件的允许拒绝的词法单元读取器。
		/// </summary>
		/// <param name="source">要读取的源文件。</param>
		/// <returns>指定源文件的词法单元读取器。</returns>
		public TokenReader<T> GetRejectableReader(SourceReader source)
		{
			return LexerRule.GetRejectableReader(source);
		}

		#endregion // 获取词法分析器

		#region 获取语法分析器

		/// <summary>
		/// 获取定义的终结符号的集合。
		/// </summary>
		/// <value>所有定义的终结符号的集合。</value>
		internal List<NonTerminal<T>> NonTerminals
		{
			get { return nonTerminals; }
		}
		/// <summary>
		/// 获取定义的产生式的数量。
		/// </summary>
		/// <value>定义的产生式的数量。</value>
		internal int ProductionCount { get { return this.productionCount; } }
		/// <summary>
		/// 获取增广文法的起始非终结符。
		/// </summary>
		/// <value>增广文法的起始非终结符。</value>
		internal NonTerminal<T> AugmentedStart { get; private set; }
		/// <summary>
		/// 获取识别当前语法规则的语法分析器规则。
		/// </summary>
		/// <value>识别当前语法规则的语法分析器规则。</value>
		public ParserRule<T> ParserRule
		{
			get
			{
				if (parserChanged)
				{
					if (this.productionCount == 0)
					{
						// 没有包含文法。
						this.parserRule = null;
					}
					else
					{
						Dictionary<T, Symbol<T>> symbolMapper = new Dictionary<T, Symbol<T>>();
						// 寻找最小的非终结符标识符。
						// 要跳过第一个非终结符，它是增广文法的起始非终结符。
						int minNonTerminal = nonTerminals.Skip(1).Select(sym => Constants.ToInt32(sym.Id)).Min();
						int nonTerminalOffset = minNonTerminal - 1;
						// 更新非终结符的索引。
						foreach (NonTerminal<T> sym in nonTerminals.Skip(1))
						{
							symbolMapper.Add(sym.Id, sym);
							sym.Index = Constants.ToInt32(sym.Id) - nonTerminalOffset;
						}
						// 认为非终结符之前的都是终结符，按顺序创建终结符。
						for (int i = 0; i < minNonTerminal; i++)
						{
							T id = (T)(object)i;
							symbolMapper.Add(id, new Terminal<T>(id, i, null, null));
						}
						int cnt = nonTerminals.Count;
						for (int i = 0; i < cnt; i++)
						{
							nonTerminals[i].BuildProductions(symbolMapper);
						}
						// 动作表中包含了 EOF、ERROR 和 UNIQUE，所以长度要加 3。
						this.parserRule = new ParserRule<T>(this, nonTerminalOffset, minNonTerminal + 3);
						parserChanged = false;
					}
				}
				return this.parserRule;
			}
		}
		/// <summary>
		/// 返回词法单元分析器。
		/// </summary>
		/// <returns>词法单元分析器的实例。</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public TokenParser<T> GetParser()
		{
			return this.ParserRule.GetParser();
		}

		#endregion // 获取语法分析器

		#region TerminalSymbol

		#region Regex 字符串

		/// <summary>
		/// 定义使用默认接受动作的终结符。
		/// </summary>
		/// <param name="id">终结符的标识符，不能与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <returns>定义的终结符。</returns>
		/// <exception cref="System.ArgumentException">标识符与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</exception>
		/// <overloads>
		/// <summary>
		/// 定义终结符。
		/// </summary>
		/// </overloads>
		public void DefineSymbol(T id, string regex)
		{
			CheckSymbolId(id);
			ExceptionHelper.CheckArgumentNull(regex, "regex");
			InternalDefineSymbol(id, regex, DefaultAccept);
		}
		/// <summary>
		/// 定义具有指定正则表达式和动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		public void DefineSymbol(string regex, Action<ReaderController<T>> action)
		{
			ExceptionHelper.CheckArgumentNull(regex, "regex");
			InternalDefineSymbol(regex, action);
		}
		/// <summary>
		/// 定义具有指定正则表达式和动作的终结符。
		/// </summary>
		/// <param name="id">终结符的标识符，不能与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <exception cref="System.ArgumentException">标识符与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</exception>
		public void DefineSymbol(T id, string regex, Action<ReaderController<T>> action)
		{
			CheckSymbolId(id);
			ExceptionHelper.CheckArgumentNull(regex, "regex");
			InternalDefineSymbol(id, regex, action);
		}

		#endregion // Regex 字符串

		#region Regex 对象

		/// <summary>
		/// 定义一个使用默认接受动作的终结符。
		/// </summary>
		/// <param name="id">终结符的标识符，不能与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <exception cref="System.ArgumentException">标识符与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</exception>
		public void DefineSymbol(T id, Regex regex)
		{
			InternalDefineSymbol(id, regex, DefaultAccept, null);
		}
		/// <summary>
		/// 在指定上下文中定义使用默认接受动作的终结符。
		/// </summary>
		/// <param name="id">终结符的标识符，不能与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <exception cref="System.ArgumentException">标识符与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</exception>
		public void DefineSymbol(T id, Regex regex, IEnumerable<string> contexts)
		{
			InternalDefineSymbol(id, regex, DefaultAccept, contexts);
		}

		#endregion // Regex 对象

		#region Regex 对象 + Action

		/// <summary>
		/// 定义具有指定正则表达式和动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <returns>定义的终结符。</returns>
		public void DefineSymbol(Regex regex, Action<ReaderController<T>> action)
		{
			InternalDefineSymbol(regex, action, null);
		}
		/// <summary>
		/// 定义具有指定正则表达式和动作的终结符。
		/// </summary>
		/// <param name="id">终结符的标识符，不能与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <returns>定义的终结符。</returns>
		/// <exception cref="System.ArgumentException">标识符与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</exception>
		public void DefineSymbol(T id, Regex regex, Action<ReaderController<T>> action)
		{
			CheckSymbolId(id);
			InternalDefineSymbol(id, regex, action, null);
		}
		/// <summary>
		/// 在指定上下文中定义具有指定正则表达式和动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <param name="action">终结符的动作。</param>
		public void DefineSymbol(Regex regex, IEnumerable<string> contexts, Action<ReaderController<T>> action)
		{
			InternalDefineSymbol(regex, action, contexts);
		}
		/// <summary>
		/// 在指定上下文中定义具有指定正则表达式和动作的终结符。
		/// </summary>
		/// <param name="id">终结符的标识符，不能与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <param name="action">终结符的动作。</param>
		/// <exception cref="System.ArgumentException">标识符与 <see cref="Token&lt;T&gt;"/> 
		/// 类中定义的特殊标识符相同。</exception>
		public void DefineSymbol(T id, Regex regex, IEnumerable<string> contexts, Action<ReaderController<T>> action)
		{
			CheckSymbolId(id);
			InternalDefineSymbol(id, regex, action, contexts);
		}

		#endregion // Regex 对象 + Action

		/// <summary>
		/// 定义一个终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		private void InternalDefineSymbol(string regex, Action<ReaderController<T>> action)
		{
			IEnumerable<string> context = ParseContexts(ref regex);
			InternalDefineSymbol(RegexParser.ParseRegex(regex, RegexOptions.None, this.regexs), action, context);
		}
		/// <summary>
		/// 定义一个终结符。
		/// </summary>
		/// <param name="id">终结符的标识符。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		private void InternalDefineSymbol(T id, string regex, Action<ReaderController<T>> action)
		{
			IEnumerable<string> context = ParseContexts(ref regex);
			InternalDefineSymbol(id, RegexParser.ParseRegex(regex, RegexOptions.None, this.regexs), action, context);
		}
		/// <summary>
		/// 定义一个终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		private void InternalDefineSymbol(Regex regex, Action<ReaderController<T>> action,
			IEnumerable<string> contexts)
		{
			ExceptionHelper.CheckArgumentNull(regex, "regex");
			Terminal<T> sym = new Terminal<T>(terminals.Count, regex, action);
			if (contexts == null || !contexts.Any())
			{
				// 添加所有包含型上下文。
				sym.Context.UnionWith(
					this.lexerContexts.Where(c => c.ContextType == LexerContextType.Inclusive));
			}
			else
			{
				sym.Context.UnionWith(CheckContexts(contexts));
			}
			terminals.Add(sym);
			lexerChanged = true;
		}
		/// <summary>
		/// 定义一个终结符。
		/// </summary>
		/// <param name="id">终结符的标识符。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		private void InternalDefineSymbol(T id, Regex regex, Action<ReaderController<T>> action,
			IEnumerable<string> contexts)
		{
			ExceptionHelper.CheckArgumentNull(regex, "regex");
			Terminal<T> sym = new Terminal<T>(id, terminals.Count, regex, action);
			if (contexts == null || !contexts.Any())
			{
				// 添加所有包含型上下文。
				sym.Context.UnionWith(
					this.lexerContexts.Where(c => c.ContextType == LexerContextType.Inclusive));
			}
			else
			{
				sym.Context.UnionWith(CheckContexts(contexts));
			}
			terminals.Add(sym);
			symbolIds.Add(id, sym);
			lexerChanged = true;
		}
		/// <summary>
		/// 解析指定正则表达式中定义的上下文。
		/// </summary>
		/// <param name="regex">要解析的正则表达式。</param>
		/// <returns>正则表达式中定义的上下文。</returns>
		private static IEnumerable<string> ParseContexts(ref string regex)
		{
			if (regex[0] == '<' && regex.IndexOf("<<EOF>>", StringComparison.OrdinalIgnoreCase) != 0)
			{
				// 处理上下文。
				int idx = regex.IndexOf('>');
				if (idx == -1)
				{
					throw CompilerExceptionHelper.IncompleteLexerContext("regex");
				}
				string contextStr = regex.Substring(1, idx - 1);
				regex = regex.Substring(idx + 1);
				if (contextStr != "*")
				{
					return contextStr.Split(',');
				}
			}
			return new string[0];
		}
		/// <summary>
		/// 检查给定的上下文是否是有效的。
		/// </summary>
		/// <param name="contexts">要检查的上下文。</param>
		/// <returns>要检查的上下文。</returns>
		private IEnumerable<LexerContext> CheckContexts(IEnumerable<string> contexts)
		{
			foreach (string label in contexts)
			{
				if (label != null)
				{
					string tmpLabel = label.Trim();
					LexerContext context;
					if (!this.lexerContexts.TryGetItem(tmpLabel, out context))
					{
						throw ExceptionHelper.InvalidLexerContext("contexts", tmpLabel);
					}
					yield return context;
				}
			}
		}

		#endregion // TerminalSymbol

		#region 正则表达式

		/// <summary>
		/// 定义一个指定名称的正则表达式。
		/// </summary>
		/// <param name="name">正则表达式的名称。</param>
		/// <param name="regex">定义的正则表达式。</param>
		/// <overloads>
		/// <summary>
		/// 定义一个指定名称的正则表达式。
		/// </summary>
		/// </overloads>
		public void DefineRegex(string name, Regex regex)
		{
			regexs[name] = regex;
		}
		/// <summary>
		/// 定义一个指定名称的正则表达式。
		/// </summary>
		/// <param name="name">正则表达式的名称。</param>
		/// <param name="regex">定义的正则表达式。</param>
		public void DefineRegex(string name, string regex)
		{
			regexs[name] = RegexParser.ParseRegex(regex, RegexOptions.None, regexs);
		}

		#endregion // 正则表达式

		#region 上下文

		/// <summary>
		/// 定义一个新的词法分析器的上下文。
		/// </summary>
		/// <param name="label">上下文的标签。</param>
		public void DefineContext(string label)
		{
			lexerContexts.DefineContext(label);
		}
		/// <summary>
		/// 定义一个新的词法分析器的包含型上下文。
		/// </summary>
		/// <param name="label">上下文的标签。</param>
		public void DefineInclusiveContext(string label)
		{
			lexerContexts.DefineInclusiveContext(label);
		}

		#endregion // 上下文

		#region NonTerminalSymbol

		/// <summary>
		/// 定义指定标识符的非终结符。
		/// </summary>
		/// <param name="id">要定义的非终结符的标识符。</param>
		/// <param name="bodies">非终结符的产生式体。</param>
		public void DefineSymbol(T id, params ProductionBody<T>[] bodies)
		{
			CheckSymbolId(id);
			if (nonTerminals.Count == 0)
			{
				// 定义增广文法。
				this.AugmentedStart = new NonTerminal<T>(Symbol<T>.Invalid, 0,
					new ProductionBody<T>[] { new ProductionBody<T>(0, new T[] { id }) });
				nonTerminals.Add(this.AugmentedStart);
			}
			// 索引之后会被更新。
			NonTerminal<T> sym = new NonTerminal<T>(id, 0, bodies);
			nonTerminals.Add(sym);
			symbolIds.Add(id, sym);
			parserChanged = true;
		}
		/// <summary>
		/// 返回产生式体。
		/// </summary>
		/// <param name="bodies">产生式体包含的终结符或非终结符。</param>
		/// <returns>产生式体。</returns>
		public ProductionBody<T> Body(params T[] bodies)
		{
			for (int i = 0; i < bodies.Length; i++)
			{
				if (EqualityComparer<T>.Default.Equals(bodies[i], Token<T>.EndOfFile))
				{
					throw CompilerExceptionHelper.InvalidSymbolId("bodies", bodies[i].ToString());
				}
				else if (!EqualityComparer<T>.Default.Equals(bodies[i], Token<T>.Error) &&
					Constants.ToInt32(bodies[i]) < 0)
				{
					throw CompilerExceptionHelper.InvalidSymbolId("bodies", bodies[i].ToString());
				}
			}
			return new ProductionBody<T>(productionCount++, bodies ?? new T[0]);
		}

		#endregion // NonTerminalSymbol

		#region 结合性

		/// <summary>
		/// 定义具有指定结合性的终结符集合。
		/// </summary>
		/// <param name="type">终结符集合的结合性。</param>
		/// <param name="symbols">具有相同结合性的终结符的集合。</param>
		public void DefineAssociativity(AssociativeType type, params T[] symbols)
		{
			Associativity associativity = new Associativity(priority++, type);
			for (int i = 0; i < symbols.Length; i++)
			{
				this.associativities.Add(symbols[i], associativity);
			}
			parserChanged = true;
		}
		/// <summary>
		/// 比较产生式和终结符的优先级。
		/// </summary>
		/// <param name="production">要比较的产生式。</param>
		/// <param name="symbol">要比较的终结符。</param>
		/// <returns>如果产生式优先级较高，则为 <c>1</c>；
		/// 如果终结符优先级较高，则为 <c>-1</c>；如果未定义则为 <c>0</c>。
		/// 如果优先级相同，左结合则为 <c>1</c>，右结合则为 <c>-1</c>，非结合则为 <c>0</c>。
		/// </returns>
		internal int CompareAssociativity(Production<T> production, T symbol)
		{
			Terminal<T> assocSymbol = production.Precedence;
			if (assocSymbol == null)
			{
				return 0;
			}
			return CompareAssociativity(assocSymbol.Id, symbol);
		}
		/// <summary>
		/// 比较两个产生式的优先级。
		/// </summary>
		/// <param name="leftProduction">要比较的第一个产生式。</param>
		/// <param name="rightProduction">要比较的第二个产生式。</param>
		/// <returns>如果第一个产生式优先级较高，则为 <c>1</c>；
		/// 如果第二个产生式优先级较高，则为 <c>-1</c>；如果未定义则为 <c>0</c>。
		/// 如果优先级相同，左结合则为 <c>1</c>，右结合则为 <c>-1</c>，非结合则为 <c>0</c>。
		/// </returns>
		internal int CompareAssociativity(Production<T> leftProduction, Production<T> rightProduction)
		{
			Terminal<T> assocSymbolL = leftProduction.Precedence;
			Terminal<T> assocSymbolR = rightProduction.Precedence;
			if (assocSymbolL == null || assocSymbolR == null)
			{
				return 0;
			}
			else
			{
				return CompareAssociativity(assocSymbolL.Id, assocSymbolR.Id);
			}
		}
		/// <summary>
		/// 比较两个终结符的优先级。
		/// </summary>
		/// <param name="left">要比较的第一个终结符。</param>
		/// <param name="right">要比较的第二个终结符。</param>
		/// <returns>如果第一个终结符优先级较高，则为 <c>1</c>；
		/// 如果第二个终结符优先级较高，则为 <c>-1</c>；如果未定义则为 <c>0</c>。
		/// 如果优先级相同，左结合则为 <c>1</c>，右结合则为 <c>-1</c>，非结合则为 <c>0</c>。
		/// </returns>
		internal int CompareAssociativity(T left, T right)
		{
			Associativity assocL, assocR;
			if (!this.associativities.TryGetValue(left, out assocL) ||
				!this.associativities.TryGetValue(right, out assocR))
			{
				return 0;
			}
			if (assocL.Priority > assocR.Priority)
			{
				return 1;
			}
			else if (assocL.Priority < assocR.Priority)
			{
				return -1;
			}
			if (assocL.AssociativeType == AssociativeType.Left)
			{
				return 1;
			}
			else if (assocL.AssociativeType == AssociativeType.Right)
			{
				return -1;
			}
			return 0;
		}

		#endregion // 结合性

		/// <summary>
		/// 检查指定符号的标识符。
		/// </summary>
		/// <param name="id">要检查的标识符。</param>
		public void CheckSymbolId(T id)
		{
			if (EqualityComparer<T>.Default.Equals(id, Token<T>.EndOfFile) ||
				EqualityComparer<T>.Default.Equals(id, Token<T>.Error))
			{
				throw CompilerExceptionHelper.InvalidSymbolId("id", id.ToString());
			}
			else if (Constants.ToInt32(id) < 0)
			{
				throw CompilerExceptionHelper.InvalidSymbolId("id", id.ToString());
			}
			if (symbolIds.ContainsKey(id))
			{
				throw CompilerExceptionHelper.DuplicatedSymbolId("id", id.ToString());
			}
		}
	}
}
