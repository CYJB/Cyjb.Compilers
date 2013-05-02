namespace Cyjb.Compiler
{
	/// <summary>
	/// 表示词法或语法分析中的终结符或非终结符。
	/// </summary>
	public abstract class Symbol
	{
		/// <summary>
		/// 表示不存在的符号的索引。
		/// </summary>
		internal const int None = -1;
		/// <summary>
		/// 使用符号的标识符初始化 <see cref="Symbol"/> 类的新实例。
		/// </summary>
		/// <param name="index">当前符号的标识符。</param>
		protected Symbol(int index)
		{
			this.Index = index;
		}
		/// <summary>
		/// 获取当前符号的标识符。
		/// </summary>
		public int Index { get; private set; }
	}
}
