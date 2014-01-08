using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cyjb.Compilers.Parsers
{
	/// <summary>
	/// 表示 LR 语法分析器的状态。
	/// </summary>
	[Serializable]
	public struct StateData : IEquatable<StateData>
	{
		/// <summary>
		/// LR 语法分析表的动作。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ParseAction[] actions;
		/// <summary>
		/// LR 语法分析表的转移。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int[] gotos;
		/// <summary>
		/// LR 语法分析器状态包含的原始规则。
		/// </summary>
		[NonSerialized, DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string[] originRules;
		/// <summary>
		/// 使用指定的动作和转移引初始化 <see cref="StateData"/> 结构的新实例。
		/// </summary>
		/// <param name="actions">LR 语法分析表的动作。</param>
		/// <param name="gotos">LR 语法分析表的转移。</param>
		/// <param name="originRules">LR 语法分析器状态包含的原始规则。</param>
		internal StateData(ParseAction[] actions, int[] gotos, string[] originRules)
		{
			this.actions = actions;
			this.gotos = gotos;
			this.originRules = originRules;
		}
		/// <summary>
		/// 获取 LR 语法分析表的动作。
		/// </summary>
		/// <value>LR 语法分析表的动作，每个元素分别对应唯一归约、分析错误、文件结束和所有终结符。
		/// 其长度与 <see cref="ParserRule&lt;T&gt;.ActionCount"/> 相同，
		/// 索引 <c>0</c> 表示唯一归约，索引 <c>1</c> 表示分析错误，
		/// 索引 <c>2</c> 表示文件结束，剩余的对应所有终结符。</value>
		public IList<ParseAction> Actions { get { return this.actions; } }
		/// <summary>
		/// 获取 LR 语法分析表的转移。
		/// </summary>
		/// <value>LR 语法分析表的转移，每个元素分别对应所有非终结符。
		/// 其中大于等于零的数表示转移到的状态，<c>-1</c> 表示不存在转移。</value>
		public IList<int> Gotos { get { return this.gotos; } }
		/// <summary>
		/// 获取 LR 语法分析器状态包含的原始规则。
		/// </summary>
		/// <value>LR 语法分析器状态包含的原始规则，仅用于调试。</value>
		public IList<string> OriginRules { get { return this.originRules; } }

		#region IEquatable<StateData> 成员

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
		public bool Equals(StateData other)
		{
			if (object.ReferenceEquals(other, this))
			{
				return true;
			}
			if (this.actions != other.actions)
			{
				return false;
			}
			return this.gotos == other.gotos;
		}

		#endregion // IEquatable<StateData> 成员

		#region object 成员

		/// <summary>
		/// 确定指定的 <see cref="System.Object"/> 是否等于当前的 <see cref="StateData"/>。
		/// </summary>
		/// <param name="obj">与当前的 <see cref="StateData"/> 进行比较的 object。</param>
		/// <returns>如果指定的 <see cref="System.Object"/> 等于当前的 <see cref="StateData"/>，
		/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is StateData))
			{
				return false;
			}
			return this.Equals((StateData)obj);
		}
		/// <summary>
		/// 用于 <see cref="StateData"/> 类型的哈希函数。
		/// </summary>
		/// <returns>当前 <see cref="StateData"/> 的哈希代码。</returns>
		public override int GetHashCode()
		{
			return this.actions.GetHashCode() ^ this.gotos.GetHashCode();
		}

		#endregion // object 成员

		#region 运算符重载

		/// <summary>
		/// 判断两个 <see cref="StateData"/> 是否相同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="StateData"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="StateData"/> 对象。</param>
		/// <returns>如果两个 <see cref="StateData"/> 对象相同，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public static bool operator ==(StateData obj1, StateData obj2)
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
		/// 判断两个 <see cref="StateData"/> 是否不同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="StateData"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="StateData"/> 对象。</param>
		/// <returns>如果两个 <see cref="StateData"/> 对象不同，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public static bool operator !=(StateData obj1, StateData obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion // 运算符重载

	}
}
