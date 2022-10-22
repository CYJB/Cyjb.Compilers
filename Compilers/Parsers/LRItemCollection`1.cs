using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 LR 项的集合，实现了按集合的内容比较。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class LRItemCollection<T> : ReadOnlyListBase<LRItem<T>>, IEquatable<LRItemCollection<T>>
	where T : struct
{
	/// <summary>
	/// LR 项的列表。
	/// </summary>
	private readonly List<LRItem<T>> items = new();
	/// <summary>
	/// LR 项的字典。
	/// </summary>
	private readonly Dictionary<int, LRItem<T>> itemDict = new();

	/// <summary>
	/// 使用指定的初始集合初始化 <see cref="LRItemCollection{T}"/> 类的新实例。
	/// </summary>
	/// <param name="other">初始的 LR 项集合。</param>
	public LRItemCollection(IEnumerable<LRItem<T>>? other = null)
	{
		if (other != null)
		{
			foreach (LRItem<T> item in other)
			{
				Add(item);
			}
		}
	}

	/// <summary>
	/// 如果指定的 LR 项在当前集合中不存在，则添加到当前集合中。
	/// </summary>
	/// <param name="item">要添加的 LR 项。</param>
	public void Add(LRItem<T> item)
	{
		int key = GetKey(item.Production, item.Index);
		if (!itemDict.ContainsKey(key))
		{
			itemDict[key] = item;
			items.Add(item);
		}
	}

	/// <summary>
	/// 如果指定的 LR 项在当前集合中不存在，则添加到当前集合中。
	/// </summary>
	/// <param name="production">LR 项对应的产生式。</param>
	/// <param name="index">LR 项的定点。</param>
	public void Add(Production<T> production, int index)
	{
		int key = GetKey(production, index);
		if (!itemDict.ContainsKey(key))
		{
			LRItem<T> item = new(production, index);
			itemDict[key] = item;
			items.Add(item);
		}
	}

	/// <summary>
	/// 如果指定的 LR 项在当前集合中存在，则返回当前集合中的实际项；
	/// 否则添加到当前集合中。
	/// </summary>
	/// <param name="production">LR 项对应的产生式。</param>
	/// <param name="index">LR 项的定点。</param>
	/// <return>当前集合中的实际项。</return>
	public LRItem<T> AddOrGet(Production<T> production, int index)
	{
		int key = GetKey(production, index);
		if (!itemDict.TryGetValue(key, out LRItem<T>? item))
		{
			item = new LRItem<T>(production, index);
			itemDict[key] = item;
			items.Add(item);
		}
		return item;
	}

	/// <summary>
	/// 返回索引键。
	/// </summary>
	/// <param name="production">LR 项对应的产生式。</param>
	/// <param name="index">LR 项的定点。</param>
	/// <returns>索引键。</returns>
	private static int GetKey(Production<T> production, int index)
	{
		return (production.Index << 16) | index;
	}

	/// <summary>
	/// 计算当前项集的 LR(1) 闭包。
	/// </summary>
	/// <param name="firstSet">非终结符号的 FIRST 集。</param>
	public void CalculateLR1Closure(Dictionary<Symbol<T>, HashSet<Symbol<T>>> firstSet)
	{
		bool changed = true;
		while (changed)
		{
			changed = false;
			for (int i = 0; i < items.Count; i++)
			{
				LRItem<T> item = items[i];
				// 项的定点右边是非终结符。 
				Symbol<T>? symbol = item.SymbolAtIndex;
				if (symbol == null || symbol.Type != SymbolType.NonTerminal)
				{
					continue;
				}
				int index = item.Index + 1;
				Production<T> production = item.Production;
				HashSet<Symbol<T>> forwards;
				if (index < production.Body.Length)
				{
					forwards = new HashSet<Symbol<T>>();
					Symbol<T> nextSymbol = production.Body[index];
					if (nextSymbol.Type == SymbolType.Terminal)
					{
						forwards.Add(nextSymbol);
					}
					else
					{
						forwards.UnionWith(firstSet[nextSymbol]);
						if (forwards.Contains(Symbol<T>.Epsilon))
						{
							// FIRST 中包含 ε，需要被替换为 item 的向前看符号。
							forwards.Remove(Symbol<T>.Epsilon);
							forwards.UnionWith(item.Forwards);
						}
					}
				}
				else
				{
					forwards = item.Forwards;
				}
				foreach (Production<T> nextProduction in symbol.Productions)
				{
					LRItem<T> newItem = AddOrGet(nextProduction, 0);
					if (newItem.AddForwards(forwards))
					{
						changed = true;
					}
				}
			}
		}
	}

	#region ReadOnlyListBase<LRItem<T>> 成员

	/// <summary>
	/// 获取当前集合包含的元素数。
	/// </summary>
	/// <value>当前集合中包含的元素数。</value>
	public override int Count => items.Count;

	/// <summary>
	/// 返回指定索引处的元素。
	/// </summary>
	/// <param name="index">要返回元素的从零开始的索引。</param>
	/// <returns>位于指定索引处的元素。</returns>
	protected override LRItem<T> GetItemAt(int index)
	{
		return items[index];
	}

	/// <summary>
	/// 确定当前列表中指定对象的索引。
	/// </summary>
	/// <param name="item">要在当前列表中定位的对象。</param>
	/// <returns>如果在当前列表中找到 <paramref name="item"/>，则为该对象的索引；否则为 <c>-1</c>。</returns>
	public override int IndexOf(LRItem<T> item)
	{
		return items.IndexOf(item);
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<LRItem<T>> GetEnumerator()
	{
		return items.GetEnumerator();
	}

	#endregion // ReadOnlyListBase<LRItem<T>> 成员

	#region IEquatable<LRItemCollection> 成员

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public bool Equals(LRItemCollection<T>? other)
	{
		if (other is null)
		{
			return false;
		}
		if (itemDict.Count != other.itemDict.Count)
		{
			return false;
		}
		foreach (var (key, items) in itemDict)
		{
			if (!other.itemDict.TryGetValue(key, out LRItem<T>? otherItems) ||
				!items.Equals(otherItems))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 返回当前对象是否等于另一对象。
	/// </summary>
	/// <param name="obj">要与当前对象进行比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="obj"/>，则为 true；否则为 false。</returns>
	public override bool Equals(object? obj)
	{
		if (obj is LRItemCollection<T> other)
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
		return HashCode.Combine(itemDict.Count, UnorderedHashCode.Combine(itemDict.Values));
	}

	/// <summary>
	/// 返回指定的 <see cref="LRItemCollection{T}"/> 是否相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator ==(LRItemCollection<T>? left, LRItemCollection<T>? right)
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
	/// 返回指定的 <see cref="LRItemCollection{T}"/> 是否不相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator !=(LRItemCollection<T>? left, LRItemCollection<T>? right)
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

	#endregion // IEquatable<LRItemCollection> 成员

}
