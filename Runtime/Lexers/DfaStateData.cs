namespace Cyjb.Compilers.Lexers;

/// <summary>
/// DFA 的状态数据。
/// </summary>
public static class DfaStateData
{
	/// <summary>
	/// 表示无效的状态。
	/// </summary>
	public const int InvalidState = -1;
	/// <summary>
	/// 默认状态的偏移。
	/// </summary>
	public const int DefaultStateOffset = 1;
	/// <summary>
	/// 符号列表的偏移。
	/// </summary>
	public const int SymbolsOffset = 2;
}
