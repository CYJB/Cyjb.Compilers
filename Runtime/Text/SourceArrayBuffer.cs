using System.Diagnostics;
using System.Text;

namespace Cyjb.Text;

/// <summary>
/// 字符数组的缓冲区。
/// </summary>
internal sealed class SourceArrayBuffer : SourceBuffer
{
	/// <summary>
	/// 字符缓冲区。
	/// </summary>
	private readonly char[] buffer = new char[BufferSize];
	/// <summary>
	/// 字符长度。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int length;

	/// <summary>
	/// 初始化新的字符缓冲区。
	/// </summary>
	public SourceArrayBuffer() : base() { }
	/// <summary>
	/// 使用指定的下一个和上一个字符缓冲区初始化。
	/// </summary>
	/// <param name="prev">上一个字符缓冲区。</param>
	/// <param name="next">下一个字符缓冲区。</param>
	public SourceArrayBuffer(SourceBuffer prev, SourceBuffer next) : base(prev, next) { }

	/// <summary>
	/// 获取缓冲区的大小。
	/// </summary>
	/// <returns>缓冲区的大小。</returns>
	public override int Length => length;

	/// <summary>
	/// 获取指定索引的字符。
	/// </summary>
	/// <param name="index">要获取的字符索引。</param>
	/// <returns>指定索引的字符。</returns>
	public override char this[int index] => buffer[index];

	/// <summary>
	/// 获取缓冲区的字符序列。
	/// </summary>
	public override ReadOnlySpan<char> Span => buffer.AsSpan(0, length);

	/// <summary>
	/// 从指定的文本读取器中读取字符。
	/// </summary>
	/// <param name="reader">要读取的文本读取器。</param>
	public override void Read(TextReader reader)
	{
		length = reader.ReadBlock(buffer, 0, BufferSize);
	}

	/// <summary>
	/// 将指定范围的字符添加到 <see cref="StringBuilder"/> 的末尾。
	/// </summary>
	/// <param name="builder">要添加到的 <see cref="StringBuilder"/>。</param>
	/// <param name="startIndex">要添加的起始位置。</param>
	/// <param name="count">要添加的长度。</param>
	public void AppendToBuilder(StringBuilder builder, int startIndex, int count)
	{
		builder.Append(buffer, startIndex, count);
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		return $"{{{new string(buffer.Where(ch => ch != '\0').ToArray())}}}";
	}
}
