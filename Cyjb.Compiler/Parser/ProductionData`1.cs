using System;
using System.Diagnostics;

namespace Cyjb.Compiler.Parser
{
	/// <summary>
	/// 表示语法产生式的数据。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	[Serializable]
	public struct ProductionData<T> : IEquatable<ProductionData<T>>
		where T : struct
	{
		/// <summary>
		/// 产生式头的索引。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int head;
		/// <summary>
		/// 产生式体包含的符号个数。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int bodySize;
		/// <summary>
		/// 产生式对应的动作。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Func<ParserController<T>, object> action;
		/// <summary>
		/// 使用产生式的数据初始化 <see cref="ProductionData&lt;T&gt;"/> 结构的新实例。
		/// </summary>
		/// <param name="head">产生式头的索引。</param>
		/// <param name="bodySize">产生式体包含的符号个数。</param>
		/// <param name="action">产生式对应的动作。</param>
		public ProductionData(int head, int bodySize, Func<ParserController<T>, object> action)
		{
			this.head = head;
			this.bodySize = bodySize;
			this.action = action;
		}
		/// <summary>
		/// 获取产生式头的索引。
		/// </summary>
		/// <value>产生式头的索引。</value>
		public int Head { get { return head; } }
		/// <summary>
		/// 获取产生式体包含的符号个数。
		/// </summary>
		/// <value>产生式体包含的符号个数。</value>
		public int BodySize { get { return bodySize; } }
		/// <summary>
		/// 获取产生式对应的动作。
		/// </summary>
		/// <value>产生式对应的动作。</value>
		public Func<ParserController<T>, object> Action { get { return action; } }
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			return string.Concat(head, "→[", bodySize, "]");
		}

		#region IEquatable<ProductionData<T>> 成员

		/// <summary>
		/// 指示当前对象是否等于同一类型的另一个对象。
		/// </summary>
		/// <param name="other">与此对象进行比较的对象。</param>
		/// <returns>如果当前对象等于 <paramref name="other"/> 参数，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		/// <overloads>
		/// <summary>
		/// 指示当前对象是否等于另一个对象。
		/// </summary>
		/// </overloads>
		public bool Equals(ProductionData<T> other)
		{
			if (object.ReferenceEquals(other, this))
			{
				return true;
			}
			if (this.head != other.head)
			{
				return false;
			}
			if (this.bodySize != other.bodySize)
			{
				return false;
			}
			return this.action == other.action;
		}

		#endregion // IEquatable<ProductionData<T>> 成员

		#region object 成员

		/// <summary>
		/// 确定指定的 <see cref="System.Object"/> 是否等于当前的 <see cref="ProductionData&lt;T&gt;"/>。
		/// </summary>
		/// <param name="obj">与当前的 <see cref="ProductionData&lt;T&gt;"/> 进行比较的 object。</param>
		/// <returns>如果指定的 <see cref="System.Object"/> 等于当前的 <see cref="ProductionData&lt;T&gt;"/>，
		/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is ProductionData<T>))
			{
				return false;
			}
			return this.Equals((ProductionData<T>)obj);
		}

		/// <summary>
		/// 用于 <see cref="ProductionData&lt;T&gt;"/> 类型的哈希函数。
		/// </summary>
		/// <returns>当前 <see cref="ProductionData&lt;T&gt;"/> 的哈希代码。</returns>
		public override int GetHashCode()
		{
			int hashCode = head ^ (bodySize << 5);
			if (action != null)
			{
				hashCode ^= action.GetHashCode();
			}
			return hashCode;
		}

		#endregion // object 成员

		#region 运算符重载

		/// <summary>
		/// 判断两个 <see cref="ProductionData&lt;T&gt;"/> 是否相同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="ProductionData&lt;T&gt;"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="ProductionData&lt;T&gt;"/> 对象。</param>
		/// <returns>如果两个 <see cref="ProductionData&lt;T&gt;"/> 对象相同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator ==(ProductionData<T> obj1, ProductionData<T> obj2)
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
		/// 判断两个 <see cref="ProductionData&lt;T&gt;"/> 是否不同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="ProductionData&lt;T&gt;"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="ProductionData&lt;T&gt;"/> 对象。</param>
		/// <returns>如果两个 <see cref="ProductionData&lt;T&gt;"/> 对象不同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator !=(ProductionData<T> obj1, ProductionData<T> obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion // 运算符重载

	}
}
