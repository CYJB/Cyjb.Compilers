namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 状态信息。
/// </summary>
internal struct StateInfo
{
	/// <summary>
	/// 当前源文件索引。
	/// </summary>
	public readonly int Index;
	/// <summary>
	/// 匹配的符号起始索引（包含）。
	/// </summary>
	public int SymbolStart;
	/// <summary>
	/// 匹配的符号结束索引（不包含）。
	/// </summary>
	public readonly int SymbolEnd;

	/// <summary>
	/// 初始化 <see cref="StateInfo"/> 结构的新实例。
	/// </summary>
	/// <param name="index">当前源文件索引。</param>
	/// <param name="symbolStart">匹配的符号起始索引（包含）。</param>
	/// <param name="symbolEnd">匹配的符号结束索引（不包含）。</param>
	public StateInfo(int index, int symbolStart, int symbolEnd)
	{
		Index = index;
		SymbolStart = symbolStart;
		SymbolEnd = symbolEnd;
	}
}
