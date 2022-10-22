namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示插入词法单元的候选。
/// </summary>
/// <typeparam name="T">语法节点标识符的类型，必须是一个枚举类型。</typeparam>
internal class InsertTokenCandidate<T> : IComparable<InsertTokenCandidate<T>>
	where T : struct
{
	/// <summary>
	/// 要插入的词法单元类型。
	/// </summary>
	private readonly T kind;
	/// <summary>
	/// 指定词法单元对应的动作。
	/// </summary>
	private readonly ParserAction action;

	/// <summary>
	/// 使用指定的词法单元候选信息初始化 <see cref="InsertTokenCandidate{T}"/> 类的新实例。
	/// </summary>
	/// <param name="kind">要插入的词法单元类型。</param>
	/// <param name="action">指定词法单元对应的动作。</param>
	public InsertTokenCandidate(T kind, ParserAction action)
	{
		this.kind = kind;
		this.action = action;
	}

	/// <summary>
	/// 获取要插入的词法单元类型。
	/// </summary>
	public T Kind => kind;

	#region IComparable<InsertTokenCandidate<T>> 成员

	/// <summary>
	/// 将当前对象与同一类型的另一个对象进行比较。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>一个值，指示要比较的对象的相对顺序。</returns>
	public int CompareTo(InsertTokenCandidate<T>? other)
	{
		if (other == null)
		{
			return 1;
		}
		// 先比较动作优先级。
		int cmp = action.Type.CompareTo(other.action.Type);
		if (cmp != 0)
		{
			return cmp;
		}
		// 动作优先级相同时，选择更小的动作 index。
		return action.Index - other.action.Index;
	}

	/// <summary>
	/// 返回一个 <see cref="InsertTokenCandidate{T}"/> 对象是否小于另一个 <see cref="InsertTokenCandidate{T}"/> 对象。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 小于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator <(InsertTokenCandidate<T>? left, InsertTokenCandidate<T>? right)
	{
		if (ReferenceEquals(left, right))
		{
			return false;
		}
		if (left is null)
		{
			return true;
		}
		return left.CompareTo(right) < 0;
	}

	/// <summary>
	/// 返回一个 <see cref="InsertTokenCandidate{T}"/> 对象是否小于等于另一个 <see cref="InsertTokenCandidate{T}"/> 对象。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 小于等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator <=(InsertTokenCandidate<T>? left, InsertTokenCandidate<T>? right)
	{
		if (ReferenceEquals(left, right) || left is null)
		{
			return true;
		}
		return left.CompareTo(right) <= 0;
	}

	/// <summary>
	/// 返回一个 <see cref="InsertTokenCandidate{T}"/> 对象是否大于另一个 <see cref="InsertTokenCandidate{T}"/> 对象。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 大于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator >(InsertTokenCandidate<T>? left, InsertTokenCandidate<T>? right)
	{
		if (ReferenceEquals(left, right) || left is null)
		{
			return false;
		}
		return left.CompareTo(right) > 0;
	}

	/// <summary>
	/// 返回一个 <see cref="InsertTokenCandidate{T}"/> 对象是否大于等于另一个 <see cref="InsertTokenCandidate{T}"/> 对象。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 大于等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator >=(InsertTokenCandidate<T>? left, InsertTokenCandidate<T>? right)
	{
		if (ReferenceEquals(left, right))
		{
			return true;
		}
		if (left is null)
		{
			return false;
		}
		return left.CompareTo(right) >= 0;
	}

	#endregion // IComparable<InsertTokenCandidate<T>> 成员

}
