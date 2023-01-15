namespace Cyjb.Compilers.Lexers;

/// <summary>
/// NFA 的构造结果。
/// </summary>
public struct NfaBuildResult
{
	/// <summary>
	/// 结果的首状态。
	/// </summary>
	public NfaState Head;
	/// <summary>
	/// 结果的尾状态。
	/// </summary>
	public NfaState Tail;
	/// <summary>
	/// 是否是行首限定的。
	/// </summary>
	public bool BeginningOfLine;
}
