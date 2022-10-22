using System.Diagnostics;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 LR 语法分析器的数据。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <remarks><see cref="ParserData{T}"/> 类包含了用于构造 LR 语法分析器的全部信息，
/// 可以用于构造语法分析器。也可以使用默认的语法分析器工厂 <see cref="ParserFactory{T}"/>。</remarks>
public sealed class ParserData<T>
	where T : struct
{
	/// <summary>
	/// 产生式列表。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly ProductionData<T>[] productions;
	/// <summary>
	/// 起始状态集合。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Dictionary<T, int>? startStates;
	/// <summary>
	/// 状态列表。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly ParserStateData<T>[] states;
	/// <summary>
	/// GOTO 表的起始索引。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Dictionary<T, int> gotoMap;
	/// <summary>
	/// GOTO 的下一状态。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly int[] gotoNext;
	/// <summary>
	/// GOTO 的状态检查。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly T[] gotoCheck;
	/// <summary>
	/// FOLLOW 的下一状态。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly int[] followNext;
	/// <summary>
	/// FOLLOW 的状态检查。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly int[] followCheck;

	/// <summary>
	/// 使用指定的语法分析器数据初始化 <see cref="ParserData{T}"/> 类的新实例。
	/// </summary>
	/// <param name="productions">产生式列表。</param>
	/// <param name="startStates">起始状态集合。</param>
	/// <param name="states">状态列表。</param>
	/// <param name="gotoMap">GOTO 表的起始索引。</param>
	/// <param name="gotoNext">GOTO 的下一状态。</param>
	/// <param name="gotoCheck">GOTO 的状态检查。</param>
	/// <param name="followNext">FOLLOW 的下一状态。</param>
	/// <param name="followCheck">FOLLOW 的状态检查。</param>
	public ParserData(ProductionData<T>[] productions, Dictionary<T, int>? startStates,
		ParserStateData<T>[] states, Dictionary<T, int> gotoMap, int[] gotoNext, T[] gotoCheck,
		int[] followNext, int[] followCheck)
	{
		this.productions = productions;
		this.startStates = startStates;
		this.states = states;
		this.gotoMap = gotoMap;
		this.gotoNext = gotoNext;
		this.gotoCheck = gotoCheck;
		this.followNext = followNext;
		this.followCheck = followCheck;
	}

	/// <summary>
	/// 获取产生式列表。
	/// </summary>
	public ProductionData<T>[] Productions => productions;
	/// <summary>
	/// 获取起始状态集合。
	/// </summary>
	public Dictionary<T, int>? StartStates => startStates;
	/// <summary>
	/// 获取状态列表。
	/// </summary>
	public ParserStateData<T>[] States => states;
	/// <summary>
	/// 获取 GOTO 表的起始索引。
	/// </summary>
	public Dictionary<T, int> GotoMap => gotoMap;
	/// <summary>
	/// 获取 GOTO 的下一状态。
	/// </summary>
	public int[] GotoNext => gotoNext;
	/// <summary>
	/// 获取 GOTO 的状态检查。
	/// </summary>
	public T[] GotoCheck => gotoCheck;
	/// <summary>
	/// 获取 FOLLOW 的下一状态。
	/// </summary>
	public int[] FollowNext => followNext;
	/// <summary>
	/// 获取 FOLLOW 的状态检查。
	/// </summary>
	public int[] FollowCheck => followCheck;

	/// <summary>
	/// 获取指定状态在指定终结符上的动作。
	/// </summary>
	/// <param name="state">要检查的状态。</param>
	/// <param name="kind">终结符。</param>
	/// <returns>语法分析动作。</returns>
	public ParserAction GetAction(int state, T kind)
	{
		return states[state].GetAction(kind);
	}

	/// <summary>
	/// 返回指定状态使用指定非终结符转移后的状态。
	/// </summary>
	/// <param name="state">当前状态。</param>
	/// <param name="kind">转移的非终结符。</param>
	/// <returns>转以后的状态，使用 <c>-1</c> 表示没有找到合适的状态。</returns>
	public int Goto(int state, T kind)
	{
		int idx = gotoMap[kind] + state;
		if (idx >= 0 && idx < gotoNext.Length && EqualityComparer<T>.Default.Equals(kind, gotoCheck[idx]))
		{
			return gotoNext[idx];
		}
		return ParserData.InvalidState;
	}

	/// <summary>
	/// 返回指定状态预期的所有词法单元类型。
	/// </summary>
	/// <param name="state">要检查的状态。</param>
	/// <returns>指定状态预期的所有词法单元类型。</returns>
	public IReadOnlySet<T> GetExpecting(int state)
	{
		return states[state].Expecting;
	}

	/// <summary>
	/// 返回指定状态的状态相关 FOLLOW 集合。
	/// </summary>
	/// <param name="state">要检查的状态。</param>
	/// <param name="parentState"><paramref name="state"/> 的父状态。</param>
	/// <returns>指定状态的状态相关 FOLLOW 集合。</returns>
	public IReadOnlySet<T>? GetFollow(int state, int parentState)
	{
		int idx = states[parentState].FollowBaseIndex + state;
		if (idx >= 0 && idx < followNext.Length && parentState == followCheck[idx])
		{
			return states[followNext[idx]].Expecting;
		}
		return null;
	}
}

