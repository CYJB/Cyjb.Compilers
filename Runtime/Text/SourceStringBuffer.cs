namespace Cyjb.Text;

/// <summary>
/// 字符串的缓冲区。
/// </summary>
/// <remarks><see cref="StringReader"/> 是 <see cref="string"/> 的包装，
/// <see cref="StringReader.ReadToEnd"/> 会返回原始的完整字符串，不需要再额外复制到缓冲区。</remarks>
internal sealed class SourceStringBuffer : SourceBuffer
{
	/// <summary>
	/// 文本内容。
	/// </summary>
	private string text = string.Empty;

	/// <summary>
	/// 初始化新的字符缓冲区。
	/// </summary>
	public SourceStringBuffer() : base() { }

	/// <summary>
	/// 获取缓冲区的大小。
	/// </summary>
	/// <returns>缓冲区的大小。</returns>
	public override int Length => text.Length;

	/// <summary>
	/// 获取指定索引的字符。
	/// </summary>
	/// <param name="index">要获取的字符索引。</param>
	/// <returns>指定索引的字符。</returns>
	public override char this[int index] => text[index];

	/// <summary>
	/// 获取指定范围的字符串。
	/// </summary>
	/// <param name="range">要获取的字符串范围。</param>
	/// <returns>指定范围的字符串。</returns>
	public string this[Range range] => text[range];

	/// <summary>
	/// 获取缓冲区的字符序列。
	/// </summary>
	public override ReadOnlySpan<char> Span => text;

	/// <summary>
	/// 从指定的文本读取器中读取字符。
	/// </summary>
	/// <param name="reader">要读取的文本读取器。</param>
	public override void Read(TextReader reader)
	{
		text = reader.ReadToEnd();
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		return text;
	}
}
