using System;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compiler
{
	/// <summary>
	/// 表示终结符或非终结符的集合。
	/// </summary>
	[Serializable]
	public class SymbolCollection<T> : ReadOnlyList<T>
		where T : Symbol
	{
		/// <summary>
		/// 初始化 <see cref="SymbolCollection&lt;T&gt;"/> 类的新实例。
		/// </summary>
		internal SymbolCollection() { }
		/// <summary>
		/// 向当前集合中添加一个符号。
		/// </summary>
		/// <param name="symbol">要添加的符号。</param>
		internal void InternalAdd(T symbol)
		{
			base.InsertItem(base.Count, symbol);
		}

		#region ListBase<T> 成员

		/// <summary>
		/// 确定 <see cref="SymbolCollection&lt;T&gt;"/> 中特定项的索引。
		/// </summary>
		/// <param name="item">要在 <see cref="SymbolCollection&lt;T&gt;"/> 中定位的对象。</param>
		/// <returns>如果在 <see cref="SymbolCollection&lt;T&gt;"/> 中找到 <paramref name="item"/>，
		/// 则为该项的索引；否则为 <c>-1</c>。</returns>
		public override int IndexOf(T item)
		{
			int idx = item.Index;
			if (idx >= 0 && idx < this.Count)
			{
				if (this[idx] == item)
				{
					return idx;
				}
			}
			return -1;
		}

		#endregion // ListBase<T> 成员

	}
}
