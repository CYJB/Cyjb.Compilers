using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Cyjb.Compilers.RegularExpressions;

/// <summary>
/// 表示用于词法分析的正则表达式。
/// </summary>
/// <remarks>这里的正则表达式，与 
/// <see href="https://docs.microsoft.com/zh-cn/dotnet/standard/base-types/regular-expressions">
/// .NET 正则表达式</see>不同，它是专门用于定义词法的。更多信息请参考我的系列博文 
/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerRegex.html">
/// 《C# 词法分析器（三）正则表达式》</see>。</remarks>
/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerRegex.html">
/// 《C# 词法分析器（三）正则表达式》</seealso>
public abstract class LexRegex : IEquatable<LexRegex>
{
	/// <summary>
	/// 根据给定的字符串解析正则表达式。
	/// </summary>
	/// <param name="pattern">正则表达式的模式字符串。</param>
	/// <param name="regexDef">正则表达式的定义。</param>
	/// <returns>解析得到的正则表达式。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="pattern"/> 为 <c>null</c>。</exception>
	/// <overloads>
	/// <summary>
	/// 根据给定的字符串或源文件解析正则表达式。
	/// </summary>
	/// </overloads>
	public static LexRegex Parse(string pattern, IDictionary<string, LexRegex>? regexDef = null)
	{
		ArgumentNullException.ThrowIfNull(pattern);
		return RegexParser.ParseRegex(pattern, RegexOptions.None, regexDef);
	}

	/// <summary>
	/// 根据给定的字符串解析正则表达式。
	/// </summary>
	/// <param name="pattern">正则表达式的模式字符串。</param>
	/// <param name="option">正则表达式的选项。</param>
	/// <param name="regexDef">正则表达式的定义。</param>
	/// <returns>解析得到的正则表达式。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="pattern"/> 为 <c>null</c>。</exception>
	/// <remarks>支持的正则表达式选项有 <see cref="RegexOptions.IgnoreCase"/>、
	/// <see cref="RegexOptions.Singleline"/>、<see cref="RegexOptions.IgnorePatternWhitespace"/>、
	/// <see cref="RegexOptions.ECMAScript"/> 和 <see cref="RegexOptions.CultureInvariant"/>。</remarks>
	public static LexRegex Parse(string pattern, RegexOptions option, IDictionary<string, LexRegex>? regexDef = null)
	{
		ArgumentNullException.ThrowIfNull(pattern);
		return RegexParser.ParseRegex(pattern, option, regexDef);
	}

	#region 静态方法

	/// <summary>
	/// 返回表示单个字符的正则表达式。
	/// </summary>
	/// <param name="ch">要表示的字符。</param>
	/// <returns>表示单个字符的正则表达式。</returns>
	/// <overloads>
	/// <summary>
	/// 返回表示字符类的正则表达式。
	/// </summary>
	/// </overloads>
	public static LexRegex Symbol(char ch)
	{
		return new LiteralExp(ch.ToString());
	}

	/// <summary>
	/// 返回表示字符类的正则表达式。
	/// </summary>
	/// <param name="pattern">正则表达式表示的字符类的模式。</param>
	/// <returns>表示字符类的正则表达式。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="pattern"/> 为 <c>null</c>。</exception>
	public static LexRegex Symbol(string pattern)
	{
		ArgumentNullException.ThrowIfNull(pattern);
		return new CharClassExp(RegexParser.ParseCharClass(pattern, RegexOptions.None, false));
	}

	/// <summary>
	/// 返回表示字符类的正则表达式。
	/// </summary>
	/// <param name="pattern">正则表达式表示的字符类的模式。</param>
	/// <param name="options">解析模式使用的正则表达式选项。</param>
	/// <returns>表示字符类的正则表达式。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="pattern"/> 为 <c>null</c>。</exception>
	public static LexRegex Symbol(string pattern, RegexOptions options)
	{
		ArgumentNullException.ThrowIfNull(pattern);
		return new CharClassExp(RegexParser.ParseCharClass(pattern, options, false));
	}

	/// <summary>
	/// 返回表示单个不区分大小写的字符的正则表达式。
	/// </summary>
	/// <param name="ch">要表示的字符。</param>
	/// <returns>表示单个字符的正则表达式。</returns>
	/// <overloads>
	/// <summary>
	/// 返回表示单个不区分大小写的字符的正则表达式。
	/// </summary>
	/// </overloads>
	public static LexRegex SymbolIgnoreCase(char ch)
	{
		return SymbolIgnoreCase(ch, false);
	}

	/// <summary>
	/// 返回表示单个不区分大小写的字符的正则表达式。
	/// </summary>
	/// <param name="ch">要表示的字符。</param>
	/// <param name="invariantCulture">是否忽略区域性差异。</param>
	/// <returns>表示单个字符的正则表达式。</returns>
	public static LexRegex SymbolIgnoreCase(char ch, bool invariantCulture)
	{
		CultureInfo culture = invariantCulture ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;
		return SymbolIgnoreCase(ch, culture);
	}

	/// <summary>
	/// 返回表示单个不区分大小写的字符的正则表达式。
	/// </summary>
	/// <param name="ch">要表示的字符。</param>
	/// <param name="culture">转换大小写使用的区域性信息。</param>
	/// <returns>表示单个字符的正则表达式。</returns>
	public static LexRegex SymbolIgnoreCase(char ch, CultureInfo? culture)
	{
		if (culture == null)
		{
			culture = CultureInfo.CurrentCulture;
		}
		return new LiteralExp(ch.ToString(), culture);
	}

	/// <summary>
	/// 返回表示字符串的正则表达式。
	/// </summary>
	/// <param name="text">要表示的字符串。</param>
	/// <returns>表示字符串的正则表达式。</returns>
	public static LexRegex Literal(string text)
	{
		ArgumentNullException.ThrowIfNull(text);
		return new LiteralExp(text);
	}

	/// <summary>
	/// 返回表示不区分大小写的字符串的正则表达式。
	/// </summary>
	/// <param name="text">要表示的字符串。</param>
	/// <returns>表示字符串的正则表达式。</returns>
	/// <overloads>
	/// <summary>
	/// 返回表示不区分大小写的字符串的正则表达式。
	/// </summary>
	/// </overloads>
	public static LexRegex LiteralIgnoreCase(string text)
	{
		return LiteralIgnoreCase(text, CultureInfo.CurrentCulture);
	}

	/// <summary>
	/// 返回表示不区分大小写的字符串的正则表达式。
	/// </summary>
	/// <param name="text">要表示的字符串。</param>
	/// <param name="invariantCulture">是否忽略区域性差异。</param>
	/// <returns>表示字符串的正则表达式。</returns>
	public static LexRegex LiteralIgnoreCase(string text, bool invariantCulture)
	{
		CultureInfo culture = invariantCulture ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;
		return LiteralIgnoreCase(text, culture);
	}

	/// <summary>
	/// 返回表示不区分大小写的字符串的正则表达式。
	/// </summary>
	/// <param name="text">要表示的字符串。</param>
	/// <param name="culture">转换大小写使用的区域性信息。</param>
	/// <returns>表示字符串的正则表达式。</returns>
	public static LexRegex LiteralIgnoreCase(string text, CultureInfo? culture)
	{
		ArgumentNullException.ThrowIfNull(text);
		if (culture == null)
		{
			culture = CultureInfo.CurrentCulture;
		}
		return new LiteralExp(text, culture);
	}

	/// <summary>
	/// 返回表示处换行以外任意字符的正则表达式。
	/// </summary>
	/// <returns>表示除换行以外任意字符的正则表达式。</returns>
	/// <overloads>
	/// <summary>
	/// 返回表示处换行以外任意字符的正则表达式。
	/// </summary>
	/// </overloads>
	public static LexRegex AnyChar()
	{
		return CharClassExp.AnyCharExp;
	}

	/// <summary>
	/// 返回表示任意字符的正则表达式。
	/// </summary>
	/// <param name="singleLine">是否使用单行模式（匹配换行符）。</param>
	/// <returns>表示任意字符的正则表达式。</returns>
	public static LexRegex AnyChar(bool singleLine)
	{
		if (singleLine)
		{
			return CharClassExp.SingleLineAnyCharExp;
		}
		else
		{
			return CharClassExp.AnyCharExp;
		}
	}

	/// <summary>
	/// 返回指定正则表达式的并联。
	/// </summary>
	/// <param name="expressions">要并联的正则表达式。</param>
	/// <returns>表示正则表达式的并联。</returns>
	public static LexRegex Alternate(params LexRegex[] expressions)
	{
		if (expressions.Length == 1)
		{
			return expressions[0];
		}
		return new AlternationExp(expressions);
	}

	/// <summary>
	/// 返回指定正则表达式的连接。
	/// </summary>
	/// <param name="expressions">要连接的正则表达式。</param>
	/// <returns>表示正则表达式的连接。</returns>
	public static LexRegex Concat(params LexRegex[] expressions)
	{
		if (expressions.Length == 1)
		{
			return expressions[0];
		}
		return new ConcatenationExp(expressions);
	}

	/// <summary>
	/// 返回表示文件结束的正则表达式。
	/// </summary>
	/// <returns>表示文件结束的正则表达式。</returns>
	public static LexRegex EndOfFile()
	{
		return EndOfFileExp.Default;
	}

	#endregion // 静态方法

	/// <summary>
	/// 初始化 <see cref="LexRegex"/> 类的新实例。
	/// </summary>
	protected LexRegex() { }

	/// <summary>
	/// 检查正则表达式是否可以被嵌套。
	/// </summary>
	/// <param name="regex">要检查的正则表达式。</param>
	/// <param name="paramName">参数名称。</param>
	protected static void CheckRegex(LexRegex regex, [CallerArgumentExpression("regex")] string? paramName = null)
	{
		ArgumentNullException.ThrowIfNull(regex, paramName);
		if (regex is AnchorExp anchor)
		{
			if (anchor.BeginningOfLine)
			{
				throw CompilerExceptions.NestedBeginningOfLine(paramName);
			}
			if (anchor.TrailingExpression == AnchorExp.EndOfLine)
			{
				throw CompilerExceptions.NestedEndOfLine(paramName);
			}
			throw CompilerExceptions.NestedTrailing(paramName);
		}
		if (regex is EndOfFileExp)
		{
			throw CompilerExceptions.NestedEndOfFile(paramName);
		}
	}

	#region 快捷方法

	/// <summary>
	/// 返回表示当前正则表达式从行的起始位置开始匹配的正则表达式。
	/// </summary>
	/// <returns>表示当前正则表达式从行的起始位置开始匹配的正则表达式。</returns>
	public LexRegex BeginningOfLine()
	{
		if (this is not AnchorExp anchorExp)
		{
			anchorExp = new AnchorExp(this);
		}
		anchorExp.BeginningOfLine = true;
		return anchorExp;
	}

	/// <summary>
	/// 返回表示当前正则表达式要到达行的结束位置的正则表达式。
	/// </summary>
	/// <returns>表示当前正则表达式要到达行的结束位置的正则表达式。</returns>
	public LexRegex EndOfLine()
	{
		return Trailing(AnchorExp.EndOfLine);
	}

	/// <summary>
	/// 返回表示当前正则表达式要向前看指定内容的正则表达式。
	/// </summary>
	/// <param name="regex">要向前看的正则表达式。</param>
	/// <returns>表示当前正则表达式要向前看指定内容的正则表达式。</returns>
	public LexRegex Trailing(LexRegex regex)
	{
		if (this is not AnchorExp anchorExp)
		{
			anchorExp = new AnchorExp(this);
		}
		anchorExp.TrailingExpression = regex;
		return anchorExp;
	}

	/// <summary>
	/// 返回表示当前正则表达式的 Kleene 闭包的正则表达式。
	/// </summary>
	/// <returns>表示当前正则表达式的 Kleene 闭包的正则表达式。</returns>
	public LexRegex Star()
	{
		return new QuantifierExp(this, 0, int.MaxValue);
	}

	/// <summary>
	/// 返回表示当前正则表达式的正闭包的正则表达式。
	/// </summary>
	/// <returns>表示当前正则表达式的正闭包的正则表达式。</returns>
	public LexRegex Positive()
	{
		return new QuantifierExp(this, 1, int.MaxValue);
	}

	/// <summary>
	/// 返回表示当前正则表达式可选的正则表达式。
	/// </summary>
	/// <returns>表示当前正则表达式可选的则表达式。</returns>
	public LexRegex Optional()
	{
		return new QuantifierExp(this, 0, 1);
	}

	/// <summary>
	/// 返回表示当前正则表达式重复多次的正则表达式。
	/// </summary>
	/// <param name="times">重复次数。</param>
	/// <returns>表示当前正则表达式重复多次的正则表达式。</returns>
	/// <overloads>
	/// <summary>
	/// 返回表示重复多次的正则表达式。
	/// </summary>
	/// </overloads>
	public LexRegex Repeat(int times)
	{
		return new QuantifierExp(this, times, times);
	}

	/// <summary>
	/// 返回表示当前正则表达式重复多次的正则表达式。
	/// </summary>
	/// <param name="minTimes">最少的重复次数。</param>
	/// <param name="maxTimes">最多的重复次数。</param>
	/// <returns>表示当前正则表达式重复多次的正则表达式。</returns>
	public LexRegex Repeat(int minTimes, int maxTimes)
	{
		return new QuantifierExp(this, minTimes, maxTimes);
	}

	/// <summary>
	/// 返回表示当前正则表达式至少重复 <paramref name="minTimes"/> 次的正则表达式。
	/// </summary>
	/// <param name="minTimes">最少的重复次数。</param>
	/// <returns>表示当前正则表达式至少重复 <paramref name="minTimes"/> 次的正则表达式。</returns>
	public LexRegex RepeatMinTimes(int minTimes)
	{
		return new QuantifierExp(this, minTimes, int.MaxValue);
	}

	/// <summary>
	/// 返回表示当前正则表达式至多重复 <paramref name="maxTimes"/> 次的正则表达式。
	/// </summary>
	/// <param name="maxTimes">最多的重复次数。</param>
	/// <returns>表示当前正则表达式至多重复 <paramref name="maxTimes"/> 次的正则表达式。</returns>
	public LexRegex RepeatMaxTimes(int maxTimes)
	{
		return new QuantifierExp(this, 0, maxTimes);
	}

	#endregion // 快捷方法

	/// <summary>
	/// 获取当前正则表达式匹配的字符串长度。
	/// </summary>
	/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>null</c>。</value>
	public abstract int? Length { get; }

	#region IEquatable<Regex> 成员

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public abstract bool Equals(LexRegex? other);

	/// <summary>
	/// 返回当前对象是否等于另一对象。
	/// </summary>
	/// <param name="obj">要与当前对象进行比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="obj"/>，则为 true；否则为 false。</returns>
	public override bool Equals(object? obj)
	{
		if (obj is LexRegex other)
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
		throw CommonExceptions.MethodNotSupported();
	}

	/// <summary>
	/// 返回指定的 <see cref="LexRegex"/> 是否相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator ==(LexRegex? left, LexRegex? right)
	{
		if (ReferenceEquals(left, right))
		{
			return true;
		}
		if (left is null)
		{
			return false;
		}
		return left.Equals(right);
	}

	/// <summary>
	/// 返回指定的 <see cref="LexRegex"/> 是否不相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator !=(LexRegex? left, LexRegex? right)
	{
		if (ReferenceEquals(left, right))
		{
			return false;
		}
		if (left is null)
		{
			return true;
		}
		return !left.Equals(right);
	}

	#endregion // IEquatable<Regex> 成员

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		StringBuilder builder = new();
		ToString(builder);
		return builder.ToString();
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <param name="builder">字符串构造器。</param>
	internal abstract void ToString(StringBuilder builder);

}
