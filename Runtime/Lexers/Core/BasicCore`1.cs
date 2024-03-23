namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示基本的词法分析器核心。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class BasicCore<T> : LexerCore<T>
	where T : struct
{
	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="BasicCore{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	public BasicCore(LexerData<T> lexerData, LexerController<T> controller) :
		base(lexerData, controller)
	{ }

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
		while (true)
		{
			state = NextState(state);
			if (state == -1)
			{
				// 没有合适的转移，退出。
				break;
			}
			ArraySegment<int> symbols = data.GetSymbols(state);
			if (symbols.Count > 0)
			{
				lastAccept = symbols[0];
				lastIndex = source.Index;
				// 使用最短匹配时，可以直接返回。
				if (data.UseShortest && data.Terminals[lastAccept].UseShortest)
				{
					break;
				}
			}
		}
		if (lastAccept >= 0)
		{
			// 将流调整到与接受状态匹配的状态。
			source.Index = lastIndex;
			controller.DoAction(start, data.Terminals[lastAccept]);
			return true;
		}
		return false;
	}
}
