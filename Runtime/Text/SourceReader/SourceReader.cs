using System.Diagnostics.CodeAnalysis;
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
public abstract class SourceReader : IDisposable
{
	/// <summary>
	/// 用于表示到达流结尾的字符。
	/// </summary>
	/// <remarks>使用非 Unicode 字符 0xFFFF。</remarks>
	public const char InvalidCharacter = char.MaxValue;
	/// <summary>
	/// 空的源文件读取器。
	/// </summary>
	internal static readonly SourceReader Empty = new StringSourceReader(string.Empty);

	/// <summary>
	/// 使用指定的文本内容创建 <see cref="SourceReader"/> 类的新实例。
	/// </summary>
	/// <param name="source">文本内容。</param>
	/// <param name="start">要读取的起始位置。</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> 小于 <c>0</c>。</exception>
	public static SourceReader Create(string source, int start = 0)
	{
		ArgumentNullException.ThrowIfNull(source);
		if (start < 0)
		{
			throw CommonExceptions.ArgumentNegative(start);
		}
		if (start == 0)
		{
			return new StringSourceReader(source);
		}
		else
		{
			return new StringViewSourceReader(source, start, source.Length - start);
		}
	}

	/// <summary>
	/// 使用指定的文本内容创建 <see cref="SourceReader"/> 类的新实例。
	/// </summary>
	/// <param name="source">文本内容。</param>
	/// <param name="start">要读取的起始位置。</param>
	/// <param name="length">要读取的文本长度。</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> 小于 <c>0</c>。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> + <paramref name="length"/>
	/// 表示的位置不在字符串范围内。</exception>
	public static SourceReader Create(string source, int start, int length)
	{
		ArgumentNullException.ThrowIfNull(source);
		if (start < 0)
		{
			throw CommonExceptions.ArgumentNegative(start);
		}
		if (start + length < 0 || start + length >= source.Length)
		{
			throw CommonExceptions.ArgumentNegative(length);
		}
		if (start == 0 && length == source.Length)
		{
			return new StringSourceReader(source);
		}
		else
		{
			return new StringViewSourceReader(source, start, length);
		}
	}

	/// <summary>
	/// 使用指定的文本视图内容创建 <see cref="SourceReader"/> 类的新实例。
	/// </summary>
	/// <param name="source">文本视图内容。</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	public static SourceReader Create(StringView source)
	{
		ArgumentNullException.ThrowIfNull(source);
		source.GetOrigin(out string text, out int start, out int length);
		return new StringViewSourceReader(text, start, length);
	}

	/// <summary>
	/// 使用指定的字符读取器创建 <see cref="SourceReader"/> 类的新实例。
	/// </summary>
	/// <param name="reader">用于读取源文件的字符读取器。</param>
	/// <param name="bufferSize">读取文本的缓冲区尺寸。
	/// 默认为 <c>0</c>，表示不限制缓冲区大小，内存消耗较大但性能高；
	/// 其他值会限制每块缓冲区的大小，当不在使用相关 <see cref="Token{T}"/> 后可以释放缓冲区节约内容，
	/// 缓冲区不宜设置太小，否则容易影响性能，可以考虑设置为 0x1000 或更高。
	/// </param>
	public static SourceReader Create(TextReader reader, int bufferSize = 0)
	{
		ArgumentNullException.ThrowIfNull(reader);
		if (reader is StringReader || bufferSize <= 0 || bufferSize == int.MaxValue)
		{
			// StringReader 已经包含了完整的字符串，没有分块读取的意义。
			return new StringSourceReader(reader.ReadToEnd());
		}
		else
		{
			return new PartialSourceReader(reader, bufferSize);
		}
	}

	/// <summary>
	/// 当前读取的位置。
	/// </summary>
	protected int curIndex = 0;
	/// <summary>
	/// 读取的起始位置。
	/// </summary>
	private int startIndex = 0;
	/// <summary>
	/// 字符被释放的起始位置。
	/// </summary>
	private int freedIndex = 0;
	/// <summary>
	/// 行列定位器。
	/// </summary>
	protected LineLocator? sourceLocator;

	/// <summary>
	/// 初始化 <see cref="SourceReader"/> 类的新实例。
	/// </summary>
	protected SourceReader() { }

	/// <summary>
	/// 获取关联到的行列定位器。
	/// </summary>
	public LineLocator? Locator => sourceLocator;

	/// <summary>
	/// 获取或设置当前的字符索引。
	/// </summary>
	/// <value>当前的字符索引，该索引从零开始。设置索引时，不能达到被丢弃的字符，或者超出文件结尾。</value>
	public int Index
	{
		get => curIndex;
		set
		{
			if (curIndex != value)
			{
				SetIndex(value);
			}
		}
	}

	/// <summary>
	/// 获取或设置读取的起始字符索引。
	/// </summary>
	/// <value>读取的起始字符索引，该索引从零开始。设置索引时，不能达到被释放的字符，或者超出文件结尾。</value>
	/// <remarks>在调用 <see cref="Unget()"/>、<see cref="Accept"/> 等方法时，均不能超过
	/// <see cref="StartIndex"/> 表示的索引。</remarks>
	public int StartIndex
	{
		get => startIndex;
		set
		{
			if (value >= freedIndex && value <= curIndex)
			{
				startIndex = value;
			}
		}
	}

	/// <summary>
	/// 返回当前是否位于行首。
	/// </summary>
	public abstract bool IsLineStart { get; }

	/// <summary>
	/// 开启行列定位功能，允许通过 <see cref="GetPosition"/> 获取指定索引的行列位置。
	/// 需要在读取字符之前设置。
	/// </summary>
	/// <param name="tabSize">Tab 的宽度。</param>
	/// <returns>当前源读取器。</returns>
	[MemberNotNull(nameof(sourceLocator))]
	public virtual SourceReader UseLineLocator(int tabSize = 4)
	{
		sourceLocator ??= new LineLocator(tabSize);
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
		if (sourceLocator == null)
		{
			throw new InvalidOperationException(Resources.GetPositionBeforeUse);
		}
		return sourceLocator.GetPosition(index);
	}

	/// <summary>
	/// 返回指定文本范围的行列位置范围，需要提前 <see cref="UseLineLocator"/>。
	/// </summary>
	/// <param name="span">要检查行列位置的文本范围。</param>
	/// <returns>指定文本范围的行列位置范围。</returns>
	/// <exception cref="InvalidOperationException">未提前调用 <see cref="UseLineLocator"/>。</exception>
	public LinePositionSpan GetLinePositionSpan(TextSpan span)
	{
		if (sourceLocator == null)
		{
			throw new InvalidOperationException(Resources.GetPositionBeforeUse);
		}
		return sourceLocator.GetSpan(span);
	}

	/// <summary>
	/// 关闭 <see cref="SourceReader"/> 对象和基础字符读取器，并释放与读取器关联的所有系统资源。
	/// </summary>
	public void Close()
	{
		Dispose();
	}

	/// <summary>
	/// 返回下一个可用的字符，但不使用它。
	/// </summary>
	/// <returns>表示下一个要读取的字符的整数，或者如果没有要读取的字符，则为 <see cref="InvalidCharacter"/>。</returns>
	/// <overloads>
	/// <summary>
	/// 返回之后可用的字符，但不使用它。
	/// </summary>
	/// </overloads>
	public abstract char Peek();

	/// <summary>
	/// 返回文本读取器中之后的 <paramref name="offset"/> 偏移的字符，但不使用它。
	/// <c>Peek(0)</c> 等价于 <see cref="Peek()"/>。
	/// </summary>
	/// <param name="offset">要读取的偏移。</param>
	/// <returns>文本读取器中之后的 <paramref name="offset"/> 偏移的字符，
	/// 或为 <see cref="InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <exception cref="ObjectDisposedException">当前 <see cref="SourceReader"/> 已关闭。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> 小于 <c>0</c>。</exception>
	public abstract char Peek(int offset);

	/// <summary>
	/// 读取文本读取器中的下一个字符并使该字符的位置提升一个字符。
	/// </summary>
	/// <returns>文本读取器中的下一个字符，或为 <see cref="InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <overloads>
	/// <summary>
	/// 返回之后可用的字符，并使该字符的位置提升。
	/// </summary>
	/// </overloads>
	public abstract char Read();

	/// <summary>
	/// 读取文本读取器中之后的 <paramref name="offset"/> 偏移的字符，并使该字符的位置提升。
	/// <c>Read(0)</c> 等价于 <see cref="Read()"/>。
	/// </summary>
	/// <param name="offset">要读取的偏移。</param>
	/// <returns>文本读取器中之后的 <paramref name="offset"/> 偏移的字符，
	/// 或为 <see cref="InvalidCharacter"/>（如果没有更多的可用字符）。</returns>
	/// <exception cref="ObjectDisposedException">当前 <see cref="SourceReader"/> 已关闭。</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> 小于 <c>0</c>。</exception>
	public abstract char Read(int offset);

	/// <summary>
	/// 读取到当前行的结束位置。
	/// </summary>
	/// <param name="containsLineSeparator">是否包含行分隔符。</param>
	/// <returns>文本读取器到行末的文本。</returns>
	public abstract StringView ReadLine(bool containsLineSeparator = true);

	/// <summary>
	/// 读取到文本的结束位置。
	/// </summary>
	/// <returns>文本读取器到结束位置的文本。</returns>
	public abstract StringView ReadToEnd();

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
		if (curIndex > startIndex)
		{
			SetIndex(curIndex - 1);
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
		count = Math.Min(count, curIndex - startIndex);
		SetIndex(curIndex - count);
		return count;
	}

	/// <summary>
	/// 从当前位置查找指定字符的偏移。
	/// </summary>
	/// <param name="ch">要查找的字符。</param>
	/// <returns>如果找到字符，则为 <paramref name="ch"/> 从当前位置开始的偏移；
	/// 如果未找到，则返回 <c>-1</c>。</returns>
	public int IndexOf(char ch)
	{
		return IndexOf(ch, 0);
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
	public abstract int IndexOf(char ch, int start);

	/// <summary>
	/// 从当前位置查找指定字符数组中的任意字符的偏移，使用指定的起始偏移开始。
	/// </summary>
	/// <param name="anyOf">要查找的字符数组。</param>
	/// <returns>如果找到 <paramref name="anyOf"/> 中的任意字符，则为该字符从当前位置开始的偏移；
	/// 如果未找到，则返回 <c>-1</c>。</returns>
	public int IndexOfAny(char[] anyOf)
	{
		return IndexOfAny(anyOf, 0);
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
	public abstract int IndexOfAny(char[] anyOf, int start);

	/// <summary>
	/// 返回从 <see cref="StartIndex"/> 到 <see cref="Index"/> 的数据。
	/// </summary>
	/// <returns>从 <see cref="StartIndex"/> 到 <see cref="Index"/> 的数据。</returns>
	public StringView GetReadedText()
	{
		return ReadBlockInternal(startIndex, curIndex - startIndex);
	}

	/// <summary>
	/// 读取指定范围的文本。
	/// </summary>
	/// <param name="index">起始索引。</param>
	/// <param name="count">要读取的长度。</param>
	/// <returns>指定范围的文本。</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> 所在的位置已被丢弃。
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> + <paramref name="count"/>
	/// 超出当前已读取的字符范围。</exception>
	public StringView GetText(int index, int count)
	{
		// 检查 index 和 count 是否在当前范围内。
		if (index < freedIndex)
		{
			throw CommonExceptions.ArgumentOutOfRange(index);
		}
		if (index + count > curIndex)
		{
			throw CommonExceptions.ArgumentOutOfRange(count);
		}
		if (count == 0)
		{
			return StringView.Empty;
		}
		return ReadBlockInternal(index, count);
	}

	/// <summary>
	/// 返回 <see cref="StartIndex"/> 到 <see cref="Index"/> 之间的数据，并将 <see cref="StartIndex"/>
	/// 设置为当前位置。之后的 <see cref="Unget()"/> 操作至多回退到当前位置。
	/// </summary>
	/// <returns>当前位置之前的数据。</returns>
	public StringView Accept()
	{
		StringView text = ReadBlockInternal(startIndex, curIndex - startIndex);
		startIndex = curIndex;
		return text;
	}

	/// <summary>
	/// 以 <see cref="Token{T}"/> 的形式返回 <see cref="StartIndex"/> 到 <see cref="Index"/>
	/// 之间的数据，并将 <see cref="StartIndex"/> 设置为当前位置。
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
			tokenSpan = new TextSpan(startIndex, curIndex);
		}
		return new Token<T>(kind, Accept(), tokenSpan, value);
	}

	/// <summary>
	/// 将当前位置之前的数据全部丢弃，会释放相关内存，之后的 <see cref="StartIndex"/> 操作至多回退到当前位置。
	/// </summary>
	public void Drop()
	{
		startIndex = curIndex;
	}

	/// <summary>
	/// 释放当前位置之前的数据，之后的 <see cref="StartIndex"/> 操作至多设置到当前位置。
	/// </summary>
	public void Free()
	{
		freedIndex = curIndex;
		startIndex = curIndex;
		Free(freedIndex);
	}

	/// <summary>
	/// 释放指定位置之前的数据，之后的 <see cref="StartIndex"/> 操作至多设置到 <paramref name="index"/>。
	/// </summary>
	/// <param name="index">要释放数据的索引。</param>
	public void Free(int index)
	{
		if (index <= freedIndex)
		{
			return;
		}
		freedIndex = index;
		if (startIndex < index)
		{
			startIndex = index;
		}
		Free(freedIndex);
	}

	#region IDisposable 成员

	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	/// <overloads>
	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	/// </overloads>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	/// <param name="disposing">是否释放托管资源。</param>
	protected virtual void Dispose(bool disposing) { }

	#endregion // IDisposable 成员

	/// <summary>
	/// 设置当前的字符索引。
	/// </summary>
	/// <param name="value">当前的字符索引，该索引从零开始。设置索引时，不能达到被丢弃的字符，或者超出文件结尾。</param>
	protected virtual void SetIndex(int value)
	{
		if (value < curIndex)
		{
			if (value < startIndex)
			{
				value = startIndex;
			}
			curIndex = value;
		}
		else
		{
			curIndex = value;
		}
	}

	/// <summary>
	/// 读取指定范围的文本。
	/// </summary>
	/// <param name="start">起始索引。</param>
	/// <param name="count">要读取的文本长度。</param>
	/// <returns>读取到的文本。</returns>
	/// <remarks>调用方确保 <paramref name="start"/> 和 <paramref name="count"/> 在有效范围之内。</remarks>
	protected abstract StringView ReadBlockInternal(int start, int count);

	/// <summary>
	/// 释放指定索引之前的字符，释放后的字符无法再被读取。
	/// </summary>
	/// <param name="index">要释放的字符起始索引。</param>
	protected virtual void FreeInternal(int index) { }

}
