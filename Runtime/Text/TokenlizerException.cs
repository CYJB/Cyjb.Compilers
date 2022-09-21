using Cyjb.Compilers;

namespace Cyjb.Text;

/// <summary>
/// 词法分析异常。
/// </summary>
public class TokenlizerException : Exception
{
	/// <summary>
	/// 使用指定的字符和索引位置初始化 <see cref="TokenlizerException"/> 类的新实例。
	/// </summary>
	/// <param name="text">未识别的字符。</param>
	/// <param name="index">字符的索引位置。</param>
	public TokenlizerException(string text, int index)
		: base(Resources.UnrecognizedToken(text, index))
	{
		Text = text;
		Index = index;
	}

	/// <summary>
	/// 获取未识别的字符。
	/// </summary>
	public string Text { get; }
	/// <summary>
	/// 获取字符的索引位置。
	/// </summary>
	public int Index { get; }
}
