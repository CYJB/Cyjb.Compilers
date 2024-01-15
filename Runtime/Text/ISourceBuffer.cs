namespace Cyjb.Text;

/// <summary>
/// 表示 <see cref="SourceReader"/> 的字符缓冲区。
/// </summary>
internal interface ISourceBuffer : IDisposable
{
	/// <summary>
	/// 获取或设置读取的起始位置。
	/// </summary>
	int StartIndex { get; set; }
	/// <summary>
	/// 获取或设置当前读取的位置。
	/// </summary>
	/// <remarks><see cref="Index"/> 可以设置为大于 <see cref="Length"/> 的值，此时会尝试将更多字符读取到缓冲区中。
	/// 如果没有更多可以读取的字符，那么 <see cref="Index"/> 会被设置为 <see cref="Length"/>。</remarks>
	int Index { get; set; }
	/// <summary>
	/// 获取当前位置是否位于行首。
	/// </summary>
	bool IsLineStart { get; }
	/// <summary>
	/// 获取字符缓冲区的总字符长度。
	/// </summary>
	/// <remarks>仅包含已读取到缓冲区的字符，可能仍有字符尚未被读取到缓冲区。</remarks>
	int Length { get; }

	/// <summary>
	/// 获取指定索引的字符。
	/// </summary>
	/// <param name="index">要检查的字符索引。</param>
	/// <returns>指定索引的字符。</returns>
	/// <remarks>调用方确保 <paramref name="index"/> 在 <see cref="StartIndex"/> 和 <see cref="Index"/> 之间。</remarks>
	char this[int index] { get; }

	/// <summary>
	/// 设置关联到的行列定位器。
	/// </summary>
	void SetLocator(LineLocator locator);
	/// <summary>
	/// 返回 <see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符，并提升索引。
	/// </summary>
	/// <param name="offset">要读取的位置偏移。</param>
	/// <returns><see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符。</returns>
	char Read(int offset);
	/// <summary>
	/// 返回 <see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符，但不提升索引。
	/// </summary>
	/// <param name="offset">要读取的位置偏移。</param>
	/// <returns><see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符。</returns>
	char Peek(int offset);
	/// <summary>
	/// 读取指定范围的文本。
	/// </summary>
	/// <param name="start">起始索引。</param>
	/// <param name="count">要读取的文本长度。</param>
	/// <returns>读取到的文本。</returns>
	/// <remarks>调用方确保 <paramref name="start"/> 和 <paramref name="count"/> 在有效范围之内。</remarks>
	StringView ReadBlock(int start, int count);

	/// <summary>
	/// 释放指定索引之前的内存，释放后的字符无法再被读取。
	/// </summary>
	/// <param name="index">要释放的字符起始索引。</param>
	void Free(int index);
}
