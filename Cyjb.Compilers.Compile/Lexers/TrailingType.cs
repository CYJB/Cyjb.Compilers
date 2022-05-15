namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 向前看符号的类型。
/// </summary>
public enum TrailingType
{
	/// <summary>
	/// 没有使用向前看符号。
	/// </summary>
	None,
	/// <summary>
	/// 定长的向前看符号。
	/// </summary>
	Fixed,
	/// <summary>
	/// 变长的向前看符号。
	/// </summary>
	Variable
}
