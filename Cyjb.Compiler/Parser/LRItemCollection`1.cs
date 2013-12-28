using System;
using System.Collections.Generic;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compiler.Parser
{
	/// <summary>
	/// 表示 LR 项的集合，实现了按集合的内容比较。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	internal sealed class LRItemCollection<T> : ListBase<LRItem<T>>, IEquatable<LRItemCollection<T>>
		where T : struct
	{
		/// <summary>
		/// LR 项的字典。
		/// </summary>
		private Dictionary<Production<T>, LRItem<T>[]> itemDict = new Dictionary<Production<T>, LRItem<T>[]>();
		/// <summary>
		/// 初始化 <see cref="LRItemCollection&lt;T&gt;"/> 类的新实例。
		/// </summary>
		public LRItemCollection() : base(true) { }
		/// <summary>
		/// 使用指定的初始集合初始化 <see cref="LRItemCollection&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="other">初始的 LR 项集合。</param>
		public LRItemCollection(IEnumerable<LRItem<T>> other)
			: base(true)
		{
			this.UnionWith(other);
		}
		/// <summary>
		/// 如果指定的 LR 项在当前集合中不存在，则添加到当前集合中。
		/// </summary>
		/// <param name="item">要添加的 LR 项。</param>
		public new void Add(LRItem<T> item)
		{
			LRItem<T>[] items = GetItems(item.Production);
			if (items[item.Index] == null)
			{
				items[item.Index] = item;
				base.InsertItem(base.Count, item);
			}
		}
		/// <summary>
		/// 如果指定的 LR 项在当前集合中不存在，则添加到当前集合中。
		/// </summary>
		/// <param name="production">LR 项对应的产生式。</param>
		/// <param name="index">LR 项的定点。</param>
		public void Add(Production<T> production, int index)
		{
			LRItem<T>[] items = GetItems(production);
			if (items[index] == null)
			{
				LRItem<T> item = new LRItem<T>(production, index);
				items[index] = item;
				base.InsertItem(Count, item);
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
			LRItem<T>[] items = GetItems(production);
			if (items[index] == null)
			{
				LRItem<T> item = new LRItem<T>(production, index);
				items[index] = item;
				; base.InsertItem(Count, item);
			}
			return items[index];
		}
		/// <summary>
		/// 修改当前集，使该集包含当前集和指定集合中同时存在的所有元素。
		/// </summary>
		/// <param name="other">要与当前集进行比较的集合。</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="other"/> 为 <c>null</c>。</exception>
		public void UnionWith(IEnumerable<LRItem<T>> other)
		{
			foreach (LRItem<T> item in other)
			{
				this.Add(item);
			}
		}
		/// <summary>
		/// 返回指定产生式对应的所有 LR 项。
		/// </summary>
		/// <param name="production">要获取 LR 项的产生式。</param>
		/// <return>指定产生式对应的所有 LR 项。</return>
		private LRItem<T>[] GetItems(Production<T> production)
		{
			LRItem<T>[] items;
			if (!this.itemDict.TryGetValue(production, out items))
			{
				items = new LRItem<T>[production.Body.Count + 1];
				this.itemDict.Add(production, items);
			}
			return items;
		}

		#region IEquatable<LalrItemCollection<T>> 成员

		/// <summary>
		/// 指示当前对象是否等于同一类型的另一个对象。
		/// </summary>
		/// <param name="other">与此对象进行比较的对象。</param>
		/// <returns>如果当前对象等于 <c>true</c> 参数，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public bool Equals(LRItemCollection<T> other)
		{
			if (other == null)
			{
				return false;
			}
			if (this.Count != other.Count)
			{
				return false;
			}
			if (this.itemDict.Count != other.itemDict.Count)
			{
				return false;
			}
			foreach (KeyValuePair<Production<T>, LRItem<T>[]> pair in this.itemDict)
			{
				LRItem<T>[] otherItems;
				if (!other.itemDict.TryGetValue(pair.Key, out otherItems))
				{
					return false;
				}
				int cnt = otherItems.Length;
				if (cnt != pair.Value.Length)
				{
					return false;
				}
				for (int i = 0; i < cnt; i++)
				{
					if (otherItems[i] != pair.Value[i])
					{
						return false;
					}
				}
			}
			return true;
		}

		#endregion // IEquatable<LalrItemCollection<T>> 成员

		#region object 成员

		/// <summary>
		/// 确定指定的 <see cref="System.Object"/> 是否等于当前的 <see cref="LRItemCollection&lt;T&gt;"/>。
		/// </summary>
		/// <param name="obj">与当前的 <see cref="LRItemCollection&lt;T&gt;"/> 进行比较的 object。</param>
		/// <returns>如果指定的 <see cref="System.Object"/> 等于当前的 <see cref="LRItemCollection&lt;T&gt;"/>，
		/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(object obj)
		{
			LRItemCollection<T> col = obj as LRItemCollection<T>;
			if (col == null)
			{
				return false;
			}
			return this.Equals(col);
		}

		/// <summary>
		/// 用于 <see cref="LRItemCollection&lt;T&gt;"/> 类型的哈希函数。
		/// </summary>
		/// <returns>当前 <see cref="LRItemCollection&lt;T&gt;"/> 的哈希代码。</returns>
		public override int GetHashCode()
		{
			int hashCode = this.Count;
			foreach (LRItem<T> item in this)
			{
				hashCode ^= item.GetHashCode();
			}
			return hashCode;
		}

		#endregion // object 成员

		#region 运算符重载

		/// <summary>
		/// 判断两个 <see cref="LRItemCollection&lt;T&gt;"/> 是否相同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="LRItemCollection&lt;T&gt;"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="LRItemCollection&lt;T&gt;"/> 对象。</param>
		/// <returns>如果两个 <see cref="LRItemCollection&lt;T&gt;"/> 对象相同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator ==(LRItemCollection<T> obj1, LRItemCollection<T> obj2)
		{
			if (object.ReferenceEquals(obj1, obj2))
			{
				return true;
			}
			if (object.ReferenceEquals(obj1, null))
			{
				return object.ReferenceEquals(obj2, null);
			}
			return obj1.Equals(obj2);
		}

		/// <summary>
		/// 判断两个 <see cref="LRItemCollection&lt;T&gt;"/> 是否不同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="LRItemCollection&lt;T&gt;"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="LRItemCollection&lt;T&gt;"/> 对象。</param>
		/// <returns>如果两个 <see cref="LRItemCollection&lt;T&gt;"/> 对象不同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator !=(LRItemCollection<T> obj1, LRItemCollection<T> obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion // 运算符重载

	}
}
