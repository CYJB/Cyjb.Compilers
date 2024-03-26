namespace Cyjb.Text;

/// <summary>
/// 表示字符串的源文件读取器。
/// </summary>
internal sealed class StringSourceReader : SourceReader
{
	/// <summary>
	/// 文本内容。
	/// </summary>
	private readonly string source;
	/// <summary>
	/// 可读取的文本长度。
	/// </summary>
	private readonly int length;

	/// <summary>
	/// 使用指定的文本内容初始化 <see cref="StringSourceReader"/> 类的新实例。
	/// </summary>
	/// <param name="source">文本内容。</param>
	public StringSourceReader(string source)
	{
		this.source = source;
		length = source.Length;
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
			char ch = source[curIndex - 1];
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
					ch = source[curIndex];
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
		sourceLocator.Read(source);
		return this;
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
		if (curIndex < length)
		{
			return source[curIndex];
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
	public override char Peek(int offset)
	{
		int idx = curIndex + offset;
		if (idx < length)
		{
			return source[idx];
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
		return source[curIndex++];
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
		if (curIndex >= length)
		{
			curIndex = length;
			return InvalidCharacter;
		}
		return source[curIndex++];
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
		return source.AsView(start, count);
	}
}
