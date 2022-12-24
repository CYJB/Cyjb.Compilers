using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示支持定长向前看符号的词法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class TokenizerFixedTrailing<T> : TokenizerBase<T>
	where T : struct
{
	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="TokenizerFixedTrailing{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	/// <param name="reader">要使用的源文件读取器。</param>
	public TokenizerFixedTrailing(LexerData<T> lexerData, LexerController<T> controller, SourceReader reader) :
		base(lexerData, controller, reader)
	{ }

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <param name="state">DFA 的起始状态。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected override bool NextToken(int state)
	{
		// 最后一次匹配的符号和文本索引。
		int lastAccept = -1, lastIndex = source.Index;
		while (true)
		{
			state = NextState(state);
			if (state == -1)
			{
				// 没有合适的转移，退出。
				break;
			}
			int[] symbols = Data.States[state].Symbols;
			// 确定不是向前看的头状态。
			if (symbols.Length > 0 && symbols[0] >= 0)
			{
				lastAccept = symbols[0];
				lastIndex = source.Index;
				// 使用最短匹配时，可以直接返回。
				if (Data.UseShortest && Data.Terminals[lastAccept].UseShortest)
				{
					break;
				}
			}
		}
		if (lastAccept >= 0)
		{
			TerminalData<T> terminal = Data.Terminals[lastAccept];
			if (terminal.Trailing.HasValue)
			{
				// 是向前看状态。
				int index = terminal.Trailing.Value;
				// 将流调整到与接受状态匹配的状态。
				if (index > 0)
				{
					// 前面长度固定。
					lastIndex = Start + index;
				}
				else
				{
					// 后面长度固定，注意此时 index 是负数。
					lastIndex += index;
				}
			}
			// 将流调整到与接受状态匹配的状态。
			source.Index = lastIndex;
			Controller.DoAction(Start, terminal);
			return true;
		}
		return false;
	}
}
