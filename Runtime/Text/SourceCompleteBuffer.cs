using System.Diagnostics;

namespace Cyjb.Text;

/// <summary>
/// 一次性读取完整字符串的缓冲区。
/// </summary>
/// <remarks><see cref="StringReader"/> 是 <see cref="string"/> 的包装，
/// <see cref="StringReader.ReadToEnd"/> 会返回原始的完整字符串，不需要再额外复制到缓冲区。</remarks>
internal sealed class SourceCompleteBuffer : ISourceBuffer
{
	/// <summary>
	/// 文本内容。
	/// </summary>
	private readonly string text;
	/// <summary>
	/// 当前读取的位置。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int index = 0;
	/// <summary>
	/// 关联到的行列定位器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private LineLocator? locator;
	/// <summary>
	/// 已读取到 Locator 中的索引。
	/// </summary>
	private int locatedIndex = int.MaxValue;

	/// <summary>
	/// 使用指定的文本读取器初始化 <see cref="SourceCompleteBuffer"/> 类的新实例。
	/// </summary>
	/// <param name="reader">文本读取器。</param>
	public SourceCompleteBuffer(TextReader reader)
	{
		text = reader.ReadToEnd();
	}

	/// <summary>
	/// 获取或设置读取的起始位置。
	/// </summary>
	public int StartIndex { get; set; }
	/// <summary>
	/// 获取或设置当前读取的位置。
	/// </summary>
	/// <remarks><see cref="Index"/> 可以设置为大于 <see cref="Length"/> 的值，此时会尝试将更多字符读取到缓冲区中。
	/// 如果没有更多可以读取的字符，那么 <see cref="Index"/> 会被设置为 <see cref="Length"/>。</remarks>
	public int Index
	{
		get => index;
		set
		{
			if (value >= text.Length)
			{
				index = text.Length;
			}
			else
			{
				index = value;
			}
			ReadToLocator();
		}
	}
	/// <summary>
	/// 获取当前位置是否位于行首。
	/// </summary>
	public bool IsLineStart
	{
		get
		{
			if (index <= 0)
			{
				return true;
			}
			char ch = text[index - 1];
			if (!Utils.IsLineBreak(ch))
			{
				return false;
			}
			// 兼容 \r\n 的场景。
			if (ch == '\r')
			{
				if (index == text.Length)
				{
					return true;
				}
				else
				{
					ch = text[index];
				}
				if (ch == '\n')
				{
					return false;
				}
			}
			return true;
		}
	}
	/// <summary>
	/// 获取字符缓冲区的总字符长度。
	/// </summary>
	/// <remarks>仅包含已读取到缓冲区的字符，可能仍有字符尚未被读取到缓冲区。</remarks>
	public int Length => text.Length;

	/// <summary>
	/// 获取指定索引的字符。
	/// </summary>
	/// <param name="index">要检查的字符索引。</param>
	/// <returns>指定索引的字符。</returns>
	/// <remarks>调用方确保 <paramref name="index"/> 在 <see cref="StartIndex"/> 和 <see cref="Index"/> 之间。</remarks>
	public char this[int index] => text[index];

	/// <summary>
	/// 设置关联到的行列定位器。
	/// </summary>
	public void SetLocator(LineLocator locator)
	{
		this.locator = locator;
		locatedIndex = 0;
	}

	/// <summary>
	/// 返回 <see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符。
	/// </summary>
	/// <param name="offset">要读取的位置偏移。</param>
	/// <returns><see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符。</returns>
	public char Read(int offset)
	{
		index += offset;
		if (index >= text.Length)
		{
			index = text.Length;
			return SourceReader.InvalidCharacter;
		}
		char ch = text[index++];
		ReadToLocator();
		return ch;
	}

	/// <summary>
	/// 返回 <see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符，但不提升索引。
	/// </summary>
	/// <param name="offset">要读取的位置偏移。</param>
	/// <returns><see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符。</returns>
	public char Peek(int offset)
	{
		int idx = index + offset;
		if (idx < text.Length)
		{
			return text[idx];
		}
		else
		{
			return SourceReader.InvalidCharacter;
		}
	}

	/// <summary>
	/// 读取指定范围的文本。
	/// </summary>
	/// <param name="start">起始索引。</param>
	/// <param name="count">要读取的文本长度。</param>
	/// <returns>读取到的文本。</returns>
	/// <remarks>调用方确保 <paramref name="start"/> 和 <paramref name="count"/> 在有效范围之内。</remarks>
	public StringView ReadBlock(int start, int count)
	{
		return text.AsView(start, count);
	}

	/// <summary>
	/// 释放指定索引之前的字符，释放后的字符无法再被读取。
	/// </summary>
	/// <param name="index">要释放的字符起始索引。</param>
	public void Free(int index) { }

	/// <summary>
	/// 将文本读取到行列定位器。
	/// </summary>
	private void ReadToLocator()
	{
		if (index > locatedIndex)
		{
			locator!.Read(text.AsSpan(locatedIndex, index - locatedIndex));
			locatedIndex = index;
		}
	}

	#region IDisposable 成员

	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	public void Dispose() { }

	#endregion

}
