namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示候选动作。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal class CandidateAction<T>
	where T : struct
{
	/// <summary>
	/// 使用指定的候选信息初始化 <see cref="CandidateAction{T}"/> 类的新实例。
	/// </summary>
	/// <param name="item">候选项。</param>
	/// <param name="action">候选动作。</param>
	public CandidateAction(LRItem<T> item, Action action)
	{
		Item = item;
		Action = action;
	}

	/// <summary>
	/// 获取候选项。
	/// </summary>
	public LRItem<T> Item { get; }
	/// <summary>
	/// 获取候选动作。
	/// </summary>
	public Action Action { get; }
}
