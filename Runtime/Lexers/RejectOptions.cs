namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 拒绝匹配的选项。
/// </summary>
public enum RejectOptions
{
	/// <summary>
	/// 拒绝当前匹配。
	/// </summary>
	Default,
	/// <summary>
	/// 拒绝当前状态的匹配。
	/// </summary>
	/// <remarks>会忽略当前状态的后续匹配，避免使用更短的字符串重复匹配相同状态。</remarks>
	State,
}
