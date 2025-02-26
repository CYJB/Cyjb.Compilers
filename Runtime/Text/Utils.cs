namespace Cyjb.Text;

/// <summary>
/// 提供工具方法。
/// </summary>
internal static class Utils
{
	/// <summary>
	/// 换行的字符。
	/// </summary>
	public static char[] NewLineChars = { '\r', '\n' };

	/// <summary>
	/// 返回指定的字符是否是换行符。
	/// </summary>
	public static bool IsLineBreak(char ch)
	{
		return ch is '\n' or '\r' or '\u0085' or '\u2028' or '\u2029';
	}
}
