using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cyjb.Compiler.Lexer;
using Cyjb.Compiler.RegularExpressions;

namespace Cyjb.Compiler
{
	/// <summary>
	/// 表示词法分析或语法分析中使用的语法规则。
	/// </summary>
	[Serializable]
	public sealed class Grammar
	{

		#region 词法分析器定义

		/// <summary>
		/// 默认的接受动作。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static readonly Action<ReaderController> DefaultAccept = controller => controller.Accept();
		/// <summary>
		/// 正则表达式列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Dictionary<string, Regex> regexs = new Dictionary<string, Regex>();
		/// <summary>
		/// 终结符号列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly SymbolCollection<TerminalSymbol> terminalSymbols = new SymbolCollection<TerminalSymbol>();
		/// <summary>
		/// 词法分析器上下文列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly LexerContextCollection lexerContexts = new LexerContextCollection();

		#endregion // 词法分析器定义

		/// <summary>
		/// 初始化 <see cref="Grammar"/> 类的新实例。
		/// </summary>
		public Grammar()
		{
			InitialContext = lexerContexts.DefineInclusiveContext("Initial");
		}
		/// <summary>
		/// 获取默认的词法分析器上下文。
		/// </summary>
		public LexerContext InitialContext { get; private set; }
		/// <summary>
		/// 获取词法分析器上下文列表。
		/// </summary>
		public LexerContextCollection Contexts { get { return lexerContexts; } }
		/// <summary>
		/// 获取定义的正则表达式的列表。
		/// </summary>
		public IDictionary<string, Regex> Regexs { get { return regexs; } }
		/// <summary>
		/// 获取定义的终结符号的列表。
		/// </summary>
		public SymbolCollection<TerminalSymbol> TerminalSymbols { get { return terminalSymbols; } }
		/// <summary>
		/// 创建识别当前语法的词法分析器。
		/// </summary>
		/// <returns>词法分析器的规则。</returns>
		public LexerRule CreateLexer()
		{
			return new LexerRule(this);
		}

		#region TerminalSymbol

		/// <summary>
		/// 定义一个仅用于占位的终结符。
		/// </summary>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol()
		{
			TerminalSymbol symbol = new TerminalSymbol(terminalSymbols.Count, null, null);
			terminalSymbols.InternalAdd(symbol);
			return symbol;
		}

		#region 字符串 Regex

		/// <summary>
		/// 定义一个使用默认接受动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(string regex)
		{
			return DefineSymbol(regex, DefaultAccept);
		}
		/// <summary>
		/// 定义一个终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(string regex, Action<ReaderController> action)
		{
			ExceptionHelper.CheckArgumentNull(regex, "regex");
			IEnumerable<LexerContext> context = null;
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
				if (contextStr == "*")
				{
					// 在所有上下文中都有效。
					context = lexerContexts;
				}
				else
				{
					// 仅在部分上下文中有效。
					context = CheckContexts(contextStr.Split(','));
				}
			}
			return DefineSymbol(RegexParser.ParseRegex(regex, RegexOptions.None, this.regexs), action, context);
		}

		#endregion // 字符串 Regex

		#region 普通 Regex

		/// <summary>
		/// 定义一个使用默认接受动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(Regex regex)
		{
			return DefineSymbol(regex, DefaultAccept);
		}
		/// <summary>
		/// 在指定上下文中定义一个使用默认接受动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(Regex regex, params LexerContext[] contexts)
		{
			return DefineSymbol(regex, DefaultAccept, contexts);
		}
		/// <summary>
		/// 在指定上下文中定义一个使用默认接受动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(Regex regex, IEnumerable<LexerContext> contexts)
		{
			return DefineSymbol(regex, DefaultAccept, contexts);
		}
		/// <summary>
		/// 在指定上下文中定义一个使用默认接受动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(Regex regex, params string[] contexts)
		{
			return DefineSymbol(regex, DefaultAccept, contexts);
		}
		/// <summary>
		/// 在指定上下文中定义一个使用默认接受动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(Regex regex, IEnumerable<string> contexts)
		{
			return DefineSymbol(regex, DefaultAccept, contexts);
		}

		#endregion // 普通 Regex

		#region 普通 Regex + Action

		/// <summary>
		/// 定义一个终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(Regex regex, Action<ReaderController> action)
		{
			TerminalSymbol symbol = InternalDefineSymbol(regex, action);
			SetInclusiveContext(symbol);
			return symbol;
		}
		/// <summary>
		/// 在指定上下文中定义具有指定正则表达式和动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(Regex regex, Action<ReaderController> action,
			params LexerContext[] contexts)
		{
			TerminalSymbol symbol = InternalDefineSymbol(regex, action);
			if (contexts == null || contexts.Length == 0)
			{
				SetInclusiveContext(symbol);
			}
			else
			{
				symbol.Context.UnionWith(CheckContexts(contexts));
			}
			return symbol;
		}
		/// <summary>
		/// 在指定上下文中定义具有指定正则表达式和动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(Regex regex, Action<ReaderController> action,
			IEnumerable<LexerContext> contexts)
		{
			TerminalSymbol symbol = InternalDefineSymbol(regex, action);
			if (contexts == null || !contexts.Any())
			{
				SetInclusiveContext(symbol);
			}
			else
			{
				symbol.Context.UnionWith(CheckContexts(contexts));
			}
			return symbol;
		}
		/// <summary>
		/// 在指定上下文中定义具有指定正则表达式和动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(Regex regex, Action<ReaderController> action,
			params string[] contexts)
		{
			TerminalSymbol symbol = InternalDefineSymbol(regex, action);
			if (contexts == null || contexts.Length == 0)
			{
				SetInclusiveContext(symbol);
			}
			else
			{
				symbol.Context.UnionWith(CheckContexts(contexts));
			}
			return symbol;
		}
		/// <summary>
		/// 在指定上下文中定义具有指定正则表达式和动作的终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <param name="contexts">定义终结符的上下文。</param>
		/// <returns>定义的终结符。</returns>
		public TerminalSymbol DefineSymbol(Regex regex, Action<ReaderController> action,
			IEnumerable<string> contexts)
		{
			TerminalSymbol symbol = InternalDefineSymbol(regex, action);
			if (contexts == null || !contexts.Any())
			{
				SetInclusiveContext(symbol);
			}
			else
			{
				symbol.Context.UnionWith(CheckContexts(contexts));
			}
			return symbol;
		}

		#endregion // 名称 + 普通 Regex + Action

		/// <summary>
		/// 定义一个终结符。
		/// </summary>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		/// <returns>定义的终结符。</returns>
		private TerminalSymbol InternalDefineSymbol(Regex regex, Action<ReaderController> action)
		{
			ExceptionHelper.CheckArgumentNull(regex, "regex");
			TerminalSymbol symbol = new TerminalSymbol(terminalSymbols.Count, regex, action);
			terminalSymbols.InternalAdd(symbol);
			return symbol;
		}
		/// <summary>
		/// 设置指定终结符的上下文为任何包含型上下文。
		/// </summary>
		/// <param name="symbol">要设置的终结符。</param>
		private void SetInclusiveContext(TerminalSymbol symbol)
		{
			// 添加所有包含型上下文。
			int cnt = lexerContexts.Count;
			for (int i = 0; i < cnt; i++)
			{
				if (lexerContexts[i].ContextType == LexerContextType.Inclusive)
				{
					symbol.Context.Add(lexerContexts[i]);
				}
			}
		}
		/// <summary>
		/// 检查给定的上下文是否是有效的。
		/// </summary>
		/// <param name="contexts">要检查的上下文。</param>
		/// <returns>要检查的上下文。</returns>
		private IEnumerable<LexerContext> CheckContexts(IEnumerable<LexerContext> contexts)
		{
			foreach (LexerContext context in contexts)
			{
				if (!this.lexerContexts.Contains(context))
				{
					throw CompilerExceptionHelper.InvalidLexerContext("contexts", context.ToString());
				}
			}
			return contexts;
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
					string cl = label.Trim();
					if (cl.Length > 0)
					{
						LexerContext context;
						if (!this.lexerContexts.TryGetItem(cl, out context))
						{
							throw CompilerExceptionHelper.InvalidLexerContext("contexts", cl);
						}
						yield return context;
					}
				}
			}
		}

		#endregion // TerminalSymbol

		#region 正则表达式

		/// <summary>
		/// 定义一个特定名称的正则表达式。
		/// </summary>
		/// <param name="name">正则表达式的名称。</param>
		/// <param name="regex">定义的正则表达式。</param>
		public void DefineRegex(string name, Regex regex)
		{
			regexs[name] = regex;
		}
		/// <summary>
		/// 定义一个特定名称的正则表达式。
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
		/// <returns>词法分析器的上下文。</returns>
		public LexerContext DefineContext()
		{
			return lexerContexts.DefineContext();
		}
		/// <summary>
		/// 定义一个新的词法分析器的上下文。
		/// </summary>
		/// <param name="label">上下文的标签。</param>
		/// <returns>词法分析器的上下文。</returns>
		public LexerContext DefineContext(string label)
		{
			return lexerContexts.DefineContext(label);
		}
		/// <summary>
		/// 定义一个新的词法分析器的包含型上下文。
		/// </summary>
		/// <returns>词法分析器的上下文。</returns>
		public LexerContext DefineInclusiveContext()
		{
			return lexerContexts.DefineInclusiveContext();
		}
		/// <summary>
		/// 定义一个新的词法分析器的包含型上下文。
		/// </summary>
		/// <param name="label">上下文的标签。</param>
		/// <returns>词法分析器的上下文。</returns>
		public LexerContext DefineInclusiveContext(string label)
		{
			return lexerContexts.DefineInclusiveContext(label);
		}

		#endregion // 上下文

	}
}
