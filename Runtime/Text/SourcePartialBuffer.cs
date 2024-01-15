using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Cyjb.Text;

/// <summary>
/// 每次只读取部分字符的缓冲区。
/// </summary>
/// <remarks>考虑到减少文本复制次数提高词法分析性能，因此不再使用环形缓冲区，
/// 而是每次创建新的缓冲区，并在相关 Token 均被释放后自动回收。
/// <see href="https://www.cnblogs.com/cyjb/p/LexerInputBuffer.html"/>
/// </remarks>
internal sealed class SourcePartialBuffer : ISourceBuffer
{
	/// <summary>
	/// 文本的读取器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TextReader? reader;
	/// <summary>
	/// 缓冲区的大小。
	/// </summary>
	private readonly int bufferSize;
	/// <summary>
	/// 当前读取的位置。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int index = 0;
	/// <summary>
	/// 字符缓冲区的总字符长度。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int length = 0;
	/// <summary>
	/// 关联到的行列定位器。
	/// </summary>
	private LineLocator? locator;
	/// <summary>
	/// 是否已经全部读取完毕。
	/// </summary>
	private bool readFinished = false;

	/// <summary>
	/// 当前存有数据的缓冲区的指针。
	/// </summary>
	private Buffer current;
	/// <summary>
	/// 最后一个存有数据的缓冲区的指针。
	/// </summary>
	private Buffer last;
	/// <summary>
	/// 第一个存有数据的缓冲区的指针。
	/// </summary>
	private Buffer first;
	/// <summary>
	/// 当前缓冲区的字符索引。
	/// </summary>
	private int currentIndex;
	/// <summary>
	/// 被释放的索引。
	/// </summary>
	private int freeIndex;

	/// <summary>
	/// 上一次已读取的文本。
	/// </summary>
	private string? readedText;
	/// <summary>
	/// 上一次已读取的文本起始索引。
	/// </summary>
	private int readedStart;

	/// <summary>
	/// 使用指定的文本读取器初始化 <see cref="SourcePartialBuffer"/> 类的新实例。
	/// </summary>
	/// <param name="reader">文本读取器。</param>
	/// <param name="bufferSize">缓冲区的大小。</param>
	public SourcePartialBuffer(TextReader reader, int bufferSize)
	{
		this.reader = reader;
		this.bufferSize = bufferSize;
		current = new Buffer();
		first = last = current;
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
			if (index == value)
			{
				return;
			}
			EnsureBuffer(ref value);
			// 之前已确保 value 不会超出有效范围。
			currentIndex += value - index;
			index = value;
			// 调整当前缓冲区的位置。
			while (currentIndex < 0)
			{
				current = current.Prev;
				currentIndex += bufferSize;
			}
			while (currentIndex >= bufferSize)
			{
				current = current.Next;
				currentIndex -= bufferSize;
			}
		}
	}
	/// <summary>
	/// 获取当前位置是否位于行首。
	/// </summary>
	public bool IsLineStart
	{
		get
		{
			if (currentIndex <= 0)
			{
				return current.IsLineStart;
			}
			char ch = current.Text[currentIndex - 1];
			if (!Utils.IsLineBreak(ch))
			{
				return false;
			}
			// 兼容 \r\n 的场景。
			if (ch == '\r')
			{
				if (index == length)
				{
					// 达到当前缓冲区结尾，尝试加载下一缓冲区。
					if (current == last && !PrepareBuffer())
					{
						return true;
					}
					ch = current.Next.Text[0];
				}
				else
				{
					ch = current.Text[index];
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
	public int Length => length;

	/// <summary>
	/// 获取指定索引的字符。
	/// </summary>
	/// <param name="index">要检查的字符索引。</param>
	/// <returns>指定索引的字符。</returns>
	/// <remarks>调用方确保 <paramref name="index"/> 在 <see cref="StartIndex"/> 和 <see cref="Index"/> 之间。</remarks>
	public char this[int index]
	{
		get
		{
			// 从 first 开始查找。
			Buffer buffer = first;
			index -= buffer.StartIndex;
			while (index >= bufferSize)
			{
				buffer = buffer.Next;
				index -= bufferSize;
			}
			return buffer.Text[index];
		}
	}

	/// <summary>
	/// 设置关联到的行列定位器。
	/// </summary>
	public void SetLocator(LineLocator locator)
	{
		this.locator = locator;
	}

	/// <summary>
	/// 返回 <see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符。
	/// </summary>
	/// <param name="offset">要读取的位置偏移。</param>
	/// <returns><see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符。</returns>
	public char Read(int offset)
	{
		index += offset;
		if (!EnsureBuffer(ref index))
		{
			// index 已经超出有效范围。
			return SourceReader.InvalidCharacter;
		}
		currentIndex += offset;
		while (currentIndex >= bufferSize)
		{
			current = current.Next;
			currentIndex -= bufferSize;
		}
		index++;
		return current.Text[currentIndex++];
	}

	/// <summary>
	/// 返回 <see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符，但不提升索引。
	/// </summary>
	/// <param name="offset">要读取的位置偏移。</param>
	/// <returns><see cref="Index"/> 之后 <paramref name="offset"/> 偏移的字符。</returns>
	public char Peek(int offset)
	{
		int idx = index + offset;
		if (!EnsureBuffer(ref idx))
		{
			// idx 已经超出有效范围。
			return SourceReader.InvalidCharacter;
		}
		// 之前已确保 value 不会超出有效范围。
		idx += currentIndex - index;
		Buffer buffer = current;
		while (idx >= bufferSize)
		{
			buffer = buffer.Next;
			idx -= bufferSize;
		}
		return buffer.Text[idx];
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
		if (readedText != null && readedStart == start && readedText.Length == count)
		{
			return readedText.AsView();
		}
		// 从 first 开始查找。
		Buffer buffer = first;
		int index = start - buffer.StartIndex;
		while (index >= bufferSize)
		{
			buffer = buffer.Next;
			index -= bufferSize;
		}
		if (count + index < buffer.Length)
		{
			// 未跨缓冲区时，可以直接返回 Span。
			return buffer.Text.AsView(index, count);
		}
		// 跨缓冲区时，需要复制一份文本出来。
		readedText = string.Create(count, 0, (Span<char> span, int state) =>
		{
			int bufferLength = buffer.Length - index;
			while (true)
			{
				if (count < bufferLength)
				{
					buffer.Text.AsSpan(index, count).CopyTo(span);
					break;
				}
				else
				{
					buffer.Text.AsSpan(index, bufferLength).CopyTo(span);
					span = span[bufferLength..];
					count -= bufferLength;
					index = 0;
					buffer = buffer.Next;
					bufferLength = buffer.Length;
				}
			}
		});
		readedStart = start;
		return readedText.AsView();
	}

	/// <summary>
	/// 释放指定索引之前的字符，释放后的字符无法再被读取。
	/// </summary>
	/// <param name="index">要释放的字符起始索引。</param>
	public void Free(int index)
	{
		freeIndex = index;
		while (index >= first.StartIndex + bufferSize)
		{
			var buffer = first;
			first = first.Next;
			buffer.Prev = buffer;
			buffer.Next = buffer;
			first.Prev = first;
		}
	}

	/// <summary>
	/// 确保将指定索引的字符读入缓冲区。
	/// </summary>
	/// <param name="index">要确保读取字符的索引。如果没有更多可以读取的字符，那么会设置为缓冲区总长度。</param>
	/// <returns>如果 <paramref name="index"/> 的字符在缓冲区内，则返回 <c>true</c>；
	/// 若由于没有更多可以读取的字符而调整了 <paramref name="index"/>，则返回 <c>false</c>。</returns>
	private bool EnsureBuffer(ref int index)
	{
		if (index < length)
		{
			return true;
		}
		if (readFinished)
		{
			// 已经完成读取，直接设置为 length。
			index = length;
			return false;
		}
		// 尝试读取更多数据。
		while (index >= length)
		{
			if (!PrepareBuffer())
			{
				index = length;
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 从基础字符读取器中读取字符，并填充到新的缓冲区中。
	/// </summary>
	/// <returns>如果基础字符读取器中读取了任何字符，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
	private bool PrepareBuffer()
	{
		if (readFinished)
		{
			return false;
		}
		if (reader == null)
		{
			throw CommonExceptions.StreamClosed(nameof(SourceReader));
		}
		if (reader.Peek() == -1)
		{
			readFinished = true;
			return false;
		}
		if (length == 0)
		{
			// 填充首个缓冲区。
			last.Read(reader, bufferSize);
		}
		else
		{
			// 建立新缓冲区。
			Buffer buffer = new(reader, bufferSize, last);
			last = buffer;
		}
		locator?.Read(last.Text.AsSpan(0, last.Length));
		length += last.Length;
		return true;
	}

	#region IDisposable 成员

	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	public void Dispose()
	{
		if (reader != null)
		{
			reader.Dispose();
			reader = null;
		}
	}

	#endregion

	/// <summary>
	/// 表示 <see cref="SourcePartialBuffer"/> 的字符缓冲区。
	/// </summary>
	private sealed class Buffer
	{
		/// <summary>
		/// 字符缓冲区。
		/// </summary>
		/// <remarks>这里缓冲区包含一个额外的结束字符，减少一些不必要的索引比较</remarks>
		public string Text;
		/// <summary>
		/// 字符缓冲区起始位置的字符索引。
		/// </summary>
		public readonly int StartIndex;
		/// <summary>
		/// 缓冲区起始是否是行首。
		/// </summary>
		public readonly bool IsLineStart;
		/// <summary>
		/// 缓冲区的大小。
		/// </summary>
		public int Length;
		/// <summary>
		/// 上一个字符缓冲区。
		/// </summary>
		public Buffer Prev;
		/// <summary>
		/// 下一个字符缓冲区。
		/// </summary>
		public Buffer Next;

		/// <summary>
		/// 初始化字符缓冲区。
		/// </summary>
		public Buffer()
		{
			Text = string.Empty;
			StartIndex = 0;
			IsLineStart = true;
			Prev = Next = this;
		}

		/// <summary>
		/// 使用指定的文本读取器和上一个字符缓冲区初始化。
		/// </summary>
		/// <param name="reader">要读取的文本读取器。</param>
		/// <param name="bufferSize">缓冲区的大小。</param>
		/// <param name="prev">上一个字符缓冲区。</param>
		public Buffer(TextReader reader, int bufferSize, Buffer prev)
		{
			Read(reader, bufferSize);
			prev.Next = this;
			Prev = prev;
			Next = this;
			StartIndex = prev.StartIndex + bufferSize;
			char lastChar = prev.Text[bufferSize - 1];
			// 兼容 \r\n 的场景
			IsLineStart = Utils.IsLineBreak(lastChar) && (lastChar != '\r' || Text[0] != '\n');
		}

		/// <summary>
		/// 从指定的文本读取器中读取字符。
		/// </summary>
		/// <param name="reader">要读取的文本读取器。</param>
		/// <param name="bufferSize">缓冲区的大小。</param>
		[MemberNotNull(nameof(Text))]
		public void Read(TextReader reader, int bufferSize)
		{
			// 字符串最后保留一个哨兵字符，可以简化判断逻辑。
			Text = string.Create(bufferSize + 1, bufferSize, (Span<char> span, int bufferSize) =>
			{
				Length = reader.ReadBlock(span[..bufferSize]);
				span[Length] = SourceReader.InvalidCharacter;
			});
		}
	}
}
