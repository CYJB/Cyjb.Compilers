using System;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示词法分析中终结符的数据。
	/// </summary>
	[Serializable]
	public struct SymbolData : IEquatable<SymbolData>
	{
		/// <summary>
		/// 终结符的标识符。
		/// </summary>
		private string id;
		/// <summary>
		/// 终结符的动作。
		/// </summary>
		private Action<ReaderController> action;
		/// <summary>
		/// 终结符的向前看信息。
		/// </summary>
		/// <remarks><c>null</c> 表示不是向前看符号，正数表示前面长度固定，
		/// 负数表示后面长度固定，<c>0</c> 表示长度不固定。
		/// </remarks>
		private int? trailing;
		/// <summary>
		/// 使用指定的标识符和动作初始化 <see cref="SymbolData"/> 结构的新实例。
		/// </summary>
		/// <param name="id">终结符的标识符。</param>
		/// <param name="action">终结符的动作。</param>
		internal SymbolData(string id, Action<ReaderController> action)
		{
			this.id = id;
			this.action = action;
			this.trailing = null;
		}
		/// <summary>
		/// 获取终结符的标识符。
		/// </summary>
		/// <value>终结符的标识符。</value>
		public string Id { get { return this.id; } }
		/// <summary>
		/// 获取终结符的动作。
		/// </summary>
		/// <value>终结符的动作。</value>
		public Action<ReaderController> Action { get { return this.action; } }
		/// <summary>
		/// 获取终结符的向前看信息。
		/// </summary>
		/// <value><c>null</c> 表示不是向前看符号，正数表示前面长度固定，
		/// 负数表示后面长度固定，<c>0</c> 表示长度不固定。
		/// </value>
		public int? Trailing
		{
			get { return this.trailing; }
			internal set { this.trailing = value; }
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			if (Id == null)
			{
				return "[]";
			}
			string str = string.Concat("[", Id, "]");
			if (Trailing != null)
			{
				str += " " + Trailing;
			}
			return str;
		}

		#region IEquatable<SymbolData> 成员

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
		public bool Equals(SymbolData other)
		{
			if (object.ReferenceEquals(other, this))
			{
				return true;
			}
			if (this.id != other.id)
			{
				return false;
			}
			if (this.action != other.action)
			{
				return false;
			}
			return this.trailing == other.trailing;
		}

		#endregion // IEquatable<SymbolData> 成员

		#region object 成员

		/// <summary>
		/// 确定指定的 <see cref="System.Object"/> 是否等于当前的 <see cref="SymbolData"/>。
		/// </summary>
		/// <param name="obj">与当前的 <see cref="SymbolData"/> 进行比较的 object。</param>
		/// <returns>如果指定的 <see cref="System.Object"/> 等于当前的 <see cref="SymbolData"/>，
		/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is SymbolData))
			{
				return false;
			}
			return this.Equals((SymbolData)obj);
		}
		/// <summary>
		/// 用于 <see cref="SymbolData"/> 类型的哈希函数。
		/// </summary>
		/// <returns>当前 <see cref="SymbolData"/> 的哈希代码。</returns>
		public override int GetHashCode()
		{
			int hashCode = this.id.GetHashCode();
			if (this.action != null)
			{
				hashCode ^= this.action.GetHashCode();
			}
			if (this.trailing != null)
			{
				hashCode ^= this.trailing.GetHashCode();
			}
			return hashCode;
		}

		#endregion // object 成员

		#region 运算符重载

		/// <summary>
		/// 判断两个 <see cref="SymbolData"/> 是否相同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="SymbolData"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="SymbolData"/> 对象。</param>
		/// <returns>如果两个 <see cref="SymbolData"/> 对象相同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator ==(SymbolData obj1, SymbolData obj2)
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
		/// 判断两个 <see cref="SymbolData"/> 是否不同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="SymbolData"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="SymbolData"/> 对象。</param>
		/// <returns>如果两个 <see cref="SymbolData"/> 对象不同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator !=(SymbolData obj1, SymbolData obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion // 运算符重载

	}
}
