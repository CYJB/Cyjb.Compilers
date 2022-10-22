using System.Diagnostics;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 LR 语法分析器的状态数据。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public class ParserStateData<T>
	where T : struct
{
	/// <summary>
	/// 当前状态的动作字典。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IReadOnlyDictionary<T, ParserAction> actions;
	/// <summary>
	/// 当前状态的默认动作。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly ParserAction defaultAction;

	/// <summary>
	/// 使用指定的动作字典和默认动作初始化 <see cref="ParserStateData{T}"/> 类的新实例。
	/// </summary>
	/// <param name="actions">动作字典。</param>
	/// <param name="defaultAction">默认动作。</param>
	/// <param name="expecting">预期词法单元类型集合。</param>
	/// <param name="recoverProduction">用于状态恢复的产生式。</param>
	/// <param name="recoverIndex">用于错误恢复的产生式定点。</param>
	/// <param name="followBaseIndex">用于错误恢复的 FOLLOW 集基索引。</param>
	public ParserStateData(IReadOnlyDictionary<T, ParserAction> actions, ParserAction defaultAction,
		IReadOnlySet<T> expecting, ProductionData<T> recoverProduction, int recoverIndex, int followBaseIndex)
	{
		this.actions = actions;
		this.defaultAction = defaultAction;
		Expecting = expecting;
		RecoverProduction = recoverProduction;
		RecoverIndex = recoverIndex;
		FollowBaseIndex = followBaseIndex;
	}

	/// <summary>
	/// 获取当前状态的动作字典。
	/// </summary>
	public IReadOnlyDictionary<T, ParserAction> Actions => actions;
	/// <summary>
	/// 获取当前状态的默认动作。
	/// </summary>
	public ParserAction DefaultAction => defaultAction;
	/// <summary>
	/// 获取当前状态的预期词法单元类型集合。
	/// </summary>
	public IReadOnlySet<T> Expecting { get; }
	/// <summary>
	/// 获取当前状态用于错误恢复的产生式。
	/// </summary>
	public ProductionData<T> RecoverProduction { get; }
	/// <summary>
	/// 获取当前状态用于错误恢复的产生式定点。
	/// </summary>
	public int RecoverIndex { get; }
	/// <summary>
	/// 获取当前状态用于错误恢复的 FOLLOW 集基索引。
	/// </summary>
	public int FollowBaseIndex { get; }

	/// <summary>
	/// 返回与指定终结符关联的动作。
	/// </summary>
	/// <param name="kind">终结符类型。</param>
	/// <returns>与其关联的动作。</returns>
	public ParserAction GetAction(T kind)
	{
		if (actions.TryGetValue(kind, out ParserAction action))
		{
			return action;
		}
		else
		{
			return defaultAction;
		}
	}
}
