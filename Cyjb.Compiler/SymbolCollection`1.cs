using System;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compiler
{
	/// <summary>
	/// 表示终结符或非终结符的集合。
	/// </summary>
	/// <typeparam name="T">终结符或非终结符的类型。</typeparam>
	[Serializable]
	public class SymbolCollection<T> : KeyedCollectionBase<string, T>
		where T : Symbol
	{
		/// <summary>
		/// 初始化 <see cref="SymbolCollection&lt;T&gt;"/> 类的新实例。
		/// </summary>
		internal SymbolCollection() : base(true) { }
		/// <summary>
		/// 向当前集合中添加一个符号。
		/// </summary>
		/// <param name="symbol">要添加的符号。</param>
		internal void InternalAdd(T symbol)
		{
			base.AddItem(symbol);
		}

		#region KeyedCollectionBase<string, T> 成员

		/// <summary>
		/// 从指定元素提取键。
		/// </summary>
		/// <param name="item">从中提取键的元素。</param>
		/// <returns>指定元素的键。</returns>
		protected override string GetKeyForItem(T item)
		{
			return item.Id;
		}

		#endregion // KeyedCollectionBase<string, T> 成员

	}
}
