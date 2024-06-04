namespace Cyjb.Text;

/// <summary>
/// 表示字符串视图的源文件读取器。
/// </summary>
internal sealed class StringViewSourceReader : SourceReader
{
	/// <summary>
	/// 文本内容。
	/// </summary>
	private readonly string source;
	/// <summary>
	/// 要读取的起始索引。
	/// </summary>
	private readonly int start;
	/// <summary>
	/// 可读取的文本长度。
	/// </summary>
	private readonly int length;

	/// <summary>
	/// 使用指定的文本内容初始化 <see cref="StringViewSourceReader"/> 类的新实例。
	/// </summary>
	/// <param name="source">文本内容。</param>
	/// <param name="start">要读取的起始索引。</param>
	/// <param name="length">要读取的字符长度。</param>
	public StringViewSourceReader(string source, int start, int length)
	{
		this.source = source;
		this.start = start;
		this.length = length;
	}

	/// <summary>
	/// 获取当前位置是否位于行首。
	/// </summary>
	public override bool IsLineStart
	{
		get
		{
			if (curIndex <= 0)
			{
				return true;
			}
			char ch = source[curIndex + start - 1];
			if (!Utils.IsLineBreak(ch))
			{
				return false;
			}
			// 兼容 \r\n 的场景。
			if (ch == '\r')
			{
				if (curIndex == length)
				{
					return true;
				}
				else
				{
					ch = source[curIndex + start];
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
	/// 开启行列定位功能，允许通过 <see cref="SourceReader.GetPosition"/> 获取指定索引的行列位置。
	/// 需要在读取字符之前设置。
	/// </summary>
	/// <param name="tabSize">Tab 的宽度。</param>
	/// <returns>当前源读取器。</returns>
	public override SourceReader UseLineLocator(int tabSize = 4)
	{
		base.UseLineLocator(tabSize);
		// 总是在关联的时候一次性完成读取。
		sourceLocator.Read(source.AsSpan(start, length));
		return this;
	}

	/// <summary>
	/// 返回下一个可用的字符，但不使用它。
	/// </summary>
	/// <returns>表示下一个要读取的字符的整数，或者如果没有要读取的字符，则为 <see cref="SourceReader.InvalidCharacter"/>。</returns>
	/// <overloads>
	/// <summary>
	/// 返回之后可用的字符，但不使用它。
	/// </summary>
	/// </overloads>
	public override char Peek()
	{
		if (curIndex < length)
		{
			return source[curIndex + start];
		}
		else
		{
			return InvalidCharacter;
		}
	}

	/// <summary>
	/// 返回文本读取器中之后的 <paramref name="offset"/> 偏移的字符，但不使用它。
	/// <c>Peek(0)</c> 等价于 <see cref="Peek()"/>。
	/// </summary>
	/// <param name="offset">要读取的偏移。</param>
	/// <returns>文本读取器中之后的 <paramref name="offset"/> 偏移的字符，
	/// 或为 <see cref="SourceReader.InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <exception cref="ObjectDisposedException">当前 <see cref="SourceReader"/> 已关闭。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> 小于 <c>0</c>。</exception>
	public override char Peek(int offset)
	{
		if (offset < 0)
		{
			throw CommonExceptions.ArgumentNegative(offset);
		}
		int idx = curIndex + offset;
		if (idx < length)
		{
			return source[idx + start];
		}
		else
		{
			return InvalidCharacter;
		}
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
		if (curIndex >= length)
		{
			return InvalidCharacter;
		}
		return source[curIndex++ + start];
	}

	/// <summary>
	/// 读取文本读取器中之后的 <paramref name="offset"/> 偏移的字符，并使该字符的位置提升。
	/// <c>Read(0)</c> 等价于 <see cref="Read()"/>。
	/// </summary>
	/// <param name="offset">要读取的偏移。</param>
	/// <returns>文本读取器中之后的 <paramref name="offset"/> 偏移的字符，
	/// 或为 <see cref="SourceReader.InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <exception cref="ObjectDisposedException">当前 <see cref="SourceReader"/> 已关闭。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> 小于 <c>0</c>。</exception>
	public override char Read(int offset)
	{
		curIndex += offset;
		if (curIndex >= length)
		{
			curIndex = length;
			return InvalidCharacter;
		}
		return source[curIndex++ + start];
	}

	/// <summary>
	/// 从当前位置查找指定字符的偏移，使用指定的起始偏移开始。
	/// </summary>
	/// <param name="ch">要查找的字符。</param>
	/// <param name="start">要查找的起始偏移。</param>
	/// <returns>如果找到字符，则为 <paramref name="ch"/> 从当前位置开始的偏移；
	/// 如果未找到，则返回 <c>-1</c>。</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/>
	/// 小于零或大于剩余字符数。</exception>
	public override int IndexOf(char ch, int start)
	{
		int index = curIndex + this.start;
		if (start < 0 || (start += index) > length)
		{
			throw CommonExceptions.ArgumentIndexOutOfRange(start);
		}
		int idx = source.IndexOf(ch, start);
		if (idx >= 0)
		{
			idx -= index;
		}
		return idx;
	}

	/// <summary>
	/// 从当前位置查找指定字符数组中的任意字符的偏移，使用指定的起始偏移开始。
	/// </summary>
	/// <param name="anyOf">要查找的字符数组。</param>
	/// <param name="start">要查找的起始偏移。</param>
	/// <returns>如果找到 <paramref name="anyOf"/> 中的任意字符，则为该字符从当前位置开始的偏移；
	/// 如果未找到，则返回 <c>-1</c>。</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/>
	/// 小于零或大于剩余字符数。</exception>
	public override int IndexOfAny(char[] anyOf, int start)
	{
		int index = curIndex + this.start;
		if (start < 0 || (start += index) > length)
		{
			throw CommonExceptions.ArgumentIndexOutOfRange(start);
		}
		int idx = source.IndexOfAny(anyOf, start);
		if (idx >= 0)
		{
			idx -= index;
		}
		return idx;
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
		return source.AsView(start + this.start, count);
	}
}
