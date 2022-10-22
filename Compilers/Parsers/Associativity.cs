namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示终结符的结合性。
/// </summary>
/// <param name="Priority">终结符的优先级。</param>
/// <param name="AssociativeType">结合性的类型。</param>
internal record class Associativity(int Priority, AssociativeType AssociativeType) : IComparable<Associativity>
{

	#region IComparable<Associativity> 成员

	/// <summary>
	/// 将当前对象与同一类型的另一个对象进行比较。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>一个值，指示要比较的对象的相对顺序。</returns>
	public int CompareTo(Associativity? other)
	{
		if (other == null)
		{
			return 1;
		}
		if (Priority != other.Priority)
		{
			return Priority - other.Priority;
		}
		return AssociativeType switch
		{
			AssociativeType.Left => 1,
			AssociativeType.Right => -1,
			_ => 0,
		};
	}

	/// <summary>
	/// 返回一个 <see cref="Associativity"/> 对象是否小于另一个 <see cref="Associativity"/> 对象。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 小于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator <(Associativity? left, Associativity? right)
	{
		if (ReferenceEquals(left, right))
		{
			return false;
		}
		if (left == null)
		{
			return true;
		}
		return left.CompareTo(right) < 0;
	}

	/// <summary>
	/// 返回一个 <see cref="Associativity"/> 对象是否小于等于另一个 <see cref="Associativity"/> 对象。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 小于等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator <=(Associativity? left, Associativity? right)
	{
		if (ReferenceEquals(left, right) || left == null)
		{
			return true;
		}
		return left.CompareTo(right) <= 0;
	}

	/// <summary>
	/// 返回一个 <see cref="Associativity"/> 对象是否大于另一个 <see cref="Associativity"/> 对象。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 大于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator >(Associativity? left, Associativity? right)
	{
		if (ReferenceEquals(left, right) || left == null)
		{
			return false;
		}
		return left.CompareTo(right) > 0;
	}

	/// <summary>
	/// 返回一个 <see cref="Associativity"/> 对象是否大于等于另一个 <see cref="Associativity"/> 对象。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 大于等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator >=(Associativity? left, Associativity? right)
	{
		if (ReferenceEquals(left, right))
		{
			return true;
		}
		if (left == null)
		{
			return false;
		}
		return left.CompareTo(right) >= 0;
	}

	#endregion // IComparable<Associativity> 成员

}
