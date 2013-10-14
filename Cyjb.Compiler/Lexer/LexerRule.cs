using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Cyjb.Compiler.RegularExpressions;
using Cyjb.IO;
using Cyjb.Text;

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
		/// EOF 动作。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<ReaderController>[] eofActions;
		/// <summary>
		/// 词法分析器中定义的上下文数量。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int contextCount;
		/// <summary>
		/// 词法分析器的终结符列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private SymbolData[] symbols;
		/// <summary>
		/// 词法分析器中定义的终结符数量。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int symbolCount;
		/// <summary>
		/// 词法分析器的 DFA 状态列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private StateData[] states;
		/// <summary>
		/// 词法分析器的 DFA 状态数量。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int stateCount;
		/// <summary>
		/// 词法分析器使用的字符类数量。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int charClassCount;
		/// <summary>
		/// 字符类的数据。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int[] charClass;
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
			this.contextCount = grammar.Contexts.Count;
			this.contexts = new Dictionary<string, int>(contextCount);
			foreach (LexerContext context in grammar.Contexts)
			{
				this.contexts.Add(context.Label, context.Index);
			}
			this.symbolCount = grammar.TerminalSymbols.Count;
			this.symbols = new SymbolData[this.symbolCount];
			foreach (TerminalSymbol sym in grammar.TerminalSymbols)
			{
				this.symbols[sym.Index] = new SymbolData(sym.Id, sym.Action);
			}
			FillEOFActions(grammar);
			bool useTrailing;
			FillDfa(grammar, out useTrailing);
			if (useTrailing)
			{
				FillTrailing(grammar);
			}
			else
			{
				this.trailingType = TrailingType.None;
			}
		}
		/// <summary>
		/// 获取词法分析的上下文字典。
		/// </summary>
		/// <value>字典的键保存了所有的上下文标签，其值是对应的索引。</value>
		public IDictionary<string, int> Contexts { get { return contexts; } }
		/// <summary>
		/// 获取 EOF 动作。
		/// </summary>
		/// <value>与文件结束对应的动作，其长度与定义的上下文数量 <see cref="ContextCount"/> 相同。</value>
		public IList<Action<ReaderController>> EofActions { get { return eofActions; } }
		/// <summary>
		/// 获取词法分析器中定义的上下文数量。
		/// </summary>
		/// <value>词法分析器中定义的上下文数量。</value>
		public int ContextCount { get { return contextCount; } }
		/// <summary>
		/// 获取词法分析器的终结符列表。
		/// </summary>
		/// <value>词法分析器的终结符列表，
		/// 其长度与词法分析器中定义的终结符数量 <see cref="SymbolCount"/> 相同。</value>
		public IList<SymbolData> Symbols { get { return symbols; } }
		/// <summary>
		/// 获取词法分析器中定义的终结符数量。
		/// </summary>
		/// <value>词法分析器中定义的终结符数量。</value>
		public int SymbolCount { get { return symbolCount; } }
		/// <summary>
		/// 获取词法分析器的 DFA 状态列表。
		/// </summary>
		/// <value>词法分析器的 DFA 状态列表，
		/// 其长度与词法分析器中定义的 DFA 状态数量 <see cref="StateCount"/> 相同。</value>
		public IList<StateData> States { get { return states; } }
		/// <summary>
		/// 获取词法分析器的 DFA 状态数量。
		/// </summary>
		/// <value>词法分析器的 DFA 状态数量。</value>
		public int StateCount { get { return stateCount; } }
		/// <summary>
		/// 获取词法分析器使用的字符类数量。
		/// </summary>
		/// <value>词法分析器使用的字符类数量。</value>
		public int CharClassCount { get { return charClassCount; } }
		/// <summary>
		/// 获取字符类的数据。
		/// </summary>
		/// <value>字符类的数据，其长度为 <c>65536</c>，保存了每个字符所属的字符类。</value>
		public IList<int> CharClass { get { return charClass; } }
		/// <summary>
		/// 获取向前看符号的类型。
		/// </summary>
		/// <value>指示正则表达式中是否使用了向前看符号，以及向前看符号的类型。</value>
		public TrailingType TrailingType { get { return this.trailingType; } }

		#region 构造词法分析器

		/// <summary>
		/// 填充 EOF 动作。
		/// </summary>
		/// <param name="grammar">词法分析器使用的语法。</param>
		private void FillEOFActions(Grammar grammar)
		{
			this.eofActions = new Action<ReaderController>[this.contextCount];
			foreach (TerminalSymbol sym in grammar.TerminalSymbols)
			{
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
			int headCnt = this.contextCount * 2;
			Dfa dfa = nfa.BuildDfa(headCnt);
			dfa.Minimize(headCnt);
			// 获取 DFA 的数据。
			this.charClass = dfa.CharClass.GetCharClassMap();
			this.stateCount = dfa.Count;
			this.states = new StateData[stateCount];
			this.charClassCount = dfa.CharClass.Count;
			for (int i = 0; i < this.stateCount; i++)
			{
				DfaState state = dfa[i];
				int[] transitions = new int[charClassCount];
				for (int j = 0; j < charClassCount; j++)
				{
					DfaState target = state[j];
					if (target == null)
					{
						transitions[j] = DeadState;
					}
					else
					{
						transitions[j] = target.Index;
					}
				}
				states[i] = new StateData(transitions, state.SymbolIndex);
			}
		}
		/// <summary>
		/// 填充向前看的数据。
		/// </summary>
		/// <param name="grammar">词法分析器使用的语法。</param>
		private void FillTrailing(Grammar grammar)
		{
			bool variableTrailing = false;
			foreach (TerminalSymbol sym in grammar.TerminalSymbols)
			{
				AnchorExp exp = sym.RegularExpression as AnchorExp;
				if (exp != null && exp.TrailingExpression != null)
				{
					int len = exp.TrailingExpression.Length;
					if (len != -1)
					{
						this.symbols[sym.Index].Trailing = -len;
					}
					else
					{
						len = exp.InnerExpression.Length;
						if (len != -1)
						{
							this.symbols[sym.Index].Trailing = len;
						}
						else
						{
							this.symbols[sym.Index].Trailing = 0;
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

		#region 获取词法单元读取器

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
		public TokenReader GetReader(string source)
		{
			ExceptionHelper.CheckArgumentNull(source, "source");
			return GetReader(new SourceReader(new StringReader(source)));
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
					return new RejectableTrailingReader(this, source);
			}
			return null;
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
		public TokenReader GetRejectableReader(string source)
		{
			ExceptionHelper.CheckArgumentNull(source, "source");
			return GetRejectableReader(new SourceReader(new StringReader(source)));
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

		#endregion // 获取词法单元读取器

	}
}
