namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 符号选项。
/// </summary>
public enum SymbolOptions
{
	/// <summary>
	/// 无选项。
	/// </summary>
	None,
	/// <summary>
	/// 可选的符号，可能出现 0 或 1 次。
	/// </summary>
	Optional,
	/// <summary>
	/// 允许出现 1 或更多次。
	/// </summary>
	OneOrMore,
	/// <summary>
	/// 允许出现 0 或更多次。
	/// </summary>
	ZeroOrMore,
}
