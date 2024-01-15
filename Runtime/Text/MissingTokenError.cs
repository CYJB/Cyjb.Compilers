using System.Diagnostics;
using Cyjb.Compilers;

namespace Cyjb.Text;

/// <summary>
/// 表示缺失词法单元的错误。
/// </summary>
/// <typeparam name="T">语法节点标识符的类型，必须是一个枚举类型。</typeparam>
public class MissingTokenError<T> : TokenParseError
	where T : struct
{
	/// <summary>
	/// 缺失的词法单元。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Token<T> token;

	/// <summary>
	/// 使用缺失的词法单元初始化 <see cref="UnexpectedTokenError{T}"/> 类的新实例。
	/// </summary>
	/// <param name="token">缺失的词法单元。</param>
	public MissingTokenError(Token<T> token)
	{
		this.token = token;
	}

	/// <summary>
	/// 获取缺失的词法单元类型。
	/// </summary>
	public T Missing => token.Kind;

	/// <summary>
	/// 获取语法分析错误的范围。
	/// </summary>
	/// <value>语法分析错误的范围。</value>
	public override TextSpan Span => token.Span;

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Equals(TokenParseError? other)
	{
		if (other is MissingTokenError<T> error)
		{
			return token == error.token;
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
		return token.GetHashCode();
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		return Resources.MissingToken(Token<T>.GetDisplayName(token.Kind), Span);
	}
}
