namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示一个接受状态。
/// </summary>
internal struct AcceptState
{
	/// <summary>
	/// 被接受的符号列表。
	/// </summary>
	public int[] Symbols;
	/// <summary>
	/// 当前的源文件索引。
	/// </summary>
	public int Index;

	/// <summary>
	/// 使用被接受的符号标识符和源文件索引初始化 <see cref="AcceptState"/> 结构的新实例。
	/// </summary>
	/// <param name="symbols">被接受的符号标识符。</param>
	/// <param name="index">当前的源文件索引。</param>
	internal AcceptState(int[] symbols, int index)
	{
		Symbols = symbols;
		Index = index;
	}
}
