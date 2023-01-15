using System.Diagnostics;
using System.Text;
using Cyjb.Compilers;
using Cyjb.Compilers.Text;

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
	/// 缓冲区的大小。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private const int BufferSize = 0x200;

	/// <summary>
	/// 返回指定的字符是否是换行符。
	/// </summary>
	private static bool IsLineBreak(char ch)
	{
		return ch is '\n' or '\r' or '\u0085' or '\u2028' or '\u2029';
	}

	/// <summary>
	/// 当前存有数据的缓冲区的指针。
	/// </summary>
	private SourceBuffer current;
	/// <summary>
	/// 最后一个存有数据的缓冲区的指针。
	/// </summary>
	private SourceBuffer last;
	/// <summary>
	/// 第一个存有数据的缓冲区的指针。
	/// </summary>
	private SourceBuffer first;
	/// <summary>
	/// 文本的读取器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TextReader? reader;
	/// <summary>
	/// 当前缓冲区的字符索引。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int index;
	/// <summary>
	/// 当前缓冲区的字符长度。
	/// </summary>
	private int length;
	/// <summary>
	/// 当前全局字符索引。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int globalIndex;
	/// <summary>
	/// 可以被 Accept 的起始字符索引。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int startIndex;
	/// <summary>
	/// 第一块缓冲区的字符索引。
	/// </summary>
	private int firstIndex;
	/// <summary>
	/// 最后一块缓冲区的字符长度。
	/// </summary>
	private int lastLength;
	/// <summary>
	/// 用于构造字符串的 <see cref="StringBuilder"/> 实例。
	/// </summary>
	private readonly StringBuilder builder = new(BufferSize);
	/// <summary>
	/// 构造字符串时的起始索引。
	/// </summary>
	private int builderIndex = -1;
	/// <summary>
	/// 已向 <see cref="builder"/> 中复制了的字符串长度。
	/// </summary>
	private int builderCopiedLen;
	/// <summary>
	/// 行列定位器。
	/// </summary>
	private LineLocator? locator;
	/// <summary>
	/// 标记列表。
	/// </summary>
	private readonly List<SourceMark> marks = new();

	/// <summary>
	/// 使用指定的字符读取器初始化 <see cref="SourceReader"/> 类的新实例。
	/// </summary>
	/// <param name="reader">用于读取源文件的字符读取器。</param>
	/// <exception cref="ArgumentNullException"><paramref name="reader"/> 为 <c>null</c>。</exception>
	public SourceReader(TextReader reader)
	{
		ArgumentNullException.ThrowIfNull(reader);
		this.reader = reader;
		current = new SourceBuffer
		{
			IsLineStart = true
		};
		last = current;
		first = current;
		firstIndex = lastLength = 0;
	}

	/// <summary>
	/// 获取基础的字符读取器。
	/// </summary>
	/// <value>基础的字符读取器。如果当前读取器已被关闭，则返回 <c>null</c>。</value>
	public TextReader? BaseReader => reader;
	/// <summary>
	/// 获取行列定位器。
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
		get { return globalIndex; }
		set
		{
			if (value > globalIndex)
			{
				Read(value - globalIndex - 1);
			}
			else if (value < globalIndex)
			{
				Unget(globalIndex - value);
			}
		}
	}

	/// <summary>
	/// 返回当前是否位于行首。
	/// </summary>
	public bool IsLineStart
	{
		get
		{
			if (index <= 0)
			{
				return current.IsLineStart;
			}
			int idx = index - 1;
			char ch = current.Buffer[idx];
			if (!IsLineBreak(ch))
			{
				return false;
			}
			if (ch == '\r')
			{
				if (index == length)
				{
					SourceBuffer cur = current;
					// 达到当前缓冲区结尾，加载下一缓冲区
					if (cur == last)
					{
						// 下一块缓冲区没有数据，需要从基础字符读取器中读取。
						if (PrepareBuffer() == 0)
						{
							return true;
						}
						cur = last;
					}
					else
					{
						// 下一块缓冲区有数据，直接后移。
						cur = cur.Next;
					}
					ch = cur.Buffer[0];
				}
				else
				{
					ch = current.Buffer[index];
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
	/// 开启行列定位功能，允许通过 <see cref="GetPosition"/> 获取指定索引的行列位置。
	/// 需要在读取字符之前设置。
	/// </summary>
	/// <param name="tabSize">Tab 的宽度。</param>
	public void UseLineLocator(int tabSize = 4)
	{
		locator ??= new LineLocator(tabSize);
	}

	/// <summary>
	/// 返回指定索引的行列位置，需要提前 <see cref="UseLineLocator"/>。
	/// </summary>
	/// <param name="index">要检查行列位置的索引。</param>
	/// <returns>指定索引的行列位置。</returns>
	/// <exception cref="InvalidOperationException"></exception>
	public LinePosition GetPosition(int index)
	{
		if (locator == null)
		{
			throw new InvalidOperationException(Resources.GetPositionBeforeUse);
		}
		return locator.GetPosition(index);
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
		if (reader != null)
		{
			reader.Dispose();
			reader = null;
			builder.Clear();
		}
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
		if (globalIndex >= End)
		{
			return InvalidCharacter;
		}
		if (index == length && !NextBuffer())
		{
			return InvalidCharacter;
		}
		return current.Buffer[index];
	}

	/// <summary>
	/// 返回文本读取器中之后的 <paramref name="idx"/> 索引的字符，但不使用它。<c>Peek(0)</c> 等价于 <see cref="Peek()"/>。
	/// </summary>
	/// <param name="idx">要读取的索引。</param>
	/// <returns>文本读取器中之后的 <paramref name="idx"/> 索引的字符，或为 <see cref="InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <exception cref="ObjectDisposedException">当前 <see cref="SourceReader"/> 已关闭。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="idx"/> 小于 <c>0</c>。</exception>
	public char Peek(int idx)
	{
		if (idx < 0)
		{
			throw CommonExceptions.ArgumentNegative(idx);
		}
		else if (idx == 0)
		{
			return Peek();
		}
		if (globalIndex + idx >= End)
		{
			return InvalidCharacter;
		}
		SourceBuffer temp = current;
		int tempLen = length;
		idx += index;
		while (true)
		{
			if (idx >= tempLen)
			{
				idx -= tempLen;
				if (temp == last && (tempLen = PrepareBuffer()) == 0)
				{
					// 没有可读数据了，返回。
					return InvalidCharacter;
				}
				temp = temp.Next;
			}
			else
			{
				return temp.Buffer[idx];
			}
		}
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
		if (globalIndex >= End)
		{
			return InvalidCharacter;
		}
		if (index == length && !NextBuffer())
		{
			return InvalidCharacter;
		}
		globalIndex++;
		return current.Buffer[index++];
	}

	/// <summary>
	/// 读取文本读取器中之后的 <paramref name="idx"/> 索引的字符，并使该字符的位置提升。
	/// <c>Read(0)</c> 等价于 <see cref="Read()"/>。
	/// </summary>
	/// <param name="idx">要读取的索引。</param>
	/// <returns>文本读取器中之后的 <paramref name="idx"/> 索引的字符，
	/// 或为 <see cref="InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <exception cref="ObjectDisposedException">当前 <see cref="SourceReader"/> 已关闭。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="idx"/> 小于 <c>0</c>。</exception>
	public char Read(int idx)
	{
		if (idx < 0)
		{
			throw CommonExceptions.ArgumentNegative(idx);
		}
		else if (idx == 0)
		{
			return Read();
		}
		if (globalIndex + idx >= End)
		{
			return InvalidCharacter;
		}
		while (true)
		{
			if (idx >= length - index)
			{
				globalIndex += length - index;
				idx -= length - index;
				index = length;
				if (!NextBuffer())
				{
					// 没有数据了，返回。
					index = length;
					return InvalidCharacter;
				}
			}
			else
			{
				globalIndex += idx + 1;
				index += idx;
				return current.Buffer[index++];
			}
		}
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
		if (current != first)
		{
			if (index <= 0)
			{
				PrevBuffer();
			}
			globalIndex--;
			index--;
			return true;
		}
		if (index > firstIndex)
		{
			globalIndex--;
			index--;
			return true;
		}
		return false;
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
		else if (count == 1)
		{
			return Unget() ? 1 : 0;
		}
		int backCount = 0;
		while (true)
		{
			if (current == first)
			{
				int charCnt = index - firstIndex;
				if (count > charCnt)
				{
					backCount += charCnt;
					index = firstIndex;
				}
				else
				{
					backCount += count;
					index -= count;
				}
				break;
			}
			if (count > index)
			{
				backCount += index;
				count -= index;
				PrevBuffer();
			}
			else
			{
				backCount += count;
				index -= count;
				break;
			}
		}
		globalIndex -= backCount;
		return backCount;
	}

	/// <summary>
	/// 返回当前位置之前的数据。
	/// </summary>
	/// <returns>当前位置之前的数据。</returns>
	public string ReadedText()
	{
		return ReadedText(false);
	}

	/// <summary>
	/// 将当前位置之前的数据全部丢弃，之后的 <see cref="Unget()"/> 操作至多回退到当前位置。
	/// </summary>
	public void Drop()
	{
		while (first != current)
		{
			startIndex += BufferSize - firstIndex;
			firstIndex = 0;
			first = first.Next;
		}
		startIndex += index - firstIndex;
		firstIndex = index;
	}

	/// <summary>
	/// 将当前位置之前的数据全部丢弃，并返回被丢弃的数据。之后的 <see cref="Unget()"/> 操作至多回退到当前位置。
	/// </summary>
	/// <returns>当前位置之前的数据。</returns>
	public string Accept()
	{
		return ReadedText(true);
	}

	/// <summary>
	/// 返回当前位置之前的数据。
	/// </summary>
	/// <param name="save">是否需要保存位置。</param>
	/// <returns>当前位置之前的数据。</returns>
	private string ReadedText(bool save)
	{
		InitBuilder();
		// 将字符串复制到 StringBuilder 中。
		SourceBuffer buf = first;
		int fIndex = firstIndex;
		int start = 0;
		while (buf != current)
		{
			CopyToBuilder(start, buf, fIndex, BufferSize - fIndex);
			start += BufferSize - fIndex;
			fIndex = 0;
			buf = buf.Next;
		}
		CopyToBuilder(start, buf, fIndex, index - fIndex);
		builderCopiedLen = start + index - fIndex;
		builder.Length = builderCopiedLen;
		if (save)
		{
			first = buf;
			firstIndex = index;
			startIndex += builderCopiedLen;
		}
		return builder.ToString();
	}

	/// <summary>
	/// 将当前位置之前的数据全部丢弃，并以 <see cref="Cyjb.Text.Token{T}"/> 的形式返回被丢弃的数据。
	/// 之后的 <see cref="Unget()"/> 操作至多回退到当前位置。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
	/// <param name="kind">返回的 <see cref="Cyjb.Text.Token{T}"/> 的标识符。</param>
	/// <param name="value"><see cref="Cyjb.Text.Token{T}"/> 的值。</param>
	/// <returns>当前位置之前的数据。</returns>
	public Token<T> AcceptToken<T>(T kind, object? value = null)
		where T : struct
	{
		int start = startIndex;
		return new Token<T>(kind, Accept(), new TextSpan(start, startIndex), value);
	}

	/// <summary>
	/// 初始化构造字符串的 <see cref="StringBuilder"/>。
	/// </summary>
	private void InitBuilder()
	{
		if (builderIndex != startIndex)
		{
			builder.Clear();
			builderIndex = startIndex;
			builderCopiedLen = 0;
		}
	}

	/// <summary>
	/// 将指定缓冲区中从指定索引开始，指定长度的字符串复制到 <see cref="builder"/> 中。
	/// </summary>
	/// <param name="index">当前的字符位置。</param>
	/// <param name="buffer">要复制字符串的缓冲区。</param>
	/// <param name="start">要复制的起始长度。</param>
	/// <param name="len">要复制的长度。</param>
	private void CopyToBuilder(int index, SourceBuffer buffer, int start, int len)
	{
		if (builderCopiedLen == index)
		{
			builder.Append(buffer.Buffer, start, len);
			builderCopiedLen += len;
		}
		else if ((index += len) > builderCopiedLen)
		{
			int l = index - builderCopiedLen;
			builder.Append(buffer.Buffer, start + len - l, l);
			builderCopiedLen = index;
		}
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
		SourceMark mark = new(globalIndex);
		int index = marks.BinarySearch(mark);
		if (index < 0)
		{
			index = ~index;
		}
		marks.Insert(index, mark);
		return mark;
	}

	/// <summary>
	/// 释放指定的源文件位置标记。
	/// </summary>
	/// <param name="mark">要释放的位置标记。</param>
	public void Release(SourceMark? mark)
	{
		if (mark == null || !mark.Valid)
		{
			return;
		}
		int index = marks.BinarySearch(mark);
		if (index < 0)
		{
			return;
		}
		// index 可能是任何一个匹配项，因此需要在双向查找一下。
		for (int i = index; i >= 0 && marks[i].Index == mark.Index; i--)
		{
			if (marks[i] == mark)
			{
				mark.Valid = false;
				marks.RemoveAt(i);
				return;
			}
		}
		for (int i = index + 1; i < marks.Count && marks[i].Index == mark.Index; i++)
		{
			if (marks[i] == mark)
			{
				mark.Valid = false;
				marks.RemoveAt(i);
				return;
			}
		}
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
	public string ReadBlock(int index, int count)
	{
		int minIndex = marks.Count == 0 ? startIndex : marks[0].Index;
		// 检查 index 和 count 是否在当前范围内。
		if (index < minIndex)
		{
			throw CommonExceptions.ArgumentOutOfRange(index);
		}
		if (index + count > globalIndex)
		{
			throw CommonExceptions.ArgumentOutOfRange(count);
		}
		if (count == 0)
		{
			return string.Empty;
		}
		StringBuilder builder = new(count);
		// 将字符串复制到 StringBuilder 中。
		SourceBuffer buffer = current;
		while (index < buffer.StartIndex)
		{
			buffer = buffer.Prev;
		}
		int start = index - buffer.StartIndex;
		while (buffer != current && count >= BufferSize)
		{
			builder.Append(buffer.Buffer, start, BufferSize - start);
			count -= BufferSize - start;
			start = 0;
			buffer = buffer.Next;
		}
		if (count > 0)
		{
			builder.Append(buffer.Buffer, start, count);
		}
		return builder.ToString();
	}

	/// <summary>
	/// 读取指定标记间的文本。
	/// </summary>
	/// <param name="start">起始标记（包含）。</param>
	/// <param name="end">结束标记（不包含）。</param>
	/// <returns>指定标记间的文本。</returns>
	/// <exception cref="ArgumentException">传入的标记已被释放。</exception>
	/// <exception cref="ArgumentOutOfRangeException">起始和结束标记的顺序不正确。</exception>
	public string ReadBlock(SourceMark start, SourceMark end)
	{
		if (!start.Valid)
		{
			throw new ArgumentException(Resources.InvalidSourceMark, nameof(start));
		}
		if (!end.Valid)
		{
			throw new ArgumentException(Resources.InvalidSourceMark, nameof(end));
		}
		if (start.Index > end.Index)
		{
			throw CommonExceptions.ArgumentMinMaxValue(nameof(start), nameof(end));
		}
		return ReadBlock(start.Index, end.Index - start.Index);
	}

	#endregion // 标记

	#region 缓冲区操作

	/// <summary>
	/// 切换到下一块缓冲区。如果没有有效的数据，则从基础字符读取器中读取字符，并填充到缓冲区中。
	/// </summary>
	/// <returns>如果切换成功，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	private bool NextBuffer()
	{
		if (current == last)
		{
			// 下一块缓冲区没有数据，需要从基础字符读取器中读取。
			if (PrepareBuffer() == 0)
			{
				return false;
			}
			length = lastLength;
			current = last;
		}
		else
		{
			// 下一块缓冲区有数据，直接后移。
			current = current.Next;
			if (current == last)
			{
				length = lastLength;
			}
		}
		index = 0;
		return length > 0;
	}

	/// <summary>
	/// 从基础字符读取器中读取字符，并填充到新的缓冲区中。
	/// </summary>
	/// <returns>从基础字符读取器中读取的字符数量。</returns>
	private int PrepareBuffer()
	{
		if (reader == null)
		{
			throw CommonExceptions.StreamClosed(nameof(SourceReader));
		}
		if (reader.Peek() == -1)
		{
			return 0;
		}
		if (length > 0)
		{
			int minMark = marks.Count == 0 ? int.MaxValue : marks[0].Index;
			if (last.Next == first || last.Next.StartIndex + BufferSize > minMark)
			{
				// 没有可用的空缓冲区，或者缓冲区需要未标记保留，则需要新建立一块。
				SourceBuffer buffer = new(current, last.Next);
				last.Next.Prev = buffer;
				last.Next = buffer;
			}
			last = last.Next;
			last.StartIndex = last.Prev.StartIndex + BufferSize;
		}
		else
		{
			// len 为 0 应仅当 last == current 时。
		}
		// 检查前一缓冲区最后位置的字符，确定是否是行首。
		if (last == first)
		{
			// 是行首。
			last.IsLineStart = true;
		}
		else
		{
			SourceBuffer prev = last.Prev;
			char lastChar = prev.Buffer[BufferSize - 1];
			// 兼容 \r\n 的场景
			if (IsLineBreak(lastChar) && (lastChar != '\r' || last.Buffer[0] != '\n'))
			{
				last.IsLineStart = true;
			}
			else
			{
				last.IsLineStart = false;
			}
		}
		lastLength = reader.ReadBlock(last.Buffer, 0, BufferSize);
		locator?.Read(last.Buffer.AsSpan(0, lastLength));
		if (length == 0)
		{
			length = lastLength;
		}
		return lastLength;
	}

	/// <summary>
	/// 切换到上一块缓冲区。
	/// </summary>
	private void PrevBuffer()
	{
		current = current.Prev;
		index = length = BufferSize;
	}

	/// <summary>
	/// 表示 <see cref="SourceReader"/> 的字符缓冲区。
	/// </summary>
	private sealed class SourceBuffer
	{
		/// <summary>
		/// 字符缓冲区。
		/// </summary>
		public readonly char[] Buffer = new char[BufferSize];
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
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			return $"{{{new string(Buffer.Where(ch => ch != '\0').ToArray())}}}";
		}
	}

	#endregion // 缓冲区操作

}
