namespace Cyjb.Compilers.Parsers
{
	/// <summary>
	/// 表示语法分析器的动作类型。
	/// </summary>
	public enum ParseActionType
	{
		/// <summary>
		/// 错误动作。
		/// </summary>
		Error,
		/// <summary>
		/// 移入动作。
		/// </summary>
		Shift,
		/// <summary>
		/// 归约动作。
		/// </summary>
		Reduce,
		/// <summary>
		/// 接受。
		/// </summary>
		Accept
	}
}
