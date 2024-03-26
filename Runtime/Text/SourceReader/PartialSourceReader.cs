using System.Diagnostics.CodeAnalysis;

namespace Cyjb.Text;

/// <summary>
/// 每次只读取部分字符的缓冲区。
/// </summary>
/// <remarks>考虑到减少文本复制次数提高词法分析性能，因此不再使用环形缓冲区，
/// 而是每次创建新的缓冲区，并在相关 Token 均被释放后自动回收。
/// <see href="https://www.cnblogs.com/cyjb/p/LexerInputBuffer.html"/>
/// </remarks>
internal sealed class PartialSourceReader : SourceReader
{
	/// <summary>
	/// 文本的读取器。
	/// </summary>
	private TextReader? reader;
	/// <summary>
	/// 缓冲区的大小。
	/// </summary>
	private readonly int bufferSize;
	/// <summary>
	/// 字符缓冲区的总字符长度。
	/// </summary>
	private int length = 0;
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
	/// 使用指定的文本读取器初始化 <see cref="PartialSourceReader"/> 类的新实例。
	/// </summary>
	/// <param name="reader">文本读取器。</param>
	/// <param name="bufferSize">缓冲区的大小。</param>
	public PartialSourceReader(TextReader reader, int bufferSize)
	{
		this.reader = reader;
		this.bufferSize = bufferSize;
		current = new Buffer();
		first = last = current;
	}

	/// <summary>
	/// 获取当前位置是否位于行首。
	/// </summary>
	public override bool IsLineStart
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
				if (curIndex == length)
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
					ch = current.Text[curIndex];
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
	/// 返回下一个可用的字符，但不使用它。
	/// </summary>
	/// <returns>表示下一个要读取的字符的整数，或者如果没有要读取的字符，则为
	/// <see cref="SourceReader.InvalidCharacter"/>。</returns>
	/// <overloads>
	/// <summary>
	/// 返回之后可用的字符，但不使用它。
	/// </summary>
	/// </overloads>
	public override char Peek()
	{
		if (!EnsureBuffer(ref curIndex))
		{
			// idx 已经超出有效范围。
			return InvalidCharacter;
		}
		// 之前已确保 value 不会超出有效范围。
		if (currentIndex >= bufferSize)
		{
			return current.Next.Text[0];
		}
		else
		{
			return current.Text[currentIndex];
		}
	}

	/// <summary>
	/// 返回文本读取器中之后的 <paramref name="offset"/> 偏移的字符，但不使用它。
	/// <c>Peek(0)</c> 等价于 <see cref="Peek()"/>。
	/// </summary>
	/// <param name="offset">要读取的偏移。</param>
	/// <returns>文本读取器中之后的 <paramref name="offset"/> 偏移的字符，
	/// 或为 <see cref="SourceReader.InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	public override char Peek(int offset)
	{
		int idx = curIndex + offset;
		if (!EnsureBuffer(ref idx))
		{
			// idx 已经超出有效范围。
			return InvalidCharacter;
		}
		// 之前已确保 value 不会超出有效范围。
		idx += currentIndex - curIndex;
		Buffer buffer = current;
		while (idx >= bufferSize)
		{
			buffer = buffer.Next;
			idx -= bufferSize;
		}
		return buffer.Text[idx];
	}

	/// <summary>
	/// 读取文本读取器中的下一个字符并使该字符的位置提升一个字符。
	/// </summary>
	/// <returns>文本读取器中的下一个字符，或为 <see cref="SourceReader.InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <overloads>
	/// <summary>
	/// 返回之后可用的字符，并使该字符的位置提升。
	/// </summary>
	/// </overloads>
	public override char Read()
	{
		if (!EnsureBuffer(ref curIndex))
		{
			// index 已经超出有效范围。
			return InvalidCharacter;
		}
		curIndex++;
		if (currentIndex >= bufferSize)
		{
			current = current.Next;
			currentIndex -= bufferSize;
		}
		return current.Text[currentIndex++];
	}

	/// <summary>
	/// 读取文本读取器中之后的 <paramref name="offset"/> 偏移的字符，并使该字符的位置提升。
	/// <c>Read(0)</c> 等价于 <see cref="Read()"/>。
	/// </summary>
	/// <param name="offset">要读取的偏移。</param>
	/// <returns>文本读取器中之后的 <paramref name="offset"/> 偏移的字符，
	/// 或为 <see cref="SourceReader.InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	public override char Read(int offset)
	{
		curIndex += offset;
		if (!EnsureBuffer(ref curIndex))
		{
			// index 已经超出有效范围。
			return InvalidCharacter;
		}
		currentIndex += offset;
		while (currentIndex >= bufferSize)
		{
			current = current.Next;
			currentIndex -= bufferSize;
		}
		curIndex++;
		return current.Text[currentIndex++];
	}

	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	/// <param name="disposing">是否释放托管资源。</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && reader != null)
		{
			reader.Dispose();
			reader = null;
		}
	}

	/// <summary>
	/// 设置当前的字符索引。
	/// </summary>
	/// <param name="value">当前的字符索引，该索引从零开始。设置索引时，不能达到被丢弃的字符，或者超出文件结尾。</param>
	protected override void SetIndex(int value)
	{
		EnsureBuffer(ref value);
		// 之前已确保 value 不会超出有效范围。
		currentIndex += value - curIndex;
		curIndex = value;
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

	/// <summary>
	/// 读取指定范围的文本。
	/// </summary>
	/// <param name="start">起始索引。</param>
	/// <param name="count">要读取的文本长度。</param>
	/// <returns>读取到的文本。</returns>
	/// <remarks>调用方确保 <paramref name="start"/> 和 <paramref name="count"/> 在有效范围之内。</remarks>
	protected override StringView ReadBlockInternal(int start, int count)
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
	protected override void Free(int index)
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
		sourceLocator?.Read(last.Text.AsSpan(0, last.Length));
		length += last.Length;
		return true;
	}

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
