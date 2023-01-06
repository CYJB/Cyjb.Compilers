using System.Text;

namespace Cyjb.Text;

public partial struct Token<T> : IEquatable<Token<T>>
{
	/// <summary>
	/// 拼接指定的词法单元。
	/// </summary>
	/// <param name="first">要拼接的第一个词法单元。</param>
	/// <param name="second">要拼接的第二个词法单元。</param>
	/// <returns>两个词法单元的拼接结果。</returns>
	/// <remarks>会使用首个词法单元的类型和值。</remarks>
	public static Token<T> Combine(Token<T> first, Token<T> second)
	{
		return first with
		{
			Text = first.Text + second.Text,
			Span = new TextSpan(first.Span.Start, second.Span.End),
		};
	}

	/// <summary>
	/// 拼接指定的词法单元。
	/// </summary>
	/// <param name="tokens">要拼接的词法单元。</param>
	/// <returns>指定词法单元的拼接结果。</returns>
	/// <remarks>会使用首个词法单元的类型和值。</remarks>
	public static Token<T> Combine(params Token<T>[] tokens)
	{
		if (tokens.Length == 0)
		{
			return Token<T>.GetEndOfFile(0);
		}
		Token<T> token = tokens[0];
		if (tokens.Length == 1)
		{
			return token;
		}
		string text = string.Concat(tokens.Select(token => token.Text));
		return token with
		{
			Text = text,
			Span = new TextSpan(token.Span.Start, tokens[^1].Span.End),
		};
	}

	/// <summary>
	/// 拼接指定的词法单元。
	/// </summary>
	/// <param name="tokens">要拼接的词法单元。</param>
	/// <returns>指定词法单元的拼接结果。</returns>
	/// <remarks>会使用首个词法单元的类型和值。</remarks>
	public static Token<T> Combine(IEnumerable<Token<T>> tokens)
	{
		Token<T> first = new();
		Token<T> last = new();
		StringBuilder text = new();
		bool isFirst = true;
		foreach (Token<T> token in tokens)
		{
			if (isFirst)
			{
				first = token;
				isFirst = false;
			}
			last = token;
			text.Append(token.Text);
		}
		if (isFirst)
		{
			return Token<T>.GetEndOfFile(0);
		}
		return first with
		{
			Text = text.ToString(),
			Span = new TextSpan(first.Span.Start, last.Span.End),
		};
	}
}
