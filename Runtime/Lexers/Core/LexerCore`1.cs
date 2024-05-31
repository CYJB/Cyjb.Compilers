using System.Runtime.CompilerServices;
using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的核心。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal abstract class LexerCore<T>
	where T : struct
{
	/// <summary>
	/// 创建词法分析器核心。
	/// </summary>
	/// <param name="lexerData">词法分析器数据。</param>
	/// <param name="controller">词法分析器的控制器。</param>
	/// <param name="debug">是否需要打印调试信息。</param>
	/// <returns>词法分析器核心。</returns>
	internal static LexerCore<T> Create(LexerData<T> lexerData, LexerController<T> controller, bool debug)
	{
		if (lexerData.Rejectable)
		{
			if (lexerData.TrailingType == TrailingType.None)
			{
				if (debug)
				{
					return new RejectableDebugCore<T>(lexerData, controller);
				}
				else
				{
					return new RejectableCore<T>(lexerData, controller);
				}
			}
		}
		else
		{
			if (lexerData.TrailingType == TrailingType.None)
			{
				if (debug)
				{
					return new BasicDebugCore<T>(lexerData, controller);
				}
				else
				{
					return new BasicCore<T>(lexerData, controller);
				}
			}
			else if (lexerData.TrailingType == TrailingType.Fixed)
			{
				if (debug)
				{
					return new FixedTrailingDebugCore<T>(lexerData, controller);
				}
				else
				{
					return new FixedTrailingCore<T>(lexerData, controller);
				}
			}
		}
		if (debug)
		{
			return new RejectableTrailingDebugCore<T>(lexerData, controller);
		}
		else
		{
			return new RejectableTrailingCore<T>(lexerData, controller);
		}
	}

	/// <summary>
	/// 当前词法分析器的控制器。
	/// </summary>
	private readonly LexerController<T> controller;
	/// <summary>
	/// 源码读取器。
	/// </summary>
	protected SourceReader source = SourceReader.Empty;
	/// <summary>
	/// 词法分析的终结符列表。
	/// </summary>
	private readonly int[] terminals;
	/// <summary>
	/// 词法分析的终结符数据。
	/// </summary>
	protected readonly TerminalData<T>[] terminalData;
	/// <summary>
	/// 已拒绝的终结符列表。
	/// </summary>
	private readonly HashSet<int> rejectedTerminals = new();
	/// <summary>
	/// 候选终结符的堆栈。
	/// </summary>
	private LexerStateInfo[] terminalStack = new LexerStateInfo[16];
	/// <summary>
	/// 候选终结符的个数。
	/// </summary>
	private int terminalStackCount = 0;
	/// <summary>
	/// 候选类型。
	/// </summary>
	private readonly HashSet<T> candidates = new();
	/// <summary>
	/// 是否需要重新计算候选类型。
	/// </summary>
	private bool isCandidatesValid = false;

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="LexerTokenizer{T}"/> 类的新实例。
	/// </summary>
	/// <param name="terminals">终结符列表。</param>
	/// <param name="terminalData">终结符数据。</param>
	/// <param name="containsBeginningOfLine">是否包含与行首匹配对应的头节点</param>
	/// <param name="controller">词法分析控制器。</param>
	protected LexerCore(int[] terminals, TerminalData<T>[] terminalData,
		bool containsBeginningOfLine, LexerController<T> controller)
	{
		this.terminals = terminals;
		this.terminalData = terminalData;
		ContainsBeginningOfLine = containsBeginningOfLine;
		this.controller = controller;
	}

	/// <summary>
	/// 获取词法分析中是否包含与行首匹配对应的头节点。
	/// </summary>
	/// <value>如果包含与行首匹配对应的头节点，则为 <c>true</c>，包含上下文个数×2 个头节点；
	/// 否则为 <c>false</c>，包含上下文个数个头节点。</value>
	public bool ContainsBeginningOfLine { get; init; }

	/// <summary>
	/// 获取当前词法分析器的控制器。
	/// </summary>
	public LexerController<T> Controller => controller;

	/// <summary>
	/// 获取当前词法分析器剩余的候选类型。
	/// </summary>
	/// <remarks>仅在允许 Reject 动作的词法分析器中，返回剩余的候选类型。</remarks>
	public IReadOnlySet<T> Candidates
	{
		get
		{
			if (!isCandidatesValid)
			{
				isCandidatesValid = true;
				candidates.Clear();
				for (int i = terminalStackCount - 1; i >= 0; i--)
				{
					LexerStateInfo info = terminalStack[i]!;
					for (int j = info.TerminalStart; j < info.TerminalEnd; j++)
					{
						int terminalIndex = terminals[j];
						if (terminalIndex < 0)
						{
							// 跳过向前看的头状态。
							break;
						}
						if (rejectedTerminals.Contains(terminalIndex))
						{
							continue;
						}
						var kind = terminalData[terminalIndex].Kind;
						if (kind.HasValue)
						{
							candidates.Add(kind.Value);
						}
					}
				}
			}
			return candidates;
		}
	}

	/// <summary>
	/// 加载指定的源读取器。
	/// </summary>
	/// <param name="source">要加载的源读取器。</param>
	public void Load(SourceReader source)
	{
		rejectedTerminals.Clear();
		this.source = source;
		controller.Source = source;
	}

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <param name="state">DFA 的起始状态。</param>
	/// <param name="start">当前词法单元的起始位置。</param>
	/// <returns>词法单元读入是否成功。</returns>
	public abstract bool NextToken(int state, int start);

	/// <summary>
	/// 清空终结符堆栈。
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void ClearTerminalStack()
	{
		terminalStackCount = 0;
	}

	/// <summary>
	/// 添加终结符。
	/// </summary>
	/// <param name="startIndex">终结符的起始索引。</param>
	/// <param name="endIndex">终结符的结束索引。</param>
	protected void AddTerminals(int startIndex, int endIndex)
	{
		if (terminalStackCount == terminalStack.Length)
		{
			// 扩容。
			LexerStateInfo[] newStack = new LexerStateInfo[terminalStackCount * 2];
			Array.Copy(terminalStack, newStack, terminalStackCount);
			terminalStack = newStack;
		}
		LexerStateInfo? info = terminalStack[terminalStackCount];
		if (info == null)
		{
			info = new LexerStateInfo();
			terminalStack[terminalStackCount] = info;
		}
		info.SourceIndex = source.Index;
		info.TerminalStart = startIndex;
		info.TerminalEnd = endIndex;
		terminalStackCount++;
	}

	/// <summary>
	/// 执行已添加的终结符动作。
	/// </summary>
	/// <param name="start">当前词法单元的起始位置。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected bool RunTerminals(int start)
	{
		rejectedTerminals.Clear();
		for (; terminalStackCount > 0; terminalStackCount--)
		{
			LexerStateInfo info = terminalStack[terminalStackCount - 1]!;
			int sourceIndex = info.SourceIndex;
			while (info.TerminalStart < info.TerminalEnd)
			{
				int terminalIndex = terminals[info.TerminalStart];
				info.TerminalStart++;
				if (rejectedTerminals.Contains(terminalIndex))
				{
					continue;
				}
				// 每次都需要清空候选集合，并在使用时重新计算。
				isCandidatesValid = false;
				DoAction(start, sourceIndex, terminalData[terminalIndex]);
				if (!controller.IsReject)
				{
					return true;
				}
				if (controller.IsRejectState)
				{
					rejectedTerminals.Add(terminalIndex);
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 执行已添加的终结符动作，并输出调试信息。
	/// </summary>
	/// <param name="start">当前词法单元的起始位置。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected bool RunTerminalsDebug(int start)
	{
		rejectedTerminals.Clear();
		for (; terminalStackCount > 0; terminalStackCount--)
		{
			LexerStateInfo info = terminalStack[terminalStackCount - 1]!;
			int sourceIndex = info.SourceIndex;
			while (info.TerminalStart < info.TerminalEnd)
			{
				int terminalIndex = terminals[info.TerminalStart];
				info.TerminalStart++;
				if (rejectedTerminals.Contains(terminalIndex))
				{
					continue;
				}
				// 每次都需要清空候选集合，并在使用时重新计算。
				isCandidatesValid = false;
				DoAction(start, sourceIndex, terminalData[terminalIndex]);
				if (controller.IsReject)
				{
					Console.WriteLine("  Match rejected {0}..{1} [{2}] {3}",
						start, sourceIndex, terminalIndex, terminalData[terminalIndex].Kind);
				}
				else
				{
					Console.WriteLine("  Match {0}..{1} [{2}] {3}",
						start, sourceIndex, terminalIndex, terminalData[terminalIndex].Kind);
					return true;
				}
				if (controller.IsRejectState)
				{
					rejectedTerminals.Add(terminalIndex);
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 执行已添加的终结符动作。
	/// </summary>
	/// <param name="start">当前词法单元的起始位置。</param>
	/// <param name="startSourceIndex">当前源码的起始位置。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected bool RunTerminalsWithTrailing(int start, int startSourceIndex)
	{
		rejectedTerminals.Clear();
		for (; terminalStackCount > 0; terminalStackCount--)
		{
			LexerStateInfo info = terminalStack[terminalStackCount - 1]!;
			int sourceIndex = info.SourceIndex;
			while (info.TerminalStart < info.TerminalEnd)
			{
				int terminalIndex = terminals[info.TerminalStart];
				info.TerminalStart++;
				if (terminalIndex < 0)
				{
					// 跳过向前看的头状态。
					break;
				}
				if (rejectedTerminals.Contains(terminalIndex))
				{
					continue;
				}
				int endIndex = GetTrailingIndex(terminalIndex, startSourceIndex, sourceIndex);
				// 每次都需要清空候选集合，并在使用时重新计算。
				isCandidatesValid = false;
				DoAction(start, endIndex, terminalData[terminalIndex]);
				if (!controller.IsReject)
				{
					return true;
				}
				if (controller.IsRejectState)
				{
					rejectedTerminals.Add(terminalIndex);
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 执行已添加的终结符动作，并输出调试信息。
	/// </summary>
	/// <param name="start">当前词法单元的起始位置。</param>
	/// <param name="startSourceIndex">当前源码的起始位置。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected bool RunTerminalsDebugWithTrailing(int start, int startSourceIndex)
	{
		rejectedTerminals.Clear();
		for (; terminalStackCount > 0; terminalStackCount--)
		{
			LexerStateInfo info = terminalStack[terminalStackCount - 1]!;
			int sourceIndex = info.SourceIndex;
			while (info.TerminalStart < info.TerminalEnd)
			{
				int terminalIndex = terminals[info.TerminalStart];
				info.TerminalStart++;
				if (terminalIndex < 0)
				{
					// 跳过向前看的头状态。
					break;
				}
				if (rejectedTerminals.Contains(terminalIndex))
				{
					continue;
				}
				int endIndex = GetTrailingIndex(terminalIndex, startSourceIndex, sourceIndex);
				// 每次都需要清空候选集合，并在使用时重新计算。
				isCandidatesValid = false;
				DoAction(start, endIndex, terminalData[terminalIndex]);
				if (controller.IsReject)
				{
					Console.WriteLine("  Match rejected {0}..{1} [{2}] {3}",
						start, endIndex, terminalIndex, terminalData[terminalIndex].Kind);
				}
				else
				{
					Console.WriteLine("  Match {0}..{1} [{2}] {3}",
						start, endIndex, terminalIndex, terminalData[terminalIndex].Kind);
					return true;
				}
				if (controller.IsRejectState)
				{
					rejectedTerminals.Add(terminalIndex);
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 将候选类型设置为空。
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void SetCandidatesEmpty()
	{
		candidates.Clear();
		isCandidatesValid = true;
	}

	/// <summary>
	/// 执行指定的词法单元动作。
	/// </summary>
	/// <param name="startIndex">当前词法单元的起始索引。</param>
	/// <param name="endIndex">当前词法单元的结束索引。</param>
	/// <param name="terminal">终结符数据。</param>
	/// <returns>是否已接受当前词法单元。<c>true</c> 表示已接受当前词法单元，<c>false</c> 表示已拒绝当前词法单元。</returns>
	protected bool DoAction(int startIndex, int endIndex, TerminalData<T> terminal)
	{
		source.Index = endIndex;
		controller.DoAction(startIndex, terminal.Kind, terminal.Value, terminal.Action);
		return !controller.IsReject;
	}

	/// <summary>
	/// 返回根据向前看状态调整的源码位置。
	/// </summary>
	/// <param name="terminalIndex">当前终结符索引。</param>
	/// <param name="startIndex">当前匹配的起始索引。</param>
	/// <param name="sourceIndex">当前源码位置。</param>
	/// <returns>调整后的源码位置。</returns>
	protected int GetTrailingIndex(int terminalIndex, int startIndex, int sourceIndex)
	{
		TerminalData<T> terminal = terminalData[terminalIndex];
		int? trailing = terminal.Trailing;
		if (trailing.HasValue)
		{
			// 是向前看状态。
			int trailingIndex = trailing.Value;
			if (trailingIndex > 0)
			{
				// 前面长度固定。
				sourceIndex = startIndex + trailingIndex;
			}
			else if (trailingIndex < 0)
			{
				// 后面长度固定，注意此时 index 是负数。
				sourceIndex += trailingIndex;
			}
			else
			{
				// 前后长度都不固定，需要沿着堆栈向前找。
				int target = -terminalIndex - 1;
				for (int i = terminalStackCount - 1; i >= 0; i--)
				{
					LexerStateInfo info = terminalStack[i];
					if (ContainsTrailingHead(info, target))
					{
						sourceIndex = info.SourceIndex;
						break;
					}
				}
			}
		}
		return sourceIndex;
	}

	/// <summary>
	/// 返回指定的接受状态的符号索引中是否包含特定的向前看头状态。
	/// </summary>
	/// <param name="state">接受状态。</param>
	/// <param name="target">目标向前看头状态。</param>
	/// <returns>如果包含特定的目标，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	private bool ContainsTrailingHead(LexerStateInfo state, int target)
	{
		// 在当前状态中查找，从后向前找。
		for (int i = state.TerminalEnd - 1; i >= state.TerminalStart; i--)
		{
			int idx = terminals[i];
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
	/// 打印已读取的文本。
	/// </summary>
	protected void PrintReadedText()
	{
		StringView view = source.GetReadedText();
		string text;
		if (view.Length <= 50)
		{
			text = view.ToString();
		}
		else
		{
			text = view[..24] + ".." + view[^24];
		}
		Console.WriteLine("read {0}..{1}: \"{2}\"", source.Index - view.Length, source.Index, text);
	}
}
