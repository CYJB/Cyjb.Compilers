namespace Cyjb.Compilers.Lexers
{
	/// <summary>
	/// 表示词法分析器的上下文。
	/// </summary>
	internal struct LexerContext
	{
		/// <summary>
		/// 获取当前上下文的索引。
		/// </summary>
		private int index;
		/// <summary>
		/// 获取当前上下文的标签。
		/// </summary>
		private string label;
		/// <summary>
		/// 获取当前上下文的类型。
		/// </summary>
		private LexerContextType contextType;
		/// <summary>
		/// 使用上下文的索引、标签和类型初始化 <see cref="LexerContext"/> 结构的新实例。
		/// </summary>
		/// <param name="index">上下文的索引。</param>
		/// <param name="label">上下文的标签。</param>
		/// <param name="type">上下文的类型。</param>
		public LexerContext(int index, string label, LexerContextType type)
		{
			this.index = index;
			this.label = label;
			this.contextType = type;
		}
		/// <summary>
		/// 获取当前上下文的索引。
		/// </summary>
		/// <value>当前上下文的索引。</value>
		internal int Index { get { return this.index; } }
		/// <summary>
		/// 获取当前上下文的标签。
		/// </summary>
		/// <value>当前上下文的标签。</value>
		public string Label { get { return this.label; } }
		/// <summary>
		/// 获取当前上下文的类型。
		/// </summary>
		/// <value>当前上下文的类型。</value>
		public LexerContextType ContextType { get { return this.contextType; } }

		#region IEquatable<LexerContext> 成员

		/// <summary>
		/// 指示当前对象是否等于同一类型的另一个对象。
		/// </summary>
		/// <param name="other">与此对象进行比较的对象。</param>
		/// <returns>如果当前对象等于 <paramref name="other"/>，
		/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
		/// <overloads>
		/// <summary>
		/// 指示当前对象是否等于另一个对象。
		/// </summary>
		/// </overloads>
		public bool Equals(LexerContext other)
		{
			if (object.ReferenceEquals(other, this))
			{
				return true;
			}
			if (this.index != other.index)
			{
				return false;
			}
			if (this.contextType != other.contextType)
			{
				return false;
			}
			return this.label == other.label;
		}

		#endregion // IEquatable<LexerContext> 成员

		#region object 成员

		/// <summary>
		/// 确定指定的 <see cref="System.Object"/> 是否等于当前的 <see cref="LexerContext"/>。
		/// </summary>
		/// <param name="obj">与当前的 <see cref="LexerContext"/> 进行比较的 <see cref="System.Object"/>。</param>
		/// <returns>如果指定的 <see cref="System.Object"/> 等于当前的 <see cref="LexerContext"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is LexerContext))
			{
				return false;
			}
			return this.Equals((LexerContext)obj);
		}

		/// <summary>
		/// 用于 <see cref="LexerContext"/> 类型的哈希函数。
		/// </summary>
		/// <returns>当前 <see cref="LexerContext"/> 的哈希代码。</returns>
		public override int GetHashCode()
		{
			return 5425641 ^ this.index ^ this.contextType.GetHashCode() ^ this.label.GetHashCode();
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			return string.Concat(this.label, " [", this.index, "]");
		}

		#endregion // object 成员

		#region 运算符重载

		/// <summary>
		/// 判断两个 <see cref="LexerContext"/> 是否相同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="LexerContext"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="LexerContext"/> 对象。</param>
		/// <returns>如果两个 <see cref="LexerContext"/> 对象相同，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public static bool operator ==(LexerContext obj1, LexerContext obj2)
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
		/// 判断两个 <see cref="LexerContext"/> 是否不同。
		/// </summary>
		/// <param name="obj1">要比较的第一个 <see cref="LexerContext"/> 对象。</param>
		/// <param name="obj2">要比较的第二个 <see cref="LexerContext"/> 对象。</param>
		/// <returns>如果两个 <see cref="LexerContext"/> 对象不同，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public static bool operator !=(LexerContext obj1, LexerContext obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion // 运算符重载

	}
}
