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
	/// 接受状态的堆栈。
	/// </summary>
	private readonly Stack<AcceptState> stateStack = new();
	/// <summary>
	/// 候选类型。
	/// </summary>
	private IReadOnlySet<T>? candidates;
	/// <summary>
	/// 当前候选。
	/// </summary>
	private AcceptState curCandidate;
	/// <summary>
	/// 当前候选索引。
	/// </summary>
	private int curCandidateIndex;
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
				for (int i = curCandidateIndex; i < curCandidate.Symbols.Length; i++)
				{
					int acceptState = curCandidate.Symbols[i];
					if (acceptState < 0)
					{
						// 跳过向前看的头状态。
						break;
					}
					var kind = Data.Terminals[acceptState].Kind;
					if (kind.HasValue)
					{
						result.Add(kind.Value);
					}
				}
				result.UnionWith(stateStack.SelectMany(GetCandidates));
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
		stateStack.Clear();
		int startIndex = source.Index;
		while (true)
		{
			state = NextState(state);
			if (state == -1)
			{
				// 没有合适的转移，退出。
				break;
			}
			int[] symbols = Data.States[state].Symbols;
			if (symbols.Length > 0)
			{
				// 将接受状态记录在堆栈中。
				stateStack.Push(new AcceptState(symbols, source.Index));
			}
		}
		// 遍历终结状态，执行相应动作。
		while (stateStack.Count > 0)
		{
			curCandidate = stateStack.Pop();
			curCandidateIndex = 0;
			foreach (int acceptState in curCandidate.Symbols)
			{
				curCandidateIndex++;
				if (acceptState < 0)
				{
					// 跳过向前看的头状态。
					break;
				}
				int lastIndex = curCandidate.Index;
				TerminalData<T> terminal = Data.Terminals[acceptState];
				int? trailing = terminal.Trailing;
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
						int target = -acceptState - 1;
						foreach (AcceptState s in stateStack)
						{
							if (ContainsTrailingHead(s.Symbols, target))
							{
								lastIndex = s.Index;
								break;
							}
						}
					}
				}
				// 将文本和流调整到与接受状态匹配的状态。
				source.Index = lastIndex;
				// 每次都需要清空候选集合，并在使用时重新计算。
				candidates = null;
				Controller.DoAction(Start, terminal);
				if (!Controller.IsReject)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 返回指定的接受状态的符号索引中是否包含特定的向前看头状态。
	/// </summary>
	/// <param name="symbols">接受状态的符号索引。</param>
	/// <param name="target">目标向前看头状态。</param>
	/// <returns>如果包含特定的目标，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	private static bool ContainsTrailingHead(int[] symbols, int target)
	{
		// 在当前状态中查找，从后向前找。
		for (int i = symbols.Length - 1; i >= 0; i--)
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

	/// <summary>
	/// 返回指定状态中的候选类型。
	/// </summary>
	/// <param name="state">要检查的状态。</param>
	/// <returns><paramref name="state"/> 中包含的候选状态。</returns>
	private IEnumerable<T> GetCandidates(AcceptState state)
	{
		foreach (int acceptState in state.Symbols)
		{
			if (acceptState < 0)
			{
				// 跳过向前看的头状态。
				break;
			}
			var kind = Data.Terminals[acceptState].Kind;
			if (kind.HasValue)
			{
				yield return kind.Value;
			}
		}
	}
}
