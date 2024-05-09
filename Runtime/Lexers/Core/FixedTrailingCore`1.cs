namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示支持定长向前看符号的词法分析器核心。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class FixedTrailingCore<T> : LexerCore<T>
	where T : struct
{
	/// <summary>
	/// 词法分析器的数据。
	/// </summary>
	private readonly LexerData<T> lexerData;
	/// <summary>
	/// DFA 的状态列表。
	/// </summary>
	private readonly int[] states;

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="FixedTrailingCore{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	public FixedTrailingCore(LexerData<T> lexerData, LexerController<T> controller) :
		base(lexerData.States, lexerData.Terminals, lexerData.ContainsBeginningOfLine, controller)
	{
		this.lexerData = lexerData;
		states = lexerData.States;
	}

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <param name="state">DFA 的起始状态。</param>
	/// <param name="start">当前词法单元的起始位置。</param>
	/// <returns>词法单元读入是否成功。</returns>
	public override bool NextToken(int state, int start)
	{
		// 最后一次匹配的符号和文本索引。
		int lastAccept = -1, lastIndex = source.Index;
		int symbolStart = 0, symbolEnd = 0;
		while (true)
		{
			state = lexerData.NextState(state, source.Read());
			if (state == -1)
			{
				// 没有合适的转移，退出。
				break;
			}
			// 确定不是向前看的头状态。
			if (lexerData.GetSymbols(state, ref symbolStart, ref symbolEnd) && states[symbolStart] >= 0)
			{
				lastAccept = states[symbolStart];
				lastIndex = source.Index;
				// 使用最短匹配时，可以直接返回。
				if (lexerData.UseShortest && terminalData[lastAccept].UseShortest)
				{
					break;
				}
			}
		}
		if (lastAccept >= 0)
		{
			TerminalData<T> terminal = terminalData[lastAccept];
			if (terminal.Trailing.HasValue)
			{
				// 是向前看状态。
				int index = terminal.Trailing.Value;
				// 将流调整到与接受状态匹配的状态。
				if (index > 0)
				{
					// 前面长度固定。
					lastIndex = start + index;
				}
				else
				{
					// 后面长度固定，注意此时 index 是负数。
					lastIndex += index;
				}
			}
			// 将流调整到与接受状态匹配的状态。
			DoAction(start, lastIndex, terminal);
			return true;
		}
		return false;
	}
}
