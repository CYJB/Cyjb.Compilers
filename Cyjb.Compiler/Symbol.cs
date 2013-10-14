namespace Cyjb.Compiler
{
	/// <summary>
	/// 表示词法或语法分析中的终结符或非终结符。
	/// </summary>
	internal abstract class Symbol
	{
		/// <summary>
		/// 表示不存在的符号的索引。
		/// </summary>
		internal const int None = -1;
		/// <summary>
		/// 使用符号的标识符初始化 <see cref="Symbol"/> 类的新实例。
		/// </summary>
		/// <param name="id">当前符号的标识符。</param>
		/// <param name="index">当前符号的索引。</param>
		protected Symbol(string id, int index)
		{
			this.Id = id;
			this.Index = index;
		}
		/// <summary>
		/// 获取当前符号的标识符。
		/// </summary>
		/// <value>当前符号的标识符。</value>
		public string Id { get; private set; }
		/// <summary>
		/// 获取当前符号的索引。
		/// </summary>
		/// <value>当前符号的索引。</value>
		internal int Index { get; private set; }
	}
}
