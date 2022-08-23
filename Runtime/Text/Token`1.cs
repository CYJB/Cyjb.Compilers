namespace Cyjb.Text;

/// <summary>
/// 表示一个词法单元。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <remarks><typeparamref name="T"/> 必须的枚举类型，使用该类型的特殊值 
/// <c>-1</c> 用于表示文件结束，<c>-2</c> 表示语法产生式的错误。</remarks>
public struct Token<T> : IEquatable<Token<T>>
	where T : struct
{
	/// <summary>
	/// 表示文件结束的值。
	/// </summary>
	private static readonly object EndOfFileValue = new();

	/// <summary>
	/// 返回表示文件结束的词法单元。
	/// </summary>
	/// <param name="index">文件结束的位置。</param>
	/// <returns>表示文件结束的词法单元。</returns>
	public static Token<T> GetEndOfFile(int index)
	{
		return new Token<T>(default, string.Empty, new TextSpan(index, index), EndOfFileValue);
	}

	/// <summary>
	/// 行定位器。
	/// </summary>
	private readonly LineLocator? locator;

	/// <summary>
	/// 使用词法单元的相关信息初始化 <see cref="Token{T}"/> 类的新实例。
	/// </summary>
	/// <param name="kind">词法单元的类型。</param>
	/// <param name="text">词法单元的文本。</param>
	/// <param name="span">词法单元的范围。</param>
	/// <param name="value">词法单元的值。</param>
	public Token(T kind, string text, TextSpan span, object? value = null)
	{
		Kind = kind;
		Text = text;
		Span = span;
		Value = value;
		locator = null;
	}

	/// <summary>
	/// 使用词法单元的相关信息初始化 <see cref="Token{T}"/> 类的新实例。
	/// </summary>
	/// <param name="kind">词法单元的类型。</param>
	/// <param name="text">词法单元的文本。</param>
	/// <param name="span">词法单元的范围。</param>
	/// <param name="locator">行定位器。</param>
	/// <param name="value">词法单元的值。</param>
	public Token(T kind, string text, TextSpan span, LineLocator? locator, object? value = null)
	{
		Kind = kind;
		Text = text;
		Span = span;
		Value = value;
		this.locator = locator;
	}

	/// <summary>
	/// 获取词法单元的类型。
	/// </summary>
	/// <value>词法单元的类型。</value>
	public T Kind { get; }
	/// <summary>
	/// 获取词法单元的文本。
	/// </summary>
	/// <value>词法单元的文本。</value>
	public string Text { get; }
	/// <summary>
	/// 获取词法单元的范围。
	/// </summary>
	/// <value>词法单元的范围。</value>
	public TextSpan Span { get; }
	/// <summary>
	/// 获取词法单元的行列位置范围。
	/// </summary>
	/// <value>词法单元的行列位置范围。</value>
	public LinePositionSpan LinePositionSpan
	{
		get
		{
			int start = Span.Start;
			int end = Span.End;
			if (locator == null)
			{
				return new LinePositionSpan(new LinePosition(1, start), new LinePosition(1, end));
			}
			else
			{
				return new LinePositionSpan(locator.GetPosition(start), locator.GetPosition(end));
			}
		}
	}
	/// <summary>
	/// 获取词法单元的值。
	/// </summary>
	/// <value>词法单元的值。</value>
	public object? Value { get; }
	/// <summary>
	/// 获取当前是否是表示文件结束的词法单元。
	/// </summary>
	public bool IsEndOfFile => Value == EndOfFileValue;

	#region IEquatable<Token> 成员

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public bool Equals(Token<T> other)
	{
		return EqualityComparer<T>.Default.Equals(Kind, other.Kind) && Text == other.Text && Span == other.Span;
	}

	/// <summary>
	/// 返回当前对象是否等于另一对象。
	/// </summary>
	/// <param name="obj">要与当前对象进行比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="obj"/>，则为 true；否则为 false。</returns>
	public override bool Equals(object? obj)
	{
		if (obj is Token<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	/// <summary>
	/// 返回当前对象的哈希值。
	/// </summary>
	/// <returns>当前对象的哈希值。</returns>
	public override int GetHashCode()
	{
		return HashCode.Combine(Kind, Text, Span);
	}

	/// <summary>
	/// 返回指定的 <see cref="Token{T}"/> 是否相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator ==(Token<T> left, Token<T> right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// 返回指定的 <see cref="Token{T}"/> 是否不相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator !=(Token<T> left, Token<T> right)
	{
		return !left.Equals(right);
	}

	#endregion // IEquatable<Token> 成员

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		if (IsEndOfFile)
		{
			return "<<EOF>>";
		}
		return $"{Kind} \"{Text}\" at {Span}";
	}
}
