namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 符号的起始类型。
/// </summary>
internal enum SymbolStartType
{
	/// <summary>
	/// 非起始符号。
	/// </summary>
	NotStart,
	/// <summary>
	/// 起始符号。
	/// </summary>
	Start,
	/// <summary>
	/// 增广起始符号（高接受优先级）。
	/// </summary>
	AugmentedStartHighPriority,
	/// <summary>
	/// 增广起始符号（低接受优先级）。
	/// </summary>
	AugmentedStartLowPriority,
}
