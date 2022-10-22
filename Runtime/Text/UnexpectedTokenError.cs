using System.Diagnostics;
using System.Text;
using Cyjb.Compilers;

namespace Cyjb.Text;

/// <summary>
/// 表示发现非预期的词法单元的错误。
/// </summary>
/// <typeparam name="T">语法节点标识符的类型，必须是一个枚举类型。</typeparam>
public class UnexpectedTokenError<T> : TokenParseError
	where T : struct
{
	/// <summary>
	/// 非预期的词法单元。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Token<T> token;
	/// <summary>
	/// 预期的词法单元类型。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IReadOnlySet<T> expecting;

	/// <summary>
	/// 使用非预期的词法单元列表初始化 <see cref="UnexpectedTokenError{T}"/> 类的新实例。
	/// </summary>
	/// <param name="token">非预期的词法单元。</param>
	/// <param name="expecting">预期的词法单元类型。</param>
	public UnexpectedTokenError(Token<T> token, IReadOnlySet<T> expecting)
	{
		this.token = token;
		this.expecting = expecting;
	}

	/// <summary>
	/// 获取非预期的词法单元。
	/// </summary>
	public Token<T> Token => token;
	/// <summary>
	/// 获取预期的词法单元类型。
	/// </summary>
	public IReadOnlySet<T> Expecting => expecting;

	/// <summary>
	/// 获取语法分析错误的范围。
	/// </summary>
	/// <value>语法分析错误的范围。</value>
	public override TextSpan Span => token.Span;
	/// <summary>
	/// 获取语法分析错误的行列位置范围。
	/// </summary>
	/// <value>语法分析错误的行列位置范围。</value>
	public override LinePositionSpan LinePositionSpan => token.LinePositionSpan;

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Equals(TokenParseError? other)
	{
		if (other is UnexpectedTokenError<T> error)
		{
			return Token == error.token && Span == other.Span && expecting.SetEquals(error.expecting);
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// 返回当前对象的哈希值。
	/// </summary>
	/// <returns>当前对象的哈希值。</returns>
	public override int GetHashCode()
	{
		return HashCode.Combine(Token, Span, UnorderedHashCode.Combine(expecting));
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		StringBuilder text = new();
		bool first = true;
		foreach (T kind in expecting)
		{
			if (first)
			{
				first = false;
			}
			else
			{
				text.Append(", ");
			}
			text.Append(Token<T>.GetDisplayName(kind));
		}
		return Resources.UnexpectedToken(Token<T>.GetDisplayName(token.Kind), Span, text);
	}
}
