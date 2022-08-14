namespace Cyjb.Compilers;

/// <summary>
/// 提供一些工具方法。
/// </summary>
internal class Utils
{
	/// <summary>
	/// 返回索引的宽度。
	/// </summary>
	/// <param name="count">索引的总个数。</param>
	/// <returns>索引位置的宽度。</returns>
	public static int GetIndexWidth(int count)
	{
		return (int)Math.Log10(count - 1) + 1;
	}
}
