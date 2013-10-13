using System.Collections.Generic;
using System.Text;
using Cyjb.Collections;
using Cyjb.IO;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示支持全部功能的词法单元读取器。
	/// </summary>
	internal sealed class RejectableTrailingReader : TokenReaderBase
	{
		/// <summary>
		/// 接受状态的堆栈。
		/// </summary>
		private ListStack<AcceptState> stateStack = new ListStack<AcceptState>();
		/// <summary>
		/// 当前匹配的文本。
		/// </summary>
		private StringBuilder text = new StringBuilder(20);
		/// <summary>
		/// 使用给定的词法分析器信息初始化 <see cref="RejectableTrailingReader"/> 类的新实例。
		/// </summary>
		/// <param name="lexerRule">要使用的词法分析器的规则。</param>
		/// <param name="reader">要使用的源文件读取器。</param>
		public RejectableTrailingReader(LexerRule lexerRule, SourceReader reader) :
			base(lexerRule, true, reader)
		{ }
		/// <summary>
		/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
		/// </summary>
		/// <param name="state">DFA 的起始状态。</param>
		/// <returns>词法单元读入是否成功。</returns>
		protected override bool InternalReadToken(int state)
		{
			stateStack.Clear();
			text.Clear();
			int startIndex = Source.Index;
			while (true)
			{
				state = TransitionState(state, base.Source.Peek());
				text.Append((char)base.Source.Read());
				if (state == LexerRule.DeadState)
				{
					// 没有合适的转移，退出。
					break;
				}
				IList<int> symbolIndex = base.LexerRule.SymbolIndex[state];
				if (symbolIndex.Count > 0)
				{
					// 将接受状态记录在堆栈中。
					stateStack.Push(new AcceptState(symbolIndex, Source.Index));
				}
			}
			// 遍历终结状态，执行相应动作。
			while (stateStack.Count > 0)
			{
				AcceptState astate = stateStack.Pop();
				for (int i = 0; i < astate.SymbolIndex.Count; i++)
				{
					int acceptState = astate.SymbolIndex[i];
					if (acceptState >= base.LexerRule.SymbolCount)
					{
						// 跳过向前看的头状态。
						break;
					}
					int lastIndex = astate.Index;
					// 保存向前看跳过的文本。
					string savedText = null;
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
							for (int j = stateStack.Count - 1; j >= 0; j--)
							{
								if (ContainsTrailingHead(stateStack[j].SymbolIndex, target))
								{
									lastIndex = stateStack[j].Index;
									break;
								}
							}
						}
						// 这里需要保存字符串状态，以保证可以正确 Reject。
						savedText = text.ToString(lastIndex - startIndex, astate.Index - lastIndex);
					}
					// 将文本和流调整到与接受状态匹配的状态。
					text.Length = lastIndex - startIndex;
					DoAction(base.LexerRule.Actions[acceptState], acceptState, text.ToString());
					if (base.IsReject)
					{
						// 被 Reject，恢复文本和流的状态。
						if (savedText != null)
						{
							text.Append(savedText);
						}
					}
					else
					{
						Source.Unget(Source.Index - lastIndex);
						Source.Drop();
						return true;
					}
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
		private bool ContainsTrailingHead(IList<int> symbolIndex, int target)
		{
			// 在当前状态中查找，从后向前找。
			for (int i = symbolIndex.Count - 1; i >= 0; i--)
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
