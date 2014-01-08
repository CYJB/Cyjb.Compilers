using System;

namespace Cyjb.Compilers.Parsers
{
	/// <summary>
	/// 表示语法分析器的动作。
	/// </summary>
	[Serializable]
	public struct ParseAction : IEquatable<ParseAction>
	{
		/// <summary>
		/// 获取表示接受的分析器动作。
		/// </summary>
		public static readonly ParseAction Accept = new ParseAction(ParseActionType.Accept);
		/// <summary>
		/// 获取表示错误的分析器动作。
		/// </summary>
		public static readonly ParseAction Error = new ParseAction();
		/// <summary>
		/// 返回表示归约的分析器动作。
		/// </summary>
		/// <param name="index">归约使用的产生式编号。</param>
		/// <returns>表示归约的分析器动作。</returns>
		public static ParseAction Reduce(int index)
		{
			return new ParseAction(ParseActionType.Reduce, index);
		}
		/// <summary>
		/// 返回表示移入的分析器动作。
		/// </summary>
		/// <param name="index">移入后要压栈的状态编号。</param>
		/// <returns>表示移入的分析器动作。</returns>
		public static ParseAction Shift(int index)
		{
			return new ParseAction(ParseActionType.Shift, index);
		}
		/// <summary>
		/// 语法分析器动作的类型。
		/// </summary>
		private ParseActionType actionType;
		/// <summary>
		/// 与动作相关联的索引。
		/// </summary>
		private int index;
		/// <summary>
		/// 使用指定的分析动作类型初始化 <see cref="ParseAction"/> 结构的新实例。
		/// </summary>
		/// <param name="actionType">语法分析器动作的类型。</param>
		/// <overloads>
		/// <summary>
		/// 初始化 <see cref="ParseAction"/> 结构的新实例。
		/// </summary>
		/// </overloads>
		public ParseAction(ParseActionType actionType)
		{
			this.actionType = actionType;
			this.index = 0;
		}
		/// <summary>
		/// 使用指定的分析动作类型和索引初始化 <see cref="ParseAction"/> 结构的新实例。
		/// </summary>
		/// <param name="actionType">语法分析器动作的类型。</param>
		/// <param name="index">与动作相关联的索引。</param>
		public ParseAction(ParseActionType actionType, int index)
		{
			this.actionType = actionType;
			this.index = index;
		}
		/// <summary>
		/// 获取语法分析器动作的类型。
		/// </summary>
		/// <value>语法分析器动作的类型。</value>
		public ParseActionType ActionType { get { return this.actionType; } }
		/// <summary>
		/// 获取与动作相关联的索引。
		/// </summary>
		/// <value>与动作相关联的索引。对于移入动作，表示移入词法单元后的状态；
		/// 对于归约动作，表示归约使用的产生式索引。</value>
		public int Index { get { return this.index; } }
		/// <summary>
		/// 返回当前对象的字符串表示。
		/// </summary>
		/// <returns>当前对象的字符串表示。</returns>
		public override string ToString()
		{
			switch (this.actionType)
			{
				case ParseActionType.Accept:
					return "acc";
				case ParseActionType.Shift:
					return string.Concat("s", this.index);
				case ParseActionType.Reduce:
					return string.Concat("r", this.index);
				default:
					return string.Empty;
			}
		}

		#region IEquatable<ParseAction> 成员

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
		public bool Equals(ParseAction other)
		{
			if (object.ReferenceEquals(other, this))
			{
				return true;
			}
			if (this.actionType != other.actionType)
			{
				return false;
			}
			return this.index == other.index;
		}

		#endregion // IEquatable<ParseAction> 成员

		#region object 成员

		/// <summary>
		/// 确定指定的 <see cref="System.Object"/> 是否等于当前的 <see cref="ParseAction"/>。
		/// </summary>
		/// <param name="obj">与当前的 <see cref="ParseAction"/> 进行比较的 object。</param>
		/// <returns>如果指定的 <see cref="System.Object"/> 等于当前的 <see cref="ParseAction"/>，
		/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is ParseAction))
			{
				return false;
			}
			return this.Equals((ParseAction)obj);
		}

		/// <summary>
		/// 用于 <see cref="ParseAction"/> 类型的哈希函数。
		/// </summary>
		/// <returns>当前 <see cref="ParseAction"/> 的哈希代码。</returns>
		public override int GetHashCode()
		{
			return actionType.GetHashCode() ^ (index << 5);
		}

		#endregion // object 成员

		#region 运算符重载

		/// <summary>
		/// 判断两个 <see cref="ParseAction"/> 是否相同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="ParseAction"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="ParseAction"/> 对象。</param>
		/// <returns>如果两个 <see cref="ParseAction"/> 对象相同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator ==(ParseAction obj1, ParseAction obj2)
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
		/// 判断两个 <see cref="ParseAction"/> 是否不同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="ParseAction"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="ParseAction"/> 对象。</param>
		/// <returns>如果两个 <see cref="ParseAction"/> 对象不同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator !=(ParseAction obj1, ParseAction obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion // 运算符重载

	}
}
