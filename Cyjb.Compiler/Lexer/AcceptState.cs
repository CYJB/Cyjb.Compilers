using System.Collections.Generic;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示一个接受状态。
	/// </summary>
	internal struct AcceptState
	{
		/// <summary>
		/// 被接受的符号标识符。
		/// </summary>
		public IList<int> SymbolIndex;
		/// <summary>
		/// 当前的源文件索引。
		/// </summary>
		public int Index;
		/// <summary>
		/// 使用被接受的符号标识符和源文件索引初始化 <see cref="AcceptState"/> 结构的新实例。
		/// </summary>
		/// <param name="symbolIndex">被接受的符号标识符。</param>
		/// <param name="index">当前的源文件索引。</param>
		internal AcceptState(IList<int> symbolIndex, int index)
		{
			this.SymbolIndex = symbolIndex;
			this.Index = index;
		}
	}
}
