using System;
using System.Collections.Generic;

namespace Cyjb.Compilers.Lexers
{
	/// <summary>
	/// 表示 DFA 的状态。
	/// </summary>
	[Serializable]
	public struct StateData : IEquatable<StateData>
	{
		/// <summary>
		/// DFA 状态的转移。
		/// </summary>
		private int[] transitions;
		/// <summary>
		/// DFA 状态对应的终结符索引。
		/// </summary>
		private int[] symbolIndex;
		/// <summary>
		/// 使用指定的转移和终结符索引初始化 <see cref="StateData"/> 结构的新实例。
		/// </summary>
		/// <param name="trans">DFA 状态的转移。</param>
		/// <param name="symbolIndex">DFA 状态对应的终结符索引。</param>
		internal StateData(int[] trans, int[] symbolIndex)
		{
			this.transitions = trans;
			this.symbolIndex = symbolIndex;
		}
		/// <summary>
		/// 获取 DFA 状态的转移。
		/// </summary>
		/// <value>其长度与词法分析器使用的字符类数量 <see cref="LexerRule&lt;T&gt;.CharClassCount"/> 相同。
		/// 使用 <c>-1</c> 表示空转移。</value>
		public IList<int> Transitions { get { return this.transitions; } }
		/// <summary>
		/// 获取 DFA 状态对应的终结符索引。
		/// </summary>
		/// <value>使用大于零，小于终结符数量 <see cref="LexerRule&lt;T&gt;.SymbolCount"/> 的数表示终结符索引，
		/// 使用 <see cref="Int32.MaxValue"/> - index 表示向前看符号的头节点。</value>
		public IList<int> SymbolIndex { get { return this.symbolIndex; } }
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			if (symbolIndex.Length == 0)
			{
				return "[]";
			}
			return string.Concat("[", string.Join(",", symbolIndex), "]");
		}

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
			if (this.transitions != other.transitions)
			{
				return false;
			}
			return this.symbolIndex == other.symbolIndex;
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
			return this.transitions.GetHashCode() ^ this.symbolIndex.GetHashCode();
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
