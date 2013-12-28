using System.Collections.Generic;

namespace Cyjb.Compiler.Parser
{
	/// <summary>
	/// 表示 LR 项集。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	internal sealed class LRItemset<T>
		where T : struct
	{
		/// <summary>
		/// 初始化 <see cref="LRItemset&lt;T&gt;"/> 类的新实例。
		/// </summary>
		internal LRItemset()
		{
			this.Items = new LRItemCollection<T>();
			this.Goto = new Dictionary<Symbol<T>, int>();
		}
		/// <summary>
		/// 使用指定的项集合初始化 <see cref="LRItemset&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="items">当前项集中包含的项。</param>
		internal LRItemset(LRItemCollection<T> items)
		{
			this.Items = items;
			this.Goto = new Dictionary<Symbol<T>, int>();
		}
		/// <summary>
		/// 获取当前项集中包含的项。
		/// </summary>
		public LRItemCollection<T> Items { get; private set; }
		/// <summary>
		/// 获取当前项集的 GOTO 表。
		/// </summary>
		public Dictionary<Symbol<T>, int> Goto { get; private set; }
	}
}
