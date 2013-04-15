namespace Cyjb.Compiler.Lexers
{
	/// <summary>
	/// 表示 NFA 状态的类型。
	/// </summary>
	internal enum NfaStateType
	{
		/// <summary>
		/// 普通状态。
		/// </summary>
		Normal,
		/// <summary>
		/// 向前看状态的头。
		/// </summary>
		TrailingHead,
		/// <summary>
		/// 向前看状态。
		/// </summary>
		Trailing
	}
}
