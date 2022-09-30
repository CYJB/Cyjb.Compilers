using Cyjb.Compilers;

namespace Cyjb.Text;

/// <summary>
/// 表示词法分析错误。
/// </summary>
public class TokenizeError
{
	/// <summary>
	/// 行定位器。
	/// </summary>
	private readonly LineLocator? locator;

	/// <summary>
	/// 使用词法分析错误的相关信息初始化 <see cref="TokenizeError"/> 类的新实例。
	/// </summary>
	/// <param name="text">词法单元的文本。</param>
	/// <param name="span">词法单元的范围。</param>
	/// <param name="locator">行定位器。</param>
	public TokenizeError(string text, TextSpan span, LineLocator? locator)
	{
		Text = text;
		Span = span;
		this.locator = locator;
	}

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

	#region IEquatable<Token> 成员

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public bool Equals(TokenizeError other)
	{
		return Text == other.Text && Span == other.Span;
	}

	/// <summary>
	/// 返回当前对象是否等于另一对象。
	/// </summary>
	/// <param name="obj">要与当前对象进行比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="obj"/>，则为 true；否则为 false。</returns>
	public override bool Equals(object? obj)
	{
		if (obj is TokenizeError other)
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
		return HashCode.Combine(Text, Span);
	}

	/// <summary>
	/// 返回指定的 <see cref="Token{T}"/> 是否相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator ==(TokenizeError left, TokenizeError right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// 返回指定的 <see cref="Token{T}"/> 是否不相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator !=(TokenizeError left, TokenizeError right)
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
		return Resources.UnrecognizedToken(Text.UnicodeEscape(false), Span);
	}
}
