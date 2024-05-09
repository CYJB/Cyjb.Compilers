namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 词法分析器的状态信息。
/// </summary>
class LexerStateInfo
{
	/// <summary>
	/// 当前源文件索引。
	/// </summary>
	public int SourceIndex;
	/// <summary>
	/// 匹配的终结符起始索引（包含）。
	/// </summary>
	public int TerminalStart;
	/// <summary>
	/// 匹配的终结符结束索引（不包含）。
	/// </summary>
	public int TerminalEnd;
}
