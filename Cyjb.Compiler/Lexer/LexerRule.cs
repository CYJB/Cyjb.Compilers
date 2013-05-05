using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Cyjb.Compiler.RegularExpressions;
using Cyjb.IO;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示词法分析器的规则。
	/// </summary>
	[Serializable]
	public sealed class LexerRule
	{
		/// <summary>
		/// 表示词法分析器使用的 DFA 中的死状态。
		/// </summary>
		public const int DeadState = -1;
		/// <summary>
		/// 上下文列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LexerContextCollection contexts;
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
		[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
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
		/// 使用指定的语法初始化 <see cref="Cyjb.Compiler.Lexer.LexerRule"/> 类的新实例。
		/// </summary>
		/// <param name="grammar">词法分析器使用的语法。</param>
		internal LexerRule(Grammar grammar)
		{
			ExceptionHelper.CheckArgumentNull(grammar, "grammar");
			this.contexts = grammar.Contexts;
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
		/// 获取词法分析的上下文。
		/// </summary>
		public LexerContextCollection Contexts { get { return contexts; } }
		/// <summary>
		/// 获取字符类的数据。
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public int[] CharClass { get { return charClass; } }
		/// <summary>
		/// 获取终结符对应的动作。
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public Action<ReaderController>[] Actions { get { return actions; } }
		/// <summary>
		/// 获取 EOF 动作。
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public Action<ReaderController>[] EofActions { get { return eofActions; } }
		/// <summary>
		/// 获取 DFA 的转移表。
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public int[,] Transitions { get { return transitions; } }
		/// <summary>
		/// 获取 DFA 状态对应的终结符索引，使用 
		/// <see cref="Int32.MaxValue"/> - index 表示向前看符号的头节点。
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public int[][] SymbolIndex { get { return symbolIndex; } }
		/// <summary>
		/// 获取向前看符号的信息，<c>null</c> 表示不是向前看符号，正数表示前面长度固定，
		/// 负数表示后面长度固定，<c>0</c> 表示长度不固定。
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public int?[] Trailing { get { return this.trailing; } }
		/// <summary>
		/// 获取向前看符号的类型。
		/// </summary>
		public TrailingType TrailingType { get { return this.trailingType; } }
		/// <summary>
		/// 获取 DFA 中的状态数。
		/// </summary>
		public int Count { get { return this.transitions.GetLength(0); } }
		/// <summary>
		/// 获取 DFA 中的字符类数。
		/// </summary>
		public int CharClassCount { get { return this.transitions.GetLength(1); } }
		/// <summary>
		/// 获取定义的符号数。
		/// </summary>
		public int SymbolCount { get { return this.actions.Length; } }

		#region 构造词法分析器

		/// <summary>
		/// 填充符号对应的动作。
		/// </summary>
		/// <param name="grammar">词法分析器使用的语法。</param>
		private void FillActions(Grammar grammar)
		{
			int symCnt = grammar.TerminalSymbols.Count;
			this.eofActions = new Action<ReaderController>[grammar.Contexts.Count];
			this.actions = new Action<ReaderController>[symCnt];
			for (int i = 0; i < symCnt; i++)
			{
				TerminalSymbol sym = grammar.TerminalSymbols[i];
				this.actions[i] = sym.Action;
				if (sym.RegularExpression is EndOfFileExp)
				{
					// 填充相应上下文对应的结束动作。
					foreach (LexerContext c in sym.Context)
					{
						if (this.eofActions[c.Index] == null)
						{
							this.eofActions[c.Index] = sym.Action;
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
		[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body")]
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
						this.Transitions[i, j] = DeadState;
					}
					else
					{
						this.Transitions[i, j] = target.Index;
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
			for (int i = 0; i < symCnt; i++)
			{
				AnchorExp exp = grammar.TerminalSymbols[i].RegularExpression as AnchorExp;
				if (exp != null && exp.TrailingExpression != null)
				{
					int len = exp.TrailingExpression.Length;
					if (len != -1)
					{
						trailing[i] = -len;
					}
					else
					{
						len = exp.InnerExpression.Length;
						if (len != -1)
						{
							trailing[i] = len;
						}
						else
						{
							trailing[i] = 0;
							variableTrailing = true;
						}
					}
				}
				else
				{
					trailing[i] = null;
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
			for (int i = 0; i < symCnt; i++)
			{
				TerminalSymbol sym = grammar.TerminalSymbols[i];
				if (!(sym.RegularExpression is EndOfFileExp))
				{
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
			}
			return nfa;
		}

		#endregion // 构造词法分析器

		#region 生成词法分析器

		/// <summary>
		/// 返回指定源文件的允许拒绝的词法单元读取器。
		/// </summary>
		/// <param name="source">要读取的源文件。</param>
		/// <returns>指定源文件的词法单元读取器。</returns>
		public TokenReader GetRejectableReader(string source)
		{
			return GetRejectableReader(new SourceReader(new StringReader(source)));
		}
		/// <summary>
		/// 返回指定源文件的词法单元读取器。
		/// </summary>
		/// <param name="source">要读取的源文件。</param>
		/// <returns>指定源文件的词法单元读取器。</returns>
		public TokenReader GetReader(string source)
		{
			return GetReader(new SourceReader(new StringReader(source)));
		}
		/// <summary>
		/// 返回指定源文件的允许拒绝的词法单元读取器。
		/// </summary>
		/// <param name="source">要读取的源文件。</param>
		/// <returns>指定源文件的词法单元读取器。</returns>
		public TokenReader GetRejectableReader(SourceReader source)
		{
			ExceptionHelper.CheckArgumentNull(source, "source");
			switch (this.trailingType)
			{
				case TrailingType.None:
					return new RejectableReader(this, source);
				case TrailingType.Fixed:
				case TrailingType.Variable:
					return new RejectableTrailingReader(this, source);
			}
			return null;
		}
		/// <summary>
		/// 返回指定源文件的词法单元读取器。
		/// </summary>
		/// <param name="source">要读取的源文件。</param>
		/// <returns>指定源文件的词法单元读取器。</returns>
		public TokenReader GetReader(SourceReader source)
		{
			ExceptionHelper.CheckArgumentNull(source, "source");
			switch (this.trailingType)
			{
				case TrailingType.None:
					return new SimpleReader(this, source);
				case TrailingType.Fixed:
					return new FixedTrailingReader(this, source);
				case TrailingType.Variable:
					return new VariableTrailingReader(this, source);
			}
			return null;
		}

		#endregion // 生成词法分析器

	}
}
