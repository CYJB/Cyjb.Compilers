using Cyjb.Compilers;

namespace Cyjb.Text;

/// <summary>
/// 表示支持字符回退的源文件读取器。
/// </summary>
/// <remarks><see cref="SourceReader"/> 类中，包含一个环形字符缓冲区，
/// 关于环形字符缓冲区的详细解释，请参见我的 C# 词法分析器系列博文
/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerInputBuffer.html">
/// 《C# 词法分析器（二）输入缓冲和代码定位》</see>。</remarks>
/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerInputBuffer.html">
/// 《C# 词法分析器（二）输入缓冲和代码定位》</seealso>
public sealed class SourceReader : IDisposable
{
	/// <summary>
	/// 用于表示到达流结尾的字符。
	/// </summary>
	/// <remarks>使用非 Unicode 字符 0xFFFF。</remarks>
	public const char InvalidCharacter = char.MaxValue;
	/// <summary>
	/// 空的源文件读取器。
	/// </summary>
	internal static readonly SourceReader Empty = new(new StringReader(string.Empty));

	/// <summary>
	/// 字符缓冲区。
	/// </summary>
	private readonly ISourceBuffer buffer;
	/// <summary>
	/// 行列定位器。
	/// </summary>
	private LineLocator? locator;
	/// <summary>
	/// 标记列表。
	/// </summary>
	private readonly List<SourceMark> marks = new();
	/// <summary>
	/// 标记的起始索引。
	/// </summary>
	private int markIndex = int.MaxValue;

	/// <summary>
	/// 使用指定的字符读取器初始化 <see cref="SourceReader"/> 类的新实例。
	/// </summary>
	/// <param name="reader">用于读取源文件的字符读取器。</param>
	/// <param name="bufferSize">读取文本的缓冲区尺寸。
	/// 默认为 <c>0</c>，表示不限制缓冲区大小，内存消耗较大但性能高；
	/// 其他值会限制每块缓冲区的大小，当不在使用相关 <see cref="Token{T}"/> 后可以释放缓冲区节约内容，
	/// 缓冲区不宜设置太小，否则容易影响性能，可以考虑设置为 0x1000 或更高。
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="reader"/> 为 <c>null</c>。</exception>
	public SourceReader(TextReader reader, int bufferSize = 0)
	{
		ArgumentNullException.ThrowIfNull(reader);
		if (reader is StringReader || bufferSize <= 0 || bufferSize == int.MaxValue)
		{
			// StringReader 已经包含了完整的字符串，没有分块读取的意义。
			buffer = new SourceCompleteBuffer(reader);
		}
		else
		{
			buffer = new SourcePartialBuffer(reader, bufferSize);
		}
	}

	/// <summary>
	/// 获取关联到的行列定位器。
	/// </summary>
	public LineLocator? Locator => locator;
	/// <summary>
	/// 获取或设置结束读取的位置。
	/// </summary>
	/// <remarks>会为超出 <see cref="End"/> 的读取返回 <see cref="InvalidCharacter"/>。</remarks>
	public int End { get; set; } = int.MaxValue;

	/// <summary>
	/// 获取或设置当前的字符索引。
	/// </summary>
	/// <value>当前的字符索引，该索引从零开始。设置索引时，不能达到被丢弃的字符，或者超出文件结尾。</value>
	public int Index
	{
		get => buffer.Index;
		set
		{
			if (value < buffer.Index)
			{
				if (value < buffer.StartIndex)
				{
					value = buffer.StartIndex;
				}
				buffer.Index = value;
			}
			else if (value > buffer.Index)
			{
				if (value > End)
				{
					value = End;
				}
				buffer.Index = value;
			}
		}
	}

	/// <summary>
	/// 返回当前是否位于行首。
	/// </summary>
	public bool IsLineStart => buffer.IsLineStart;

	/// <summary>
	/// 开启行列定位功能，允许通过 <see cref="GetPosition"/> 获取指定索引的行列位置。
	/// 需要在读取字符之前设置。
	/// </summary>
	/// <param name="tabSize">Tab 的宽度。</param>
	/// <returns>当前源读取器。</returns>
	public SourceReader UseLineLocator(int tabSize = 4)
	{
		if (locator == null)
		{
			locator = new LineLocator(tabSize);
			buffer.SetLocator(locator);
		}
		return this;
	}

	/// <summary>
	/// 返回指定索引的行列位置，需要提前 <see cref="UseLineLocator"/>。
	/// </summary>
	/// <param name="index">要检查行列位置的索引。</param>
	/// <returns>指定索引的行列位置。</returns>
	/// <exception cref="InvalidOperationException">未提前调用 <see cref="UseLineLocator"/>。</exception>
	public LinePosition GetPosition(int index)
	{
		if (locator == null)
		{
			throw new InvalidOperationException(Resources.GetPositionBeforeUse);
		}
		return locator.GetPosition(index);
	}

	/// <summary>
	/// 返回指定文本范围的行列位置范围，需要提前 <see cref="UseLineLocator"/>。
	/// </summary>
	/// <param name="span">要检查行列位置的文本范围。</param>
	/// <returns>指定文本范围的行列位置范围。</returns>
	/// <exception cref="InvalidOperationException">未提前调用 <see cref="UseLineLocator"/>。</exception>
	public LinePositionSpan GetLinePositionSpan (TextSpan span)
	{
		if (locator == null)
		{
			throw new InvalidOperationException(Resources.GetPositionBeforeUse);
		}
		return locator.GetSpan(span);
	}

	/// <summary>
	/// 关闭 <see cref="SourceReader"/> 对象和基础字符读取器，并释放与读取器关联的所有系统资源。
	/// </summary>
	public void Close()
	{
		Dispose();
	}

	#region IDisposable 成员

	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	public void Dispose()
	{
		buffer.Dispose();
	}

	#endregion

	#region 读取字符

	/// <summary>
	/// 返回下一个可用的字符，但不使用它。
	/// </summary>
	/// <returns>表示下一个要读取的字符的整数，或者如果没有要读取的字符，则为 <see cref="InvalidCharacter"/>。</returns>
	/// <overloads>
	/// <summary>
	/// 返回之后可用的字符，但不使用它。
	/// </summary>
	/// </overloads>
	public char Peek()
	{
		if (buffer.Index >= End)
		{
			return InvalidCharacter;
		}
		return buffer.Peek(0);
	}

	/// <summary>
	/// 返回文本读取器中之后的 <paramref name="offset"/> 偏移的字符，但不使用它。
	/// <c>Peek(0)</c> 等价于 <see cref="Peek()"/>。
	/// </summary>
	/// <param name="offset">要读取的偏移。</param>
	/// <returns>文本读取器中之后的 <paramref name="offset"/> 偏移的字符，
	/// 或为 <see cref="InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <exception cref="ObjectDisposedException">当前 <see cref="SourceReader"/> 已关闭。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> 小于 <c>0</c>。</exception>
	public char Peek(int offset)
	{
		if (offset < 0)
		{
			throw CommonExceptions.ArgumentNegative(offset);
		}
		if (buffer.Index + offset >= End)
		{
			return InvalidCharacter;
		}
		return buffer.Peek(offset);
	}

	/// <summary>
	/// 读取文本读取器中的下一个字符并使该字符的位置提升一个字符。
	/// </summary>
	/// <returns>文本读取器中的下一个字符，或为 <see cref="InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <overloads>
	/// <summary>
	/// 返回之后可用的字符，并使该字符的位置提升。
	/// </summary>
	/// </overloads>
	public char Read()
	{
		if (buffer.Index >= End)
		{
			return InvalidCharacter;
		}
		return buffer.Read(0);
	}

	/// <summary>
	/// 读取文本读取器中之后的 <paramref name="offset"/> 偏移的字符，并使该字符的位置提升。
	/// <c>Read(0)</c> 等价于 <see cref="Read()"/>。
	/// </summary>
	/// <param name="offset">要读取的偏移。</param>
	/// <returns>文本读取器中之后的 <paramref name="offset"/> 偏移的字符，
	/// 或为 <see cref="InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <exception cref="ObjectDisposedException">当前 <see cref="SourceReader"/> 已关闭。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> 小于 <c>0</c>。</exception>
	public char Read(int offset)
	{
		if (offset < 0)
		{
			throw CommonExceptions.ArgumentNegative(offset);
		}
		if (buffer.Index + offset >= End)
		{
			offset = End - buffer.Index;
		}
		return buffer.Read(offset);
	}

	/// <summary>
	/// 回退最后被读取的字符，只有之前的数据未被丢弃时才可以进行回退。
	/// </summary>
	/// <returns>如果回退成功，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	/// <overloads>
	/// <summary>
	/// 回退最后被读取的字符，只有之前的数据未被丢弃时才可以进行回退。
	/// </summary>
	/// </overloads>
	public bool Unget()
	{
		if (buffer.Index > buffer.StartIndex)
		{
			buffer.Index--;
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// 回退 <paramref name="count"/> 个字符，只有之前的数据未被丢弃时才可以进行回退。
	/// <c>Unget(1)</c> 等价于 <see cref="Unget()"/>。
	/// </summary>
	/// <param name="count">要回退的字符个数。</param>
	/// <returns>实际回退的字符个数，小于等于 <paramref name="count"/>。</returns>
	/// <exception cref="ObjectDisposedException">当前 <see cref="SourceReader"/> 已关闭。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> 小于 <c>0</c>。</exception>
	public int Unget(int count)
	{
		if (count < 0)
		{
			throw CommonExceptions.ArgumentNegative(count);
		}
		else if (count == 0)
		{
			return 0;
		}
		count = Math.Min(count, buffer.Index - buffer.StartIndex);
		buffer.Index -= count;
		return count;
	}

	/// <summary>
	/// 返回当前位置之前的数据。
	/// </summary>
	/// <returns>当前位置之前的数据。</returns>
	public StringView GetReadedText()
	{
		return buffer.ReadBlock(buffer.StartIndex, buffer.Index - buffer.StartIndex);
	}

	/// <summary>
	/// 将当前位置之前的数据全部丢弃，之后的 <see cref="Unget()"/> 操作至多回退到当前位置。
	/// </summary>
	public void Drop()
	{
		buffer.StartIndex = buffer.Index;
		buffer.Free(Math.Min(buffer.StartIndex, markIndex));
	}

	/// <summary>
	/// 将当前位置之前的数据全部丢弃，并返回被丢弃的数据。之后的 <see cref="Unget()"/> 操作至多回退到当前位置。
	/// </summary>
	/// <returns>当前位置之前的数据。</returns>
	public StringView Accept()
	{
		StringView text = buffer.ReadBlock(buffer.StartIndex, buffer.Index - buffer.StartIndex);
		Drop();
		return text;
	}

	/// <summary>
	/// 将当前位置之前的数据全部丢弃，并以 <see cref="Token{T}"/> 的形式返回被丢弃的数据。
	/// 之后的 <see cref="Unget()"/> 操作至多回退到当前位置。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
	/// <param name="kind">返回的 <see cref="Token{T}"/> 的标识符。</param>
	/// <param name="span">词法单元的文本范围。</param>
	/// <param name="value"><see cref="Token{T}"/> 的值。</param>
	/// <returns>当前位置之前的数据。</returns>
	public Token<T> AcceptToken<T>(T kind, TextSpan? span = null, object? value = null)
		where T : struct
	{
		TextSpan tokenSpan;
		if (span.HasValue)
		{
			tokenSpan = span.Value;
		}
		else
		{
			tokenSpan = new TextSpan(buffer.StartIndex, buffer.Index);
		}
		return new Token<T>(kind, Accept(), tokenSpan, value);
	}

	#endregion // 读取字符

	#region 标记

	/// <summary>
	/// 标记源文件的当前位置。
	/// </summary>
	/// <returns>位置的标记。</returns>
	/// <remarks>被标记的位置及其之后的字符可以确保能够通过 <c>ReadBlock</c> 方法读取。</remarks>
	public SourceMark Mark()
	{
		SourceMark mark = new(buffer.Index);
		int index = marks.BinarySearch(mark);
		if (index < 0)
		{
			index = ~index;
		}
		marks.Insert(index, mark);
		if (markIndex > mark.Index)
		{
			markIndex = mark.Index;
		}
		return mark;
	}

	/// <summary>
	/// 释放指定的源文件位置标记。
	/// </summary>
	/// <param name="mark">要释放的位置标记。</param>
	public void Release(SourceMark? mark)
	{
		int index = FindMark(mark);
		if (index < 0)
		{
			return;
		}
		mark!.Valid = false;
		marks.RemoveAt(index);
		if (index == 0)
		{
			// 丢弃不再需要的字符。
			if (marks.Count == 0)
			{
				markIndex = int.MaxValue;
				buffer.Free(buffer.StartIndex);
			}
			else if (marks[0].Index < buffer.StartIndex)
			{
				buffer.Free(marks[0].Index);
			}
		}
	}

	/// <summary>
	/// 找到指定源文件位置标记的索引。
	/// </summary>
	/// <param name="mark">要查找的源文件位置标记。</param>
	/// <returns>指定源文件位置标记的索引，如果未找到则返回 <c>-1</c>。</returns>
	private int FindMark(SourceMark? mark)
	{
		if (mark == null || !mark.Valid)
		{
			return -1;
		}
		int index = marks.BinarySearch(mark);
		if (index < 0)
		{
			return -1;
		}
		// index 可能是任何一个匹配项，因此需要在双向查找一下。
		for (int i = index; i >= 0 && marks[i].Index == mark.Index; i--)
		{
			if (marks[i] == mark)
			{
				return i;
			}
		}
		for (int i = index + 1; i < marks.Count && marks[i].Index == mark.Index; i++)
		{
			if (marks[i] == mark)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// 读取指定范围的文本。
	/// </summary>
	/// <param name="index">起始索引。</param>
	/// <param name="count">要读取的长度。</param>
	/// <returns>指定范围的文本。</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> 不在已保留的字符缓冲范围内。
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> + <paramref name="count"/>
	/// 超出当前已读取的字符范围。</exception>
	public StringView ReadBlock(int index, int count)
	{
		int minIndex = marks.Count == 0 ? buffer.StartIndex : marks[0].Index;
		// 检查 index 和 count 是否在当前范围内。
		if (index < minIndex)
		{
			throw CommonExceptions.ArgumentOutOfRange(index);
		}
		if (index + count > buffer.Index)
		{
			throw CommonExceptions.ArgumentOutOfRange(count);
		}
		if (count == 0)
		{
			return StringView.Empty;
		}
		return buffer.ReadBlock(index, count);
	}

	/// <summary>
	/// 读取指定标记间的文本。
	/// </summary>
	/// <param name="start">起始标记（包含）。</param>
	/// <param name="end">结束标记（不包含）。</param>
	/// <returns>指定标记间的文本。</returns>
	/// <exception cref="ArgumentException">传入的标记已被释放。</exception>
	/// <exception cref="ArgumentOutOfRangeException">起始和结束标记的顺序不正确。</exception>
	public StringView ReadBlock(SourceMark start, SourceMark end)
	{
		if (!start.Valid)
		{
			throw new ArgumentException(Resources.InvalidSourceMark, nameof(start));
		}
		if (!end.Valid)
		{
			throw new ArgumentException(Resources.InvalidSourceMark, nameof(end));
		}
		int count = end.Index - start.Index;
		if (count < 0)
		{
			throw CommonExceptions.ArgumentMinMaxValue(nameof(start), nameof(end));
		}
		else if (count == 0)
		{
			return StringView.Empty;
		}
		else
		{
			return buffer.ReadBlock(start.Index, count);
		}
	}

	#endregion // 标记

}
