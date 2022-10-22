namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 语法分析中的符号类型。
/// </summary>
internal enum SymbolType
{
	/// <summary>
	/// 未知类型。
	/// </summary>
	Unknown,
	/// <summary>
	/// 终结符。
	/// </summary>
	Terminal,
	/// <summary>
	/// 非终结符。
	/// </summary>
	NonTerminal,
	/// <summary>
	/// 错误标记符。
	/// </summary>
	Error,
	/// <summary>
	/// 空串标记符。
	/// </summary>
	Epsilon,
	/// <summary>
	/// 传递标记符。
	/// </summary>
	Spread,
	/// <summary>
	/// 文件结束标记符。
	/// </summary>
	EndOfFile,
}
