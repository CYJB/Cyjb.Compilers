using Cyjb.Collections;
using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示支持全部功能的词法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class TokenizerRejectableTrailing<T> : TokenizerBase<T>
	where T : struct
{
	/// <summary>
	/// 接受符号的堆栈。
	/// </summary>
	private readonly ListStack<ArraySegment<int>> symbolStack = new();
	/// <summary>
	/// 接受索引的堆栈。
	/// </summary>
	private readonly ListStack<int> indexStack = new();
	/// <summary>
	/// 候选类型。
	/// </summary>
	private IReadOnlySet<T>? candidates;
	/// <summary>
	/// 无效的状态列表。
	/// </summary>
	private readonly HashSet<int> invalidStates = new();
	/// <summary>
	/// 当前候选符号。
	/// </summary>
	private ArraySegment<int> curSymbols;

	/// <summary>
	/// 获取当前词法分析器剩余的候选类型。
	/// </summary>
	/// <remarks>仅在允许 Reject 动作的词法分析器中，返回剩余的候选类型。</remarks>
	internal override IReadOnlySet<T> Candidates
	{
		get
		{
			if (candidates == null)
			{
				HashSet<T> result = new();
				// 先添加当前候选
				result.UnionWith(GetCandidates(curSymbols));
				result.UnionWith(symbolStack.SelectMany(GetCandidates));
				candidates = result.AsReadOnly();
			}
			return candidates;
		}
	}

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="TokenizerRejectableTrailing{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	/// <param name="reader">要使用的源文件读取器。</param>
	public TokenizerRejectableTrailing(LexerData<T> lexerData, LexerController<T> controller, SourceReader reader) :
		base(lexerData, controller, reader)
	{ }

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <param name="state">DFA 的起始状态。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected override bool NextToken(int state)
	{
		symbolStack.Clear();
		indexStack.Clear();
		int startIndex = source.Index;
		while (true)
		{
			state = NextState(state);
			if (state == -1)
			{
				// 没有合适的转移，退出。
				break;
			}
			ArraySegment<int> symbols = Data.GetSymbols(state);
			if (symbols.Count > 0)
			{
				if (Data.UseShortest)
				{
					// 保存流的索引，避免被误修改影响后续匹配。
					int originIndex = source.Index;
					// 最短匹配时不需要生成候选列表。
					candidates = SetUtil.Empty<T>();
					// 使用最短匹配时，需要先调用 Action。
					foreach (int acceptState in symbols)
					{
						// 跳过向前看的头状态。
						if (acceptState < 0)
						{
							break;
						}
						var terminal = Data.Terminals[acceptState];
						if (terminal.UseShortest)
						{
							AdjustIndex(acceptState, startIndex, originIndex);
							Controller.DoAction(Start, terminal);
							if (!Controller.IsReject)
							{
								return true;
							}
							source.Index = originIndex;
						}
					}
				}
				// 将接受状态记录在堆栈中。
				symbolStack.Push(symbols);
				indexStack.Push(source.Index);
			}
		}
		// 遍历终结状态，执行相应动作。
		invalidStates.Clear();
		while (symbolStack.Count > 0)
		{
			curSymbols = symbolStack.Pop();
			int index = indexStack.Pop();
			while (curSymbols.Count > 0)
			{
				int acceptState = curSymbols[0];
				curSymbols = curSymbols.Slice(1);
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
				candidates = null;
				Controller.DoAction(Start, Data.Terminals[acceptState]);
				if (!Controller.IsReject)
				{
					return true;
				}
				if (Controller.IsRejectState)
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
		TerminalData<T> terminal = Data.Terminals[state];
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
				for (int i = 0; i < symbolStack.Count; i++)
				{
					if (ContainsTrailingHead(symbolStack[i], target))
					{
						index = indexStack[i];
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
	/// <param name="symbols">接受状态的符号索引。</param>
	/// <param name="target">目标向前看头状态。</param>
	/// <returns>如果包含特定的目标，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	private static bool ContainsTrailingHead(ArraySegment<int> symbols, int target)
	{
		// 在当前状态中查找，从后向前找。
		for (int i = symbols.Count - 1; i >= 0; i--)
		{
			int idx = symbols[i];
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
