using System.Diagnostics;
using System.Text.RegularExpressions;
using Cyjb.Collections;
using Cyjb.Compilers.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 提供构造词法分析器的功能。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <typeparam name="TController">词法分析控制器的类型。</typeparam>
/// <remarks>
/// <para>泛型参数 <typeparamref name="T"/> 一般是一个枚举类型，用于标识词法单元。</para>
/// <para>对于词法分析中的冲突，总是选择最长的词素。如果最长的词素可以与多个模式匹配，
/// 则选择最先被定义的模式。关于词法分析的相关信息，请参考我的系列博文
/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerIntroduce.html">
/// 《C# 词法分析器（一）词法分析介绍》</see>，词法分析器的使用指南请参见
/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerSummary.html">
/// 《C# 词法分析器（七）总结》</see>。</para></remarks>
/// <example>
/// 下面简单的构造一个数学算式的词法分析器：
/// <code>
/// enum Calc { Id, Add, Sub, Mul, Div, Pow, LBrace, RBrace }
/// Lexer&lt;Calc&gt; lexer = new Lexer&lt;Calc&gt;();
/// // 终结符的定义。
/// lexer.DefineSymbol("[0-9]+").Kind(Calc.Id).Action(c => c.Accept(double.Parse(c.Text)));
/// lexer.DefineSymbol("\\+").Kind(Calc.Add);
/// lexer.DefineSymbol("\\-").Kind(Calc.Sub);
/// lexer.DefineSymbol("\\*").Kind(Calc.Mul);
/// lexer.DefineSymbol("\\/").Kind(Calc.Div);
/// lexer.DefineSymbol("\\^").Kind(Calc.Pow);
/// lexer.DefineSymbol("\\(").Kind(Calc.LBrace);
/// lexer.DefineSymbol("\\)").Kind(Calc.RBrace);
/// // 吃掉所有空白。
/// lexer.DefineSymbol("\\s");
/// ILexerFactory&lt;Calc&gt; lexerFactory = lexer.GetFactory();
/// // 要分析的源文件。
/// string source = "1 + 20 * 3 / 4*(5+6)";
/// ITokenizer&lt;Calc&gt; tokenizer = lexerFactory.CreateTokenizer(source);
/// // 构造词法分析器。
/// foreach (Token&lt;Calc&gt; token in tokenizer)
/// {
/// 	Console.WriteLine(token);
/// }
/// </code>
/// </example>
/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerIntroduce.html">
/// 《C# 词法分析器（一）词法分析介绍》</seealso>
/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerSummary.html">
/// 《C# 词法分析器（七）总结》</seealso>
public class Lexer<T, TController>
	where T : struct
	where TController : LexerController<T>, new()
{
	/// <summary>
	/// 正则表达式列表。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Dictionary<string, LexRegex> regexs = new();
	/// <summary>
	/// 终结符列表。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<Terminal<T>> terminals = new();
	/// <summary>
	/// 词法分析器上下文列表。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Dictionary<string, LexerContext> contexts = new();
	/// <summary>
	/// 词法分析器上下文标签列表。
	/// </summary>
	private string[]? contextLabels;
	/// <summary>
	/// 向前看符号的类型。
	/// </summary>
	private TrailingType trailingType = TrailingType.None;
	/// <summary>
	/// 是否包含行首匹配的规则。
	/// </summary>
	private bool containsBeginningOfLine = false;
	/// <summary>
	/// 最近构造的 DFA。
	/// </summary>
	private Dfa? dfa;
	/// <summary>
	/// 终结符的合并信息。
	/// </summary>
	private readonly Dictionary<int, List<int>> terminalMerge = new();

	/// <summary>
	/// 初始化 <see cref="Lexer{T,TController}"/> 类的新实例。
	/// </summary>
	public Lexer()
	{
		// 定义默认上下文。
		contexts[ContextData.Initial] = new LexerContext(0, ContextData.Initial, LexerContextType.Inclusive);
	}

	/// <summary>
	/// 定义一个指定名称的正则表达式。
	/// </summary>
	/// <param name="name">正则表达式的名称。</param>
	/// <param name="regex">定义的正则表达式。</param>
	/// <param name="options">正则表达式的选项。</param>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> 为 <c>null</c>。</exception>
	/// <exception cref="ArgumentNullException"><paramref name="regex"/> 为 <c>null</c>。</exception>
	/// <exception cref="RegexParseException">正则表达式解析失败。</exception>
	public void DefineRegex(string name, string regex, RegexOptions options = RegexOptions.None)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(regex);
		regexs[name] = LexRegex.Parse(regex, options, regexs);
	}

	/// <summary>
	/// 定义一个新的词法分析器的上下文。
	/// </summary>
	/// <param name="label">上下文的标签。</param>
	public void DefineContext(string label)
	{
		if (contexts.TryGetValue(label, out LexerContext? context))
		{
			if (context.Type == LexerContextType.Exclusive)
			{
				// 已声明相同类型的上下文，忽略即可。
				return;
			}
			else
			{
				// 类型不同，报错。
				throw CompilerExceptions.DuplicateLexerContext(label);
			}
		}
		LexerContext item = new(contexts.Count, label, LexerContextType.Exclusive);
		contexts[label] = item;
		contextLabels = null;
	}

	/// <summary>
	/// 定义一个新的词法分析器的包含型上下文。
	/// </summary>
	/// <param name="label">上下文的标签。</param>
	public void DefineInclusiveContext(string label)
	{
		if (contexts.TryGetValue(label, out LexerContext? context))
		{
			if (context.Type == LexerContextType.Inclusive)
			{
				// 已声明相同类型的上下文，忽略即可。
				return;
			}
			else
			{
				// 类型不同，报错。
				throw CompilerExceptions.DuplicateLexerContext(label);
			}
		}
		LexerContext item = new(contexts.Count, label, LexerContextType.Inclusive);
		contexts[label] = item;
		contextLabels = null;
	}

	/// <summary>
	/// 返回指定标签的词法分析上下文。
	/// </summary>
	/// <param name="label">上下文的名称。</param>
	/// <returns>词法分析的上下文。</returns>
	internal LexerContext GetContext(string label)
	{
		if (contexts.TryGetValue(label.Trim(), out LexerContext? context))
		{
			return context;
		}
		throw new ArgumentException(Resources.InvalidLexerContext(context), nameof(label));
	}

	/// <summary>
	/// 定义使用指定正则表达式的终结符。
	/// </summary>
	/// <param name="regex">终结符使用的正则表达式。</param>
	/// <param name="options">正则表达式的选项。</param>
	/// <returns>终结符的构造器。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="regex"/> 为 <c>null</c>。</exception>
	public ITerminalBuilder<T, TController> DefineSymbol(string regex, RegexOptions options = RegexOptions.None)
	{
		ArgumentNullException.ThrowIfNull(regex);
		if (regex == "<<EOF>>")
		{
			return new EofBuilder<T, TController>(this);
		}
		else
		{
			Terminal<T> terminal = new(terminals.Count, LexRegex.Parse(regex, options, regexs));
			terminals.Add(terminal);
			return new TerminalBuilder<T, TController>(this, terminal);
		}
	}

	/// <summary>
	/// 获取词法分析器上下文列表。
	/// </summary>
	/// <value>词法分析器的上下文列表。</value>
	public string[] Contexts
	{
		get
		{
			contextLabels ??= contexts.Keys.ToArray();
			return contextLabels;
		}
	}

	/// <summary>
	/// 获取终结符的合并信息。
	/// </summary>
	/// <remarks>键为终结符索引，值为合并前的原始终结符索引。</remarks>
	public Dictionary<int, List<int>> TerminalMerge => terminalMerge;

	/// <summary>
	/// 返回词法分析的数据。
	/// </summary>
	/// <param name="rejectable">是否用到了 Reject 动作。</param>
	/// <returns>词法分析的数据。</returns>
	public LexerData<T> GetData(bool rejectable = false)
	{
		// 如果终结符未指定上下文，那么添加所有包含型上下文。
		LexerContext[] inclusiveContexts = this.contexts.Values
			.Where(c => c.Type == LexerContextType.Inclusive).ToArray();
		foreach (Terminal<T> terminal in this.terminals)
		{
			// 此时尚未合并终结符，只会包含一个匹配。
			TerminalMatch match = terminal.Matches[0];
			if (match.Context.Count == 0)
			{
				match.Context.UnionWith(inclusiveContexts);
			}
		}
		Dictionary<string, ContextData> contexts = new();
		foreach (var (label, context) in this.contexts)
		{
			contexts.Add(label, context.GetData());
		}
		// 合并终结符。
		MergeTerminal();
		// 构造 DFA。
		Nfa nfa = BuildNfa();
		int headCount = contexts.Count;
		if (containsBeginningOfLine)
		{
			headCount *= 2;
		}
		dfa = nfa.BuildDFA(headCount, rejectable);
		TerminalData<T>[] terminals = this.terminals.Select(t => t.GetData()).ToArray();
		if (terminals.Any(t => t.UseShortest))
		{
			// 如果包含最短匹配的状态，那么需要调整 DFA 的符号顺序，将最短匹配的符号排在前面。
			foreach (DfaState state in dfa)
			{
				Array.Sort(state.Symbols, (int x, int y) =>
				{
					if (x < 0)
					{
						if (y < 0)
						{
							return x - y;
						}
						else
						{
							return 1;
						}
					}
					else if (y < 0)
					{
						return -1;
					}
					if (terminals[x].UseShortest)
					{
						if (terminals[y].UseShortest)
						{
							return x - y;
						}
						else
						{
							return -1;
						}
					}
					else if (terminals[y].UseShortest)
					{
						return 1;
					}
					return x - y;
				});
			}
		}
		DfaData data = dfa.GetData();
		return new LexerData<T>(contexts, terminals, dfa.GetCharClassMap(), data.States, data.Trans,
			trailingType, containsBeginningOfLine, rejectable, typeof(TController));
	}

	/// <summary>
	/// 返回词法分析的工厂。
	/// </summary>
	/// <param name="rejectable">是否用到了 Reject 动作。</param>
	/// <returns>词法分析器的工厂。</returns>
	public ILexerFactory<T> GetFactory(bool rejectable = false)
	{
		LexerData<T> data = GetData(rejectable);
		return new LexerFactory<T, TController>(data);
	}

	/// <summary>
	/// 返回最近一次构造的 DFA 的字符类描述信息。
	/// </summary>
	/// <returns>最近一次构造的 DFA 的字符类描述信息。</returns>
	public string GetCharClassDescription()
	{
		if (dfa == null)
		{
			return string.Empty;
		}
		return dfa.GetCharClassDescription();
	}

	/// <summary>
	/// 返回最近一次构造的 DFA 的状态描述信息。
	/// </summary>
	/// <returns>最近一次构造的 DFA 的状态描述信息。</returns>
	public string GetStateDescription()
	{
		if (dfa == null)
		{
			return string.Empty;
		}
		return dfa.GetStateDescription();
	}

	/// <summary>
	/// 合并指向相同状态的终结符。
	/// </summary>
	private void MergeTerminal()
	{
		// 合并指向相同状态的终结符。
		HashSet<Terminal<T>> terminalSet = new(Terminal<T>.StateComparer);
		int j = 0;
		for (int i = 0; i < terminals.Count; i++)
		{
			Terminal<T> cur = terminals[i];
			if (terminalSet.TryGetValue(cur, out Terminal<T>? prev))
			{
				// 已包含指向相同状态的终结符，合并过去。
				prev.Matches.AddRange(cur.Matches);
				terminalMerge[prev.Index].Add(cur.Index);
			}
			else
			{
				terminalSet.Add(cur);
				terminalMerge[j] = new List<int>() { cur.Index };
				cur.Index = j;
				terminals[j] = cur;
				j++;
			}
		}
		terminals.RemoveRange(j, terminals.Count - j);
	}

	/// <summary>
	/// 构造 NFA。
	/// </summary>
	/// <returns>构造得到的 NFA。</returns>
	private Nfa BuildNfa()
	{
		foreach (Terminal<T> terminal in terminals)
		{
			if (terminal.ContainsBeginningOfLine())
			{
				containsBeginningOfLine = true;
				break;
			}
		}
		// 将多个上下文的规则放入一个 NFA 中，但起始状态不同。
		Nfa nfa = new();
		for (int i = 0; i < contexts.Count; i++)
		{
			// 为每个上下文创建自己的起始状态，普通规则的起始状态。
			nfa.NewState();
			if (containsBeginningOfLine)
			{
				// 行首规则的起始状态。
				nfa.NewState();
			}
		}
		foreach (Terminal<T> terminal in terminals)
		{
			foreach (TerminalMatch match in terminal.Matches)
			{
				NfaBuildResult result = nfa.BuildRegex(match.RegularExpression, terminal.Index);
				foreach (LexerContext context in match.Context)
				{
					if (result.BeginningOfLine)
					{
						// 行首限定规则。
						nfa[context.Index * 2 + 1].Add(result.Head);
					}
					else
					{
						// 普通规则。
						if (containsBeginningOfLine)
						{
							nfa[context.Index * 2].Add(result.Head);
							nfa[context.Index * 2 + 1].Add(result.Head);
						}
						else
						{
							nfa[context.Index].Add(result.Head);
						}
					}
				}
			}
		}
		trailingType = nfa.TrailingType;
		return nfa;
	}
}
