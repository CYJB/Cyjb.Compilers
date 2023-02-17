using System.Text;

namespace Cyjb.Text;

/// <summary>
/// 表示 <see cref="SourceReader"/> 的字符缓冲区。
/// </summary>
internal abstract class SourceBuffer
{
	/// <summary>
	/// 默认缓冲区的大小。
	/// </summary>
	public const int BufferSize = 0x200;

	/// <summary>
	/// 字符缓冲区起始位置的字符索引。
	/// </summary>
	public int StartIndex;
	/// <summary>
	/// 缓冲区起始是否是行首。
	/// </summary>
	public bool IsLineStart = false;
	/// <summary>
	/// 上一个字符缓冲区。
	/// </summary>
	public SourceBuffer Prev;
	/// <summary>
	/// 下一个字符缓冲区。
	/// </summary>
	public SourceBuffer Next;

	/// <summary>
	/// 初始化新的字符缓冲区。
	/// </summary>
	public SourceBuffer()
	{
		Prev = this;
		Next = this;
	}

	/// <summary>
	/// 使用指定的下一个和上一个字符缓冲区初始化。
	/// </summary>
	/// <param name="prev">上一个字符缓冲区。</param>
	/// <param name="next">下一个字符缓冲区。</param>
	public SourceBuffer(SourceBuffer prev, SourceBuffer next)
	{
		Prev = prev;
		Next = next;
	}

	/// <summary>
	/// 获取缓冲区的大小。
	/// </summary>
	/// <returns>缓冲区的大小。</returns>
	public abstract int Length { get; }

	/// <summary>
	/// 获取指定索引的字符。
	/// </summary>
	/// <param name="index">要获取的字符索引。</param>
	/// <returns>指定索引的字符。</returns>
	public abstract char this[int index] { get; }

	/// <summary>
	/// 获取缓冲区的字符序列。
	/// </summary>
	public abstract ReadOnlySpan<char> Span { get; }

	/// <summary>
	/// 从指定的文本读取器中读取字符。
	/// </summary>
	/// <param name="reader">要读取的文本读取器。</param>
	public abstract void Read(TextReader reader);
}
