using System.Collections.Generic;
using Cyjb.IO;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示支持变长向前看符号的词法单元读取器。
	/// </summary>
	internal sealed class VariableTrailingReader : TokenReader
	{
		/// <summary>
		/// 接受状态的堆栈。
		/// </summary>
		private Stack<AcceptState> stateStack = new Stack<AcceptState>();
		/// <summary>
		/// 使用给定的词法分析器信息初始化 <see cref="VariableTrailingReader"/> 类的新实例。
		/// </summary>
		/// <param name="lexerRule">要使用的词法分析器的规则。</param>
		/// <param name="reader">要使用的源文件读取器。</param>
		public VariableTrailingReader(LexerRule lexerRule, SourceReader reader) :
			base(lexerRule, false, reader)
		{ }
		/// <summary>
		/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
		/// </summary>
		/// <param name="state">DFA 的起始状态。</param>
		/// <returns>词法单元读入是否成功。</returns>
		protected override bool InternalReadToken(int state)
		{
			stateStack.Clear();
			int startIndex = Source.Index;
			while (true)
			{
				state = TransitionState(state, base.Source.Read());
				if (state == LexerRule.DeadState)
				{
					// 没有合适的转移，退出。
					break;
				}
				int[] symbolIndex = base.LexerRule.SymbolIndex[state];
				if (symbolIndex.Length > 0)
				{
					// 将接受状态记录在堆栈中。
					stateStack.Push(new AcceptState(symbolIndex, Source.Index));
				}
			}
			// 遍历终结状态，执行相应动作。
			while (stateStack.Count > 0)
			{
				AcceptState astate = stateStack.Pop();
				// 确定不是向前看的头状态。
				int acceptState = astate.SymbolIndex[0];
				if (acceptState < base.LexerRule.SymbolCount)
				{
					int lastIndex = astate.Index;
					int? trailing = base.LexerRule.Trailing[acceptState];
					if (trailing.HasValue)
					{
						// 是向前看状态。
						int index = trailing.Value;
						if (index > 0)
						{
							// 前面长度固定。
							lastIndex = startIndex + index;
						}
						else if (index < 0)
						{
							// 后面长度固定，注意此时 index 是负数。
							lastIndex += index;
						}
						else
						{
							// 前后长度都不固定，需要沿着堆栈向前找。
							int target = int.MaxValue - acceptState;
							while (true)
							{
								if (ContainsTrailingHead(astate.SymbolIndex, target))
								{
									lastIndex = astate.Index;
									break;
								}
								astate = stateStack.Pop();
							}
						}
					}
					// 将流调整到与接受状态匹配的状态。
					Source.Unget(Source.Index - lastIndex);
					DoAction(base.LexerRule.Actions[acceptState], acceptState, base.Source.Accept());
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 返回指定的接受状态的符号索引中是否包含特定的向前看头状态。
		/// </summary>
		/// <param name="symbolIndex">接受状态的符号索引。</param>
		/// <param name="target">目标向前看头状态。</param>
		/// <returns>如果包含特定的目标，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private bool ContainsTrailingHead(int[] symbolIndex, int target)
		{
			// 在当前状态中查找，从后向前找。
			for (int i = symbolIndex.Length - 1; i >= 0; i--)
			{
				int idx = symbolIndex[i];
				if (idx < base.LexerRule.SymbolCount)
				{
					// 前面的状态已经不可能是向前看头状态了，所以直接退出。
					break;
				}
				if (idx == target)
				{
					return true;
				}
			}
			return false;
		}
	}
}
