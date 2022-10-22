namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 LR 语法分析器的移入动作。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal class ShiftAction<T> : Action
	where T : struct
{
	/// <summary>
	/// 使用指定的状态初始化 <see cref="ShiftAction{T}"/> 类的新实例。
	/// </summary>
	/// <param name="state">要移入的状态。</param>
	public ShiftAction(LRState<T> state)
	{
		State = state;
	}

	/// <summary>
	/// 要移入的状态。
	/// </summary>
	public LRState<T> State { get; }

	/// <summary>
	/// 获取当前动作是否是成功动作。
	/// </summary>
	public override bool IsSuccessAction => true;

	/// <summary>
	/// 返回当前动作对应的 <see cref="ParserAction"/>。
	/// </summary>
	/// <returns>当前动作对应的 <see cref="ParserAction"/>。</returns>
	public override ParserAction ToParserAction()
	{
		return ParserAction.Shift(State.Index);
	}

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Equals(Action? other)
	{
		return other is ShiftAction<T> action && State == action.State;
	}

	/// <summary>
	/// 返回当前对象的哈希值。
	/// </summary>
	/// <returns>当前对象的哈希值。</returns>
	public override int GetHashCode()
	{
		return State.GetHashCode();
	}

	/// <summary>
	/// 返回当前对象的字符串表示。
	/// </summary>
	/// <returns>当前对象的字符串表示。</returns>
	public override string ToString()
	{
		return $"s{State.Index}";
	}
}
