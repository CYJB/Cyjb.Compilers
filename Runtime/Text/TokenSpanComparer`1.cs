namespace Cyjb.Text;

/// <summary>
/// 表示一个词法单元的位置比较器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal class TokenSpanComparer<T> : IComparer<Token<T>>
	where T : struct
{
	/// <summary>
	/// 词法单元的位置比较器实例。
	/// </summary>
	public static readonly TokenSpanComparer<T> Instance = new();

	/// <summary>
	/// 比较两个对象并返回一个值，指示一个对象小于、等于还是大于另一个对象。
	/// </summary>
	/// <param name="x">要比较的第一个对象。</param>
	/// <param name="y">要比较的第二个对象。</param>
	/// <returns>一个有符号整数，指示 <paramref name="x"/> 和 <paramref name="x"/> 的相对值。</returns>
	public int Compare(Token<T>? x, Token<T>? y)
	{
		if (ReferenceEquals(x, y))
		{
			return 0;
		}
		else if (x is null)
		{
			return -1;
		}
		else if (y is null)
		{
			return 1;
		}
		return x.Span.CompareTo(y.Span);
	}
}
