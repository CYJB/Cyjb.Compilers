namespace Cyjb.Compilers.Parsers;

/// <summary>
/// LR 语法分析器的动作类型。
/// </summary>
public enum ParserActionType
{
	/// <summary>
	/// 错误动作。
	/// </summary>
	Error = 0,
	/// <summary>
	/// 移入动作。
	/// </summary>
	Shift = 1,
	/// <summary>
	/// 归约动作。
	/// </summary>
	Reduce = 2,
	/// <summary>
	/// 接受动作。
	/// </summary>
	Accept = 3,
}
