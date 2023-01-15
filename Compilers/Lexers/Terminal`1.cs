using Cyjb.Compilers.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析中的终结符。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class Terminal<T>
	where T : struct
{
	/// <summary>
	/// 终结符的状态相等比较器。
	/// </summary>
	public static readonly IEqualityComparer<Terminal<T>> StateComparer = new StateEqualityComparer();

	/// <summary>
	/// 终结符的匹配信息。
	/// </summary>
	private readonly List<TerminalMatch> matches = new();

	/// <summary>
	/// 使用终结符的正则表达式初始化 <see cref="Terminal{T}"/> 类的新实例。
	/// </summary>
	/// <param name="index">终结符的索引。</param>
	/// <param name="regex">终结符对应的正则表达式。</param>
	public Terminal(int index, LexRegex regex)
	{
		Index = index;
		matches.Add(new TerminalMatch(regex));
		// 检查向前看的长度。
		LexRegex? trailingExp;
		if (regex is AnchorExp anchor && (trailingExp = anchor.TrailingExpression) != null)
		{
			int? len = trailingExp.Length;
			if (len != null)
			{
				Trailing = -len.Value;
			}
			else
			{
				len = anchor.InnerExpression.Length;
				if (len != null)
				{
					Trailing = len.Value;
				} else
				{
					Trailing = 0;
				}
			}
		}
	}

	/// <summary>
	/// 获取或设置当前终结符的索引。
	/// </summary>
	public int Index { get; set; }
	/// <summary>
	/// 获取当前终结符的匹配信息。
	/// </summary>
	public IList<TerminalMatch> Matches => matches;
	/// <summary>
	/// 获取或设置当前终结符的类型。
	/// </summary>
	public T? Kind { get; set; }
	/// <summary>
	/// 获取或设置当前终结符的值。
	/// </summary>
	public object? Value { get; set; }
	/// <summary>
	/// 获取或设置当前终结符的动作。
	/// </summary>
	/// <value>当前终结符的动作。</value>
	public Delegate? Action { get; set; }
	/// <summary>
	/// 获取终结符的向前看信息。
	/// </summary>
	/// <remarks><c>null</c> 表示不是向前看符号，正数表示前面长度固定，
	/// 负数表示后面长度固定，<c>0</c> 表示长度不固定。</remarks>
	public int? Trailing { get; }
	/// <summary>
	/// 是否使用当前终结符的最短匹配。
	/// </summary>
	public bool UseShortest { get; set; } = false;

	/// <summary>
	/// 返回当前终结符是否包含行首匹配的正则表达式。
	/// </summary>
	/// <returns>如果当前终结符包含行首匹配的正则表达式，返回 <c>true</c>；否则返回 <c>false</c>。</returns>
	public bool ContainsBeginningOfLine()
	{
		foreach (TerminalMatch match in matches)
		{
			if (match.RegularExpression is AnchorExp anchor)
			{
				if (anchor.BeginningOfLine)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 返回词法分析器的终结符数据。
	/// </summary>
	/// <returns>词法分析器的终结符数据。</returns>
	public TerminalData<T> GetData()
	{
		return new TerminalData<T>(Kind, Value, Action, Trailing, UseShortest);
	}

	/// <summary>
	/// 终结符的状态相等比较器。
	/// </summary>
	private class StateEqualityComparer : IEqualityComparer<Terminal<T>>
	{
		/// <summary>
		/// 词法单元类型的相等比较器。
		/// </summary>
		private static readonly IEqualityComparer<T?> KindComparer = EqualityComparer<T?>.Default;

		/// <summary>
		/// 返回指定对象是否相等。
		/// </summary>
		/// <param name="left">要比较的第一个对象。</param>
		/// <param name="right">要比较的第二个对象。</param>
		/// <returns>如果两个对象相等，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
		public bool Equals(Terminal<T>? left, Terminal<T>? right)
		{
			if (left == right)
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			return KindComparer.Equals(left.Kind, right.Kind) &&
			left.Value == right.Value && left.Action == right.Action &&
				left.Trailing == right.Trailing && left.UseShortest == right.UseShortest;
		}

		/// <summary>
		/// 返回指定对象的哈希值。
		/// </summary>
		/// <param name="obj">要检查的对象。</param>
		/// <returns>指定对象的哈希值。</returns>
		/// <exception cref="NotImplementedException"></exception>
		public int GetHashCode(Terminal<T> obj)
		{
			return HashCode.Combine(obj.Kind, obj.Value, obj.Action, obj.Trailing, obj.UseShortest);
		}
	}
}
