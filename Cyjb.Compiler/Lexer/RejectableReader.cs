using System.Collections.Generic;
using System.Text;
using Cyjb.IO;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示支持 Reject 动作的词法单元读取器。
	/// </summary>
	internal sealed class RejectableReader : TokenReaderBase
	{
		/// <summary>
		/// 接受状态的堆栈。
		/// </summary>
		private Stack<AcceptState> stateStack = new Stack<AcceptState>();
		/// <summary>
		/// 当前匹配的文本。
		/// </summary>
		private StringBuilder text = new StringBuilder(20);
		/// <summary>
		/// 使用给定的词法分析器信息初始化 <see cref="RejectableReader"/> 类的新实例。
		/// </summary>
		/// <param name="lexerRule">要使用的词法分析器的规则。</param>
		/// <param name="reader">要使用的源文件读取器。</param>
		public RejectableReader(LexerRule lexerRule, SourceReader reader) :
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
				if (state == LexerRule.DeadState)
				{
					// 没有合适的转移，退出。
					break;
				}
				text.Append((char)base.Source.Read());
				IList<int> symbolIndex = base.LexerRule.States[state].SymbolIndex;
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
					int lastIndex = astate.Index;
					// 将文本和流调整到与接受状态匹配的状态。
					text.Length = lastIndex - startIndex;
					DoAction(base.LexerRule.Symbols[acceptState].Action, acceptState, text.ToString());
					if (!base.IsReject)
					{
						Source.Unget(Source.Index - lastIndex);
						Source.Drop();
						return true;
					}
				}
			}
			return false;
		}
	}
}
