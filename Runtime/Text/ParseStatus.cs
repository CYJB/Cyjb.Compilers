namespace Cyjb.Text;

/// <summary>
/// 词法或语法分析器的解析状态。
/// </summary>
public enum ParseStatus
{
	/// <summary>
	/// 已准备好解析。
	/// </summary>
	Ready,
	/// <summary>
	/// 已解析完毕。
	/// </summary>
	Finished,
	/// <summary>
	/// 已取消解析。
	/// </summary>
	Cancelled
}
