using Cyjb.Collections;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示支持 Reject 动作的词法分析器核心。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class RejectableCore<T> : LexerCore<T>
	where T : struct
{
	/// <summary>
	/// 接受符号的堆栈。
	/// </summary>
	private readonly ListStack<ValueTuple<int, int>> symbolStack = new();
	/// <summary>
	/// 接受索引的堆栈。
	/// </summary>
	private readonly Stack<int> indexStack = new();
	/// <summary>
	/// 候选类型。
	/// </summary>
	private readonly HashSet<T> candidates = new();
	/// <summary>
	/// 无效的状态列表。
	/// </summary>
	private readonly HashSet<int> invalidStates = new();
	/// <summary>
	/// 当前候选符号。
	/// </summary>
	private ValueTuple<int, int> curSymbols;
	/// <summary>
	/// 是否需要重新计算候选类型。
	/// </summary>
	private bool isCandidatesValid = false;

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="RejectableCore{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	public RejectableCore(LexerData<T> lexerData, LexerController<T> controller) :
		base(lexerData, controller)
	{ }

	/// <summary>
	/// 获取当前词法分析器剩余的候选类型。
	/// </summary>
	/// <remarks>仅在允许 Reject 动作的词法分析器中，返回剩余的候选类型。</remarks>
	public override IReadOnlySet<T> Candidates
	{
		get
		{
			if (!isCandidatesValid)
			{
				isCandidatesValid = true;
				candidates.Clear();
				int[] states = data.States;
				// 先添加当前候选
				GetCandidates(states, curSymbols, candidates);
				for (int i = 0; i < symbolStack.Count; i++)
				{
					GetCandidates(states, symbolStack[i], candidates);
				}
			}
			return candidates;
		}
	}

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <param name="state">DFA 的起始状态。</param>
	/// <param name="start">当前词法单元的起始位置。</param>
	/// <returns>词法单元读入是否成功。</returns>
	public override bool NextToken(int state, int start)
	{
		symbolStack.Clear();
		indexStack.Clear();
		int[] states = data.States;
		int symbolStart = 0, symbolEnd = 0;
		while (true)
		{
			state = NextState(state);
			if (state == -1)
			{
				// 没有合适的转移，退出。
				break;
			}
			if (data.GetSymbols(state, ref symbolStart, ref symbolEnd))
			{
				if (data.UseShortest)
				{
					// 保存流的索引，避免被误修改影响后续匹配。
					int originIndex = source.Index;
					// 最短匹配时不需要生成候选列表。
					candidates.Clear();
					isCandidatesValid = true;
					// 使用最短匹配时，需要先调用 Action。
					for (int i = symbolStart; i < symbolEnd; i++)
					{
						var terminal = data.Terminals[states[i]];
						if (terminal.UseShortest)
						{
							controller.DoAction(start, terminal);
							if (!controller.IsReject)
							{
								return true;
							}
							source.Index = originIndex;
						}
					}
				}
				// 将接受状态记录在堆栈中。
				symbolStack.Push(new ValueTuple<int, int>(symbolStart, symbolEnd));
				indexStack.Push(source.Index);
			}
		}
		// 遍历终结状态，执行相应动作。
		invalidStates.Clear();
		while (symbolStack.Count > 0)
		{
			curSymbols = symbolStack.Pop();
			int index = indexStack.Pop();
			while (curSymbols.Item1 < curSymbols.Item2)
			{
				int acceptState = states[curSymbols.Item1];
				curSymbols.Item1++;
				if (invalidStates.Contains(acceptState))
				{
					continue;
				}
				// 将文本和流调整到与接受状态匹配的状态。
				source.Index = index;
				// 每次都需要清空候选集合，并在使用时重新计算。
				isCandidatesValid = false;
				controller.DoAction(start, data.Terminals[acceptState]);
				if (!controller.IsReject)
				{
					return true;
				}
				if (controller.IsRejectState)
				{
					invalidStates.Add(acceptState);
				}
			}
		}
		return false;
	}
}
