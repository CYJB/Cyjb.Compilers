using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cyjb.Compiler.RegularExpressions;
using Cyjb.IO;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示词法分析器的规则。
	/// </summary>
	/// <remarks><para><see cref="LexerRule"/> 类包含了用于构造词法分析器的全部信息，
	/// 一般用于自己构造词法分析器。如果不希望手动构造词法分析器，请使用 
	/// <see cref="Grammar.GetReader(SourceReader)"/> 或 
	/// <see cref="Grammar.GetRejectableReader(SourceReader)"/> 方法，
	/// 得到自动构造的词法分析器。</para>
	/// <para>关于如何构造自己的词法分析器，可以参考我的博文
	/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html">
	/// 《C# 词法分析器（六）构造词法分析器》</see>。</para></remarks>
	/// <seealso cref="Grammar"/>
	/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html">
	/// 《C# 词法分析器（六）构造词法分析器》</seealso>
	[Serializable]
	public sealed class LexerRule
	{
		/// <summary>
		/// 表示词法分析器使用的 DFA 中的死状态。
		/// </summary>
		public const int DeadState = -1;
		/// <summary>
		/// 上下文字典。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Dictionary<string, int> contexts;
		/// <summary>
		/// 词法单元的标识符列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string[] tokenIds;
		/// <summary>
		/// 字符类的数据。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int[] charClass;
		/// <summary>
		/// 终结符对应的动作。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<ReaderController>[] actions;
		/// <summary>
		/// EOF 动作。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<ReaderController>[] eofActions;
		/// <summary>
		/// DFA 的转移。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int[,] transitions;
		/// <summary>
		/// DFA 状态对应的终结符索引，使用 
		/// <see cref="Int32.MaxValue"/> - index 表示向前看符号的头节点。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int[][] symbolIndex;
		/// <summary>
		/// 向前看符号的信息。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int?[] trailing;
		/// <summary>
		/// 向前看符号的类型。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private TrailingType trailingType;
		/// <summary>
		/// 使用指定的语法规则初始化 <see cref="Cyjb.Compiler.Lexer.LexerRule"/> 类的新实例。
		/// </summary>
		/// <param name="grammar">词法分析器使用的语法规则。</param>
		internal LexerRule(Grammar grammar)
		{
			ExceptionHelper.CheckArgumentNull(grammar, "grammar");
			this.contexts = new Dictionary<string, int>();
			foreach (LexerContext context in grammar.Contexts)
			{
				this.contexts.Add(context.Label, context.Index);
			}
			this.tokenIds = new string[grammar.TerminalSymbols.Count];
			foreach (TerminalSymbol sym in grammar.TerminalSymbols)
			{
				tokenIds[sym.Index] = sym.Id;
			}
			FillActions(grammar);
			bool useTrailing;
			FillDfa(grammar, out useTrailing);
			if (useTrailing)
			{
				FillTrailing(grammar);
			}
			else
			{
				this.trailing = null;
				this.trailingType = TrailingType.None;
			}
		}
		/// <summary>
		/// 获取词法分析的上下文字典。
		/// </summary>
		/// <value>字典的键保存了所有的上下文标签，其值是对应的索引。</value>
		public IDictionary<string, int> Contexts { get { return contexts; } }
		/// <summary>
		/// 获取词法单元的标识符列表。
		/// </summary>
		/// <value>词法单元的标识符列表，其长度与终结符的数量 <see cref="SymbolCount"/> 相同。</value>
		public IList<string> TokenIds { get { return tokenIds; } }
		/// <summary>
		/// 获取字符类的数据。
		/// </summary>
		/// <value>字符类的数据，其长度为 <c>65536</c>，保存了每个字符所属的字符类。</value>
		public IList<int> CharClass { get { return charClass; } }
		/// <summary>
		/// 获取终结符对应的动作。
		/// </summary>
		/// <value>终结符对应的动作，其长度与定义的终结符的数量 <see cref="SymbolCount"/> 相同。</value>
		public IList<Action<ReaderController>> Actions { get { return actions; } }
		/// <summary>
		/// 获取 EOF 动作。
		/// </summary>
		/// <value>与文件结束对应的动作，其长度与定义的上下文数量相同。</value>
		public IList<Action<ReaderController>> EofActions { get { return eofActions; } }
		/// <summary>
		/// 获取 DFA 状态对应的终结符索引。
		/// </summary>
		/// <value>DFA 状态对应的终结符索引，其长度与 DFA 的状态数相同，即为 <see cref="Count"/>。
		/// 使用大于零，小于终结符数量 <see cref="SymbolCount"/> 的数表示终结符索引，
		/// 使用 <see cref="Int32.MaxValue"/> - index 表示向前看符号的头节点。</value>
		public IList<IList<int>> SymbolIndex { get { return symbolIndex; } }
		/// <summary>
		/// 获取向前看符号的信息。
		/// </summary>
		/// <value>终结符的向前看信息，其长度与定义的终结符的数量相同。
		/// 其中 <c>null</c> 表示不是向前看符号，正数表示前面长度固定，
		/// 负数表示后面长度固定，<c>0</c> 表示长度不固定。。</value>
		public IList<int?> Trailing { get { return this.trailing; } }
		/// <summary>
		/// 获取向前看符号的类型。
		/// </summary>
		/// <value>指示正则表达式中是否使用了向前看符号，以及向前看符号的类型。</value>
		public TrailingType TrailingType { get { return this.trailingType; } }
		/// <summary>
		/// 获取 DFA 中的状态数。
		/// </summary>
		/// <value>DFA 中的状态数。</value>
		public int Count { get { return this.symbolIndex.Length; } }
		/// <summary>
		/// 获取 DFA 中的字符类数。
		/// </summary>
		/// <value>DFA 中的字符类数。</value>
		public int CharClassCount { get { return this.transitions.GetLength(1); } }
		/// <summary>
		/// 获取定义了的终结符的数量。
		/// </summary>
		/// <value>定义了的终结符的数量。</value>
		public int SymbolCount { get { return this.actions.Length; } }
		/// <summary>
		/// 返回 DFA 中指定状态在指定字符类上的转移。
		/// </summary>
		/// <param name="state">当前状态。</param>
		/// <param name="cc">当前字符类。</param>
		/// <returns>目标状态。</returns>
		public int Transitions(int state, int cc)
		{
			return transitions[state, cc];
		}

		#region 构造词法分析器

		/// <summary>
		/// 填充符号对应的动作。
		/// </summary>
		/// <param name="grammar">词法分析器使用的语法。</param>
		private void FillActions(Grammar grammar)
		{
			int symCnt = grammar.TerminalSymbols.Count;
			this.eofActions = new Action<ReaderController>[this.contexts.Count];
			this.actions = new Action<ReaderController>[symCnt];
			foreach (TerminalSymbol sym in grammar.TerminalSymbols)
			{
				this.actions[sym.Index] = sym.Action;
				if (sym.RegularExpression is EndOfFileExp)
				{
					// 填充相应上下文对应的结束动作。
					foreach (LexerContext context in sym.Context)
					{
						if (this.eofActions[context.Index] == null)
						{
							this.eofActions[context.Index] = sym.Action;
						}
					}
				}
			}
		}
		/// <summary>
		/// 填充 DFA 的数据。
		/// </summary>
		/// <param name="grammar">词法分析器使用的语法。</param>
		/// <param name="useTrailing">是否用到了向前看。</param>
		private void FillDfa(Grammar grammar, out bool useTrailing)
		{
			// 构造 DFA。
			Nfa nfa = BuildNfa(grammar, out useTrailing);
			int headCnt = this.contexts.Count * 2;
			Dfa dfa = nfa.BuildDfa(headCnt);
			dfa.Minimize(headCnt);
			// 获取 DFA 的数据。
			this.charClass = dfa.CharClass.GetCharClassMap();
			int stateCnt = dfa.Count;
			int ccCnt = dfa.CharClass.Count;
			this.transitions = new int[stateCnt, ccCnt];
			this.symbolIndex = new int[stateCnt][];
			for (int i = 0; i < stateCnt; i++)
			{
				DfaState state = dfa[i];
				this.symbolIndex[i] = state.SymbolIndex;
				for (int j = 0; j < ccCnt; j++)
				{
					DfaState target = state[j];
					if (target == null)
					{
						this.transitions[i, j] = DeadState;
					}
					else
					{
						this.transitions[i, j] = target.Index;
					}
				}
			}
		}
		/// <summary>
		/// 填充向前看的数据。
		/// </summary>
		/// <param name="grammar">词法分析器使用的语法。</param>
		private void FillTrailing(Grammar grammar)
		{
			int symCnt = grammar.TerminalSymbols.Count;
			this.trailing = new int?[symCnt];
			bool variableTrailing = false;
			foreach (TerminalSymbol sym in grammar.TerminalSymbols)
			{
				AnchorExp exp = sym.RegularExpression as AnchorExp;
				if (exp == null || exp.TrailingExpression == null)
				{
					trailing[sym.Index] = null;
				}
				else
				{
					int len = exp.TrailingExpression.Length;
					if (len != -1)
					{
						trailing[sym.Index] = -len;
					}
					else
					{
						len = exp.InnerExpression.Length;
						if (len != -1)
						{
							trailing[sym.Index] = len;
						}
						else
						{
							trailing[sym.Index] = 0;
							variableTrailing = true;
						}
					}
				}
			}
			if (variableTrailing)
			{
				this.trailingType = TrailingType.Variable;
			}
			else
			{
				this.trailingType = TrailingType.Fixed;
			}
		}
		/// <summary>
		/// 将正则表达式转换为 NFA。
		/// </summary>
		/// <param name="grammar">语法分析的语法。</param>
		/// <param name="useTrailing">是否使用了向前看符号。</param>
		/// <returns>构造得到的 NFA。</returns>
		private static Nfa BuildNfa(Grammar grammar, out bool useTrailing)
		{
			int contextCnt = grammar.Contexts.Count;
			int symCnt = grammar.TerminalSymbols.Count;
			// 将多个上下文的规则放入一个 NFA 中，但起始状态不同。
			Nfa nfa = new Nfa();
			for (int i = 0; i < contextCnt; i++)
			{
				// 为每个上下文创建自己的起始状态，普通规则的起始状态。
				nfa.NewState();
				// 行首规则的起始状态。
				nfa.NewState();
			}
			useTrailing = false;
			foreach (TerminalSymbol sym in grammar.TerminalSymbols)
			{
				if (sym.RegularExpression is EndOfFileExp)
				{
					continue;
				}
				sym.RegularExpression.BuildNfa(nfa);
				nfa.TailState.SymbolIndex = sym.Index;
				// 是否是行首限定的。
				bool isBeginningOfLine = false;
				AnchorExp anchorExp = sym.RegularExpression as AnchorExp;
				if (anchorExp != null)
				{
					if (anchorExp.BeginningOfLine)
					{
						isBeginningOfLine = true;
					}
					if (anchorExp.TrailingHeadState != null)
					{
						// 设置向前看状态类型。
						anchorExp.TrailingHeadState.SymbolIndex = sym.Index;
						useTrailing = true;
					}
				}
				foreach (LexerContext context in sym.Context)
				{
					if (isBeginningOfLine)
					{
						// 行首限定规则。
						nfa[context.Index * 2 + 1].Add(nfa.HeadState);
					}
					else
					{
						// 普通规则。
						nfa[context.Index * 2].Add(nfa.HeadState);
						nfa[context.Index * 2 + 1].Add(nfa.HeadState);
					}
				}
			}
			return nfa;
		}

		#endregion // 构造词法分析器

	}
}
