using System.Collections.Generic;
using Cyjb.IO;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示基本的词法单元读取器。
	/// </summary>
	internal sealed class SimpleReader : TokenReaderBase
	{
		/// <summary>
		/// 使用给定的词法分析器信息初始化 <see cref="SimpleReader"/> 类的新实例。
		/// </summary>
		/// <param name="lexerRule">要使用的词法分析器的规则。</param>
		/// <param name="reader">要使用的源文件读取器。</param>
		public SimpleReader(LexerRule lexerRule, SourceReader reader) :
			base(lexerRule, false, reader)
		{ }
		/// <summary>
		/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
		/// </summary>
		/// <param name="state">DFA 的起始状态。</param>
		/// <returns>词法单元读入是否成功。</returns>
		protected override bool InternalReadToken(int state)
		{
			// 最后一次匹配的符号和文本索引。
			int lastAccept = -1, lastIndex = Source.Index;
			while (true)
			{
				state = TransitionState(state, base.Source.Read());
				if (state == LexerRule.DeadState)
				{
					// 没有合适的转移，退出。
					break;
				}
				IList<int> symbolIndex = base.LexerRule.States[state].SymbolIndex;
				if (symbolIndex.Count > 0)
				{
					lastAccept = symbolIndex[0];
					lastIndex = Source.Index;
				}
			}
			if (lastAccept >= 0)
			{
				// 将流调整到与接受状态匹配的状态。
				Source.Unget(Source.Index - lastIndex);
				DoAction(base.LexerRule.Symbols[lastAccept].Action, lastAccept, Source.Accept());
				return true;
			}
			return false;
		}
	}
}
