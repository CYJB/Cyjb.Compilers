using System.Runtime.CompilerServices;
using Cyjb.Collections;
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
	/// <returns>词法分析器核心。</returns>
	public static LexerCore<T> Create(LexerData<T> lexerData, LexerController<T> controller)
	{
		if (lexerData.Rejectable)
		{
			if (lexerData.TrailingType == TrailingType.None)
			{
				return new RejectableCore<T>(lexerData, controller);
			}
		}
		else
		{
			if (lexerData.TrailingType == TrailingType.None)
			{
				return new BasicCore<T>(lexerData, controller);
			}
			else if (lexerData.TrailingType == TrailingType.Fixed)
			{
				return new FixedTrailingCore<T>(lexerData, controller);
			}
		}
		return new RejectableTrailingCore<T>(lexerData, controller);
	}

	/// <summary>
	/// 词法分析器的数据。
	/// </summary>
	protected readonly LexerData<T> data;
	/// <summary>
	/// 当前词法分析器的控制器。
	/// </summary>
	protected readonly LexerController<T> controller;
	/// <summary>
	/// 源码读取器。
	/// </summary>
	protected SourceReader source = SourceReader.Empty;
	/// <summary>
	/// 已拒绝的状态列表。
	/// </summary>
	private readonly HashSet<int> rejectedStates = new();

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="LexerTokenizer{T}"/> 类的新实例。
	/// </summary>
	/// <param name="data">要使用的词法分析器数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	protected LexerCore(LexerData<T> data, LexerController<T> controller)
	{
		this.data = data;
		this.controller = controller;
		controller.SetCore(this, data.Contexts, data.Rejectable);
	}

	/// <summary>
	/// 获取词法分析中是否包含与行首匹配对应的头节点。
	/// </summary>
	/// <value>如果包含与行首匹配对应的头节点，则为 <c>true</c>，包含上下文个数×2 个头节点；
	/// 否则为 <c>false</c>，包含上下文个数个头节点。</value>
	public bool ContainsBeginningOfLine => data.ContainsBeginningOfLine;
	/// <summary>
	/// 获取当前词法分析器剩余的候选类型。
	/// </summary>
	/// <remarks>仅在允许 Reject 动作的词法分析器中，返回剩余的候选类型。</remarks>
	public virtual IReadOnlySet<T> Candidates => SetUtil.Empty<T>();

	/// <summary>
	/// 加载指定的源读取器。
	/// </summary>
	/// <param name="source">要加载的源读取器。</param>
	public void Load(SourceReader source)
	{
		this.source = source;
		// 重置状态。
		rejectedStates.Clear();
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
	/// 使用源文件中的下一个字符转移到后续状态。
	/// </summary>
	/// <param name="state">当前状态索引。</param>
	/// <returns>转以后的状态，使用 <c>-1</c> 表示没有找到合适的状态。</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected int NextState(int state)
	{
		char ch = source.Read();
		return data.NextState(state, ch);
	}

	/// <summary>
	/// 返回指定符号列表中的候选类型。
	/// </summary>
	/// <param name="states">状态列表。</param>
	/// <param name="state">要检查的状态信息。</param>
	/// <param name="candidates">候选状态集合。</param>
	protected void GetCandidates(int[] states, StateInfo state, HashSet<T> candidates)
	{
		for (int i = state.SymbolStart; i < state.SymbolEnd; i++)
		{
			int acceptState = states[i];
			if (acceptState < 0)
			{
				// 跳过向前看的头状态。
				break;
			}
			if (rejectedStates.Contains(acceptState))
			{
				continue;
			}
			var kind = data.Terminals[acceptState].Kind;
			if (kind.HasValue)
			{
				candidates.Add(kind.Value);
			}
		}
	}
}
