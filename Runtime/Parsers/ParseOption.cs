namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 语法分析选项。
/// </summary>
public enum ParseOption
{
	/// <summary>
	/// 扫描到文件结尾。
	/// </summary>
	ScanToEOF,
	/// <summary>
	/// 扫描到匹配的语法单元。
	/// </summary>
	/// <remarks>对于 <c>A*</c> 这样的规则，会匹配到最后一个可能的 <c>A</c>。</remarks>
	ScanToMatch,
	/// <summary>
	/// 扫描到首次匹配的语法单元。
	/// </summary>
	/// <remarks>对于 <c>A*</c> 这样的规则，只会匹配第一个 <c>A</c>。</remarks>
	ScanToFirstMatch,
}
