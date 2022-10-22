using System.Text;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 LR 的项，项的相等比较仅考虑产生式和定点，不会考虑向前看符号。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal class LRItem<T> : IEquatable<LRItem<T>>
	where T : struct
{
	/// <summary>
	/// 使用指定的产生式和定点初始化 <see cref="LRItem{T}"/> 类的新实例。
	/// </summary>
	/// <param name="production">语法产生式。</param>
	/// <param name="index">项的定点。</param>
	public LRItem(Production<T> production, int index)
	{
		Production = production;
		Index = index;
	}

	/// <summary>
	/// 获取语法产生式。
	/// </summary>
	public Production<T> Production { get; }
	/// <summary>
	/// 获取项的定点。
	/// </summary>
	public int Index { get; }
	/// <summary>
	/// 获取当前项是否是接受项。
	/// </summary>
	public bool IsAccept => Index == Production.Body.Length &&
		(Production.Head.StartType is SymbolStartType.AugmentedStartHighPriority or SymbolStartType.AugmentedStartLowPriority);
	/// <summary>
	/// 获取或设置当前是否是高优先级接受项。
	/// </summary>
	public bool IsHighPriorityAccept => Production.Head.StartType == SymbolStartType.AugmentedStartHighPriority;
	/// <summary>
	/// 获取向前看符号集合。
	/// </summary>
	public HashSet<Symbol<T>> Forwards { get; } = new();

	/// <summary>
	/// 获取定点右边的符号。
	/// </summary>
	/// <value>定点右边的符号。如果不存在，则为 <c>null</c>。</value>
	public Symbol<T>? SymbolAtIndex
	{
		get
		{
			if (Index < Production.Body.Length)
			{
				return Production.Body[Index];
			}
			return null;
		}
	}

	/// <summary>
	/// 添加指定的向前看符号。
	/// </summary>
	/// <param name="symbols">要添加的向前看符号集合。</param>
	/// <returns>如果成功添加了一个或多个向前看符号，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public bool AddForwards(IEnumerable<Symbol<T>> symbols)
	{
		int oldCount = Forwards.Count;
		Forwards.UnionWith(symbols);
		return Forwards.Count > oldCount;
	}

	#region IEquatable<LRItem<T>> 成员

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public bool Equals(LRItem<T>? other)
	{
		if (other is null)
		{
			return false;
		}
		return Production == other.Production && Index == other.Index;
	}

	/// <summary>
	/// 返回当前对象是否等于另一对象。
	/// </summary>
	/// <param name="obj">要与当前对象进行比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="obj"/>，则为 true；否则为 false。</returns>
	public override bool Equals(object? obj)
	{
		if (obj is LRItem<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	/// <summary>
	/// 返回当前对象的哈希值。
	/// </summary>
	/// <returns>当前对象的哈希值。</returns>
	public override int GetHashCode()
	{
		return HashCode.Combine(Production, Index);
	}

	/// <summary>
	/// 返回指定的 <see cref="LRItem{T}"/> 是否相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator ==(LRItem<T>? left, LRItem<T>? right)
	{
		if (ReferenceEquals(left, right))
		{
			return true;
		}
		if (left is null)
		{
			return false;
		}
		return left.Equals(right);
	}

	/// <summary>
	/// 返回指定的 <see cref="LRItem{T}"/> 是否不相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator !=(LRItem<T>? left, LRItem<T>? right)
	{
		if (ReferenceEquals(left, right))
		{
			return false;
		}
		if (left is null)
		{
			return true;
		}
		return !left.Equals(right);
	}

	#endregion // IEquatable<LRItem<T>> 成员

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		StringBuilder text = new();
		text.Append(Production.Index);
		text.Append(' ');
		text.Append(Production.Head);
		text.Append(" -> ");
		int len = Production.Body.Length;
		if (len == 0)
		{
			text.Append('ε');
		}
		else
		{
			for (int i = 0; i < len; i++)
			{
				if (i > 0)
				{
					text.Append(' ');
				}
				if (Index == i)
				{
					text.Append('•');
				}
				text.Append(Production.Body[i]);
			}
			if (Index == len)
			{
				text.Append('•');
			}
		}
		return text.ToString();
	}
}
