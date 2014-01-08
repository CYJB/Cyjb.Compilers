using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Cyjb.Compilers.Parsers
{
	/// <summary>
	/// 表示 LR 的项，项的相等比较仅考虑产生式和定点，不会考虑向前看符号。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	internal sealed class LRItem<T> : IEquatable<LRItem<T>>
		where T : struct
	{
		/// <summary>
		/// 项对应的产生式。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Production<T> production;
		/// <summary>
		/// 项的定点。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int index;
		/// <summary>
		/// 向前看符号集合。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private HashSet<Terminal<T>> forwards = new HashSet<Terminal<T>>();
		/// <summary>
		/// 使用给定的语法产生式和定点初始化 <see cref="LRItem&lt;T&gt;"/> 结构的新实例。
		/// </summary>
		/// <param name="production">语法产生式。</param>
		/// <param name="index">项的定点。</param>
		public LRItem(Production<T> production, int index)
		{
			this.production = production;
			this.index = index;
		}
		/// <summary>
		/// 获取项对应的产生式。
		/// </summary>
		public Production<T> Production { get { return production; } }
		/// <summary>
		/// 获取项的定点。
		/// </summary>
		public int Index { get { return index; } }
		/// <summary>
		/// 获取向前看符号集合。
		/// </summary>
		public HashSet<Terminal<T>> Forwards { get { return this.forwards; } }
		/// <summary>
		/// 获取定点右边的符号。
		/// </summary>
		/// <value>定点右边的符号。如果不存在，则为 <c>null</c>。</value>
		public Symbol<T> SymbolAtIndex
		{
			get
			{
				if (index < production.Body.Count)
				{
					return production.Body[index];
				}
				return null;
			}
		}
		/// <summary>
		/// 添加指定的向前看符号。
		/// </summary>
		/// <param name="forwardSymbols">要添加的向前看符号集合。</param>
		/// <returns>如果成功添加了一个或多个向前看符号，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public bool AddForwards(IEnumerable<Terminal<T>> forwardSymbols)
		{
			bool changed = false;
			foreach (Terminal<T> sym in forwardSymbols)
			{
				if (this.forwards.Add(sym) && !changed)
				{
					changed = true;
				}
			}
			return changed;
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			StringBuilder text = new StringBuilder();
			text.Append(production.Index);
			text.Append(' ');
			if (EqualityComparer<T>.Default.Equals(production.Head.Id, Symbol<T>.Invalid))
			{
				text.Append(Constants.AugmentedStartLabel);
			}
			else
			{
				text.Append(production.Head);
			}
			text.Append(" → ");
			int cnt = production.Body.Count;
			if (cnt == 0)
			{
				text.Append("·");
			}
			else
			{
				for (int i = 0; i < cnt; i++)
				{
					if (i > 0)
					{
						text.Append(" ");
					}
					if (index == i)
					{
						text.Append("·");
					}
					text.Append(production.Body[i].ToString());
				}
				if (index == cnt)
				{
					text.Append("·");
				}
			}
			if (index >= production.Body.Count && forwards.Count > 0)
			{
				text.Append(" [");
				foreach (Terminal<T> sym in forwards)
				{
					text.Append(sym.ToString());
					text.Append(", ");
				}
				text.Length -= 2;
				text.Append(']');
			}
			return text.ToString();
		}

		#region IEquatable<LalrItem<T>> 成员

		/// <summary>
		/// 指示当前对象是否等于同一类型的另一个对象。
		/// </summary>
		/// <param name="other">与此对象进行比较的对象。</param>
		/// <returns>如果当前对象等于 <paramref name="other"/> 参数，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public bool Equals(LRItem<T> other)
		{
			if (other == null)
			{
				return false;
			}
			if (object.ReferenceEquals(other, this))
			{
				return true;
			}
			if (production != other.production)
			{
				return false;
			}
			return index == other.index;
		}

		#endregion // IEquatable<LalrItem<T>> 成员

		#region object 成员

		/// <summary>
		/// 确定指定的 <see cref="System.Object"/> 是否等于当前的 <see cref="LRItem&lt;T&gt;"/>。
		/// </summary>
		/// <param name="obj">与当前的 <see cref="LRItem&lt;T&gt;"/> 进行比较的 object。</param>
		/// <returns>如果指定的 <see cref="System.Object"/> 等于当前的 <see cref="LRItem&lt;T&gt;"/>，
		/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(object obj)
		{
			LRItem<T> item = obj as LRItem<T>;
			if (item == null)
			{
				return false;
			}
			return this.Equals(item);
		}

		/// <summary>
		/// 用于 <see cref="LRItem&lt;T&gt;"/> 类型的哈希函数。
		/// </summary>
		/// <returns>当前 <see cref="LRItem&lt;T&gt;"/> 的哈希代码。</returns>
		public override int GetHashCode()
		{
			return production.GetHashCode() ^ (index << 16);
		}

		#endregion // object 成员

		#region 运算符重载

		/// <summary>
		/// 判断两个 <see cref="LRItem&lt;T&gt;"/> 是否相同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="LRItem&lt;T&gt;"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="LRItem&lt;T&gt;"/> 对象。</param>
		/// <returns>如果两个 <see cref="LRItem&lt;T&gt;"/> 对象相同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator ==(LRItem<T> obj1, LRItem<T> obj2)
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
		/// 判断两个 <see cref="LRItem&lt;T&gt;"/> 是否不同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="LRItem&lt;T&gt;"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="LRItem&lt;T&gt;"/> 对象。</param>
		/// <returns>如果两个 <see cref="LRItem&lt;T&gt;"/> 对象不同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator !=(LRItem<T> obj1, LRItem<T> obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion // 运算符重载

	}
}
