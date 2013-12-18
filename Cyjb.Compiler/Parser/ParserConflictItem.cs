using System;

namespace Cyjb.Compiler.Parser
{
	/// <summary>
	/// 表示语法分析器的 LR(1) 冲突项。
	/// </summary>
	[Serializable]
	public struct ParserConflictItem : IEquatable<ParserConflictItem>
	{
		/// <summary>
		/// 产生冲突的产生式。
		/// </summary>
		private string production;
		/// <summary>
		/// 冲突的动作。
		/// </summary>
		private ParseAction action;
		/// <summary>
		/// 使用指定的产生式和动作初始化 <see cref="ParserConflictItem"/> 结构的新实例。
		/// </summary>
		/// <param name="production">产生冲突的产生式。</param>
		/// <param name="action">冲突的动作。</param>
		public ParserConflictItem(string production, ParseAction action)
		{
			this.production = production;
			this.action = action;
		}
		/// <summary>
		/// 获取产生冲突的产生式。
		/// </summary>
		/// <value>产生冲突的产生式。</value>
		public string Production { get { return this.production; } }
		/// <summary>
		/// 获取冲突的动作。
		/// </summary>
		/// <value>冲突的动作。</value>
		public ParseAction Action { get { return this.action; } }
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			return string.Concat(production, " (", action, ")");
		}

		#region IEquatable<ParserConflictItem> 成员

		/// <summary>
		/// 指示当前对象是否等于同一类型的另一个对象。
		/// </summary>
		/// <param name="other">与此对象进行比较的对象。</param>
		/// <returns>如果当前对象等于 <paramref name="other"/> 参数，
		/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
		/// <overloads>
		/// <summary>
		/// 指示当前对象是否等于另一个对象。
		/// </summary>
		/// </overloads>
		public bool Equals(ParserConflictItem other)
		{
			if (object.ReferenceEquals(other, this))
			{
				return true;
			}
			return this.production == other.production && this.action == other.action;
		}

		#endregion // IEquatable<ParserConflictItem> 成员

		#region object 成员

		/// <summary>
		/// 确定指定的 <see cref="System.Object"/> 是否等于当前的 <see cref="ParserConflictItem"/>。
		/// </summary>
		/// <param name="obj">与当前的 <see cref="ParserConflictItem"/> 进行比较的 object。</param>
		/// <returns>如果指定的 <see cref="System.Object"/> 等于当前的 <see cref="ParserConflictItem"/>，
		/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is ParserConflictItem))
			{
				return false;
			}
			return this.Equals((ParserConflictItem)obj);
		}
		/// <summary>
		/// 用于 <see cref="ParserConflictItem"/> 类型的哈希函数。
		/// </summary>
		/// <returns>当前 <see cref="ParserConflictItem"/> 的哈希代码。</returns>
		public override int GetHashCode()
		{
			return this.production.GetHashCode() ^ this.action.GetHashCode();
		}

		#endregion // object 成员

		#region 运算符重载

		/// <summary>
		/// 判断两个 <see cref="ParserConflictItem"/> 是否相同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="ParserConflictItem"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="ParserConflictItem"/> 对象。</param>
		/// <returns>如果两个 <see cref="ParserConflictItem"/> 对象相同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator ==(ParserConflictItem obj1, ParserConflictItem obj2)
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
		/// 判断两个 <see cref="ParserConflictItem"/> 是否不同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="ParserConflictItem"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="ParserConflictItem"/> 对象。</param>
		/// <returns>如果两个 <see cref="ParserConflictItem"/> 对象不同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator !=(ParserConflictItem obj1, ParserConflictItem obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion // 运算符重载

	}
}
