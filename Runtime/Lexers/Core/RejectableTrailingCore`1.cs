using Cyjb.Collections;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示支持全部功能的词法分析器核心。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class RejectableTrailingCore<T> : LexerCore<T>
	where T : struct
{
	/// <summary>
	/// 候选状态的堆栈。
	/// </summary>
	private readonly ListStack<StateInfo> stateStack = new();
	/// <summary>
	/// 候选类型。
	/// </summary>
	private readonly HashSet<T> candidates = new();
	/// <summary>
	/// 无效的状态列表。
	/// </summary>
	private readonly HashSet<int> invalidStates = new();
	/// <summary>
	/// 当前候选状态。
	/// </summary>
	private StateInfo curState;
	/// <summary>
	/// 是否需要重新计算候选类型。
	/// </summary>
	private bool isCandidatesValid = false;

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="RejectableTrailingCore{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	public RejectableTrailingCore(LexerData<T> lexerData, LexerController<T> controller) :
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
				GetCandidates(states, curState, candidates);
				for (int i = 0; i < stateStack.Count; i++)
				{
					GetCandidates(states, stateStack[i], candidates);
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
		stateStack.Clear();
		int startIndex = source.Index;
		int symbolStart = 0, symbolEnd = 0;
		int[] states = data.States;
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
						int acceptState = states[i];
						// 跳过向前看的头状态。
						if (acceptState < 0)
						{
							break;
						}
						var terminal = data.Terminals[acceptState];
						if (terminal.UseShortest)
						{
							AdjustIndex(acceptState, startIndex, originIndex);
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
				stateStack.Push(new StateInfo(source.Index, symbolStart, symbolEnd));
			}
		}
		// 遍历终结状态，执行相应动作。
		invalidStates.Clear();
		while (stateStack.Count > 0)
		{
			curState = stateStack.Pop();
			int index = curState.Index;
			while (curState.SymbolStart < curState.SymbolEnd)
			{
				int acceptState = states[curState.SymbolStart];
				curState.SymbolStart++;
				if (acceptState < 0)
				{
					// 跳过向前看的头状态。
					break;
				}
				if (invalidStates.Contains(acceptState))
				{
					continue;
				}
				AdjustIndex(acceptState, startIndex, index);
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

	/// <summary>
	/// 根据向前看状态调整流的位置。
	/// </summary>
	/// <param name="state">当前状态。</param>
	/// <param name="startIndex">当前匹配的起始索引。</param>
	/// <param name="index">当前索引。</param>
	private void AdjustIndex(int state, int startIndex, int index)
	{
		TerminalData<T> terminal = data.Terminals[state];
		int[] states = data.States;
		int? trailing = terminal.Trailing;
		if (trailing.HasValue)
		{
			// 是向前看状态。
			int trailingIndex = trailing.Value;
			if (trailingIndex > 0)
			{
				// 前面长度固定。
				index = startIndex + trailingIndex;
			}
			else if (trailingIndex < 0)
			{
				// 后面长度固定，注意此时 index 是负数。
				index += trailingIndex;
			}
			else
			{
				// 前后长度都不固定，需要沿着堆栈向前找。
				int target = -state - 1;
				for (int i = 0; i < stateStack.Count; i++)
				{
					StateInfo info = stateStack[i];
					if (ContainsTrailingHead(states, info, target))
					{
						index = info.Index;
						break;
					}
				}
			}
		}
		// 将文本和流调整到与接受状态匹配的状态。
		source.Index = index;
	}

	/// <summary>
	/// 返回指定的接受状态的符号索引中是否包含特定的向前看头状态。
	/// </summary>
	/// <param name="states">状态列表。</param>
	/// <param name="state">接受状态。</param>
	/// <param name="target">目标向前看头状态。</param>
	/// <returns>如果包含特定的目标，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	private static bool ContainsTrailingHead(int[] states, StateInfo state, int target)
	{
		// 在当前状态中查找，从后向前找。
		for (int i = state.SymbolEnd - 1; i >= state.SymbolStart; i--)
		{
			int idx = states[i];
			if (idx >= 0)
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
