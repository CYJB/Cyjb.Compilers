namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示带有调试信息的支持全部功能的词法分析器核心。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class RejectableTrailingDebugCore<T> : LexerCore<T>
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
	/// 使用给定的词法分析器信息初始化 <see cref="RejectableTrailingCore{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	public RejectableTrailingDebugCore(LexerData<T> lexerData, LexerController<T> controller) :
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
		ClearTerminalStack();
		int startIndex = source.Index;
		int symbolStart = 0, symbolEnd = 0;
		while (true)
		{
			state = lexerData.NextState(state, source.Read());
			if (state == -1)
			{
				// 没有合适的转移，退出。
				break;
			}
			if (lexerData.GetSymbols(state, ref symbolStart, ref symbolEnd))
			{
				if (lexerData.UseShortest)
				{
					// 保存流的索引，避免被误修改影响后续匹配。
					int originIndex = source.Index;
					// 使用最短匹配时，需要先调用 Action。
					for (int i = symbolStart; i < symbolEnd; i++)
					{
						int acceptState = states[i];
						// 普通符号会排在向前看符号的前面，这里可以直接退出匹配。
						if (acceptState < 0)
						{
							break;
						}
						var terminal = terminalData[acceptState];
						if (terminal.UseShortest)
						{
							// 最短匹配时不需要生成候选列表。
							SetCandidatesEmpty();
							int endIndex = GetTrailingIndex(acceptState, startIndex, originIndex);
							if (DoAction(start, endIndex, terminal))
							{
								Console.WriteLine("  Match shortest {0}..{1} [{2}] {3}",
									startIndex, endIndex, acceptState, terminal.Kind);
								return true;
							}
							else
							{
								Console.WriteLine("  Match shortest failed {0}..{1} [{2}] {3}",
									startIndex, originIndex, states[i], terminal.Kind);
							}
							symbolStart++;
						}
						else
						{
							// 最短匹配要求手工调整终结符顺序。
							break;
						}
					}
					source.Index = originIndex;
				}
				// 将接受状态记录在堆栈中。
				AddTerminals(symbolStart, symbolEnd);
			}
		}
		PrintReadedText();
		return RunTerminalsDebugWithTrailing(start, startIndex);
	}
}
