using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Cyjb.Compiler.Lexer;
using Cyjb.IO;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示正则表达式。
	/// </summary>
	public abstract class Regex
	{
		/// <summary>
		/// 表示所有字符的字符类。
		/// </summary>
		private const string AnyCharClass = "\x01\x04\x00\x0A\x0B\x0D\x0E";
		/// <summary>
		/// 表示单行模式下的所有字符的字符类。
		/// </summary>
		private const string AnyCharSingleLineClass = "\x00\x01\x00\x00";
		/// <summary>
		/// 检查正则表达式是否可以被嵌套。
		/// </summary>
		/// <param name="regex">要检查的正则表达式。</param>
		protected static void CheckRegex(Regex regex)
		{
			AnchorExp anchor = regex as AnchorExp;
			if (anchor != null)
			{
				if (anchor.BeginningOfLine)
				{
					throw CompilerExceptionHelper.NestedBeginningOfLine("regex");
				}
				if (anchor.TrailingExpression == AnchorExp.EndOfLine)
				{
					throw CompilerExceptionHelper.NestedEndOfLine("regex");
				}
				throw CompilerExceptionHelper.NestedTrailing("regex");
			}
			if (regex is EndOfFileExp)
			{
				throw CompilerExceptionHelper.NestedEndOfFile("regex");
			}
		}

		#region 解析正则表达式

		/// <summary>
		/// 根据给定的字符串解析正则表达式。
		/// </summary>
		/// <param name="pattern">正则表达式的模式字符串。</param>
		/// <returns>解析得到的正则表达式。</returns>
		public static Regex Parse(string pattern)
		{
			return RegexParser.ParseRegex(pattern, RegexOptions.None, null);
		}
		/// <summary>
		/// 根据给定的字符串解析正则表达式。
		/// </summary>
		/// <param name="pattern">正则表达式的模式字符串。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <returns>解析得到的正则表达式。</returns>
		public static Regex Parse(string pattern, RegexOptions option)
		{
			return RegexParser.ParseRegex(pattern, option, null);
		}
		/// <summary>
		/// 根据给定的字符串解析正则表达式。
		/// </summary>
		/// <param name="pattern">正则表达式的模式字符串。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		/// <returns>解析得到的正则表达式。</returns>
		public static Regex Parse(string pattern, IDictionary<string, Regex> regexDef)
		{
			return RegexParser.ParseRegex(pattern, RegexOptions.None, regexDef);
		}
		/// <summary>
		/// 根据给定的字符串解析正则表达式。
		/// </summary>
		/// <param name="pattern">正则表达式的模式字符串。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		/// <returns>解析得到的正则表达式。</returns>
		public static Regex Parse(string pattern, RegexOptions option, IDictionary<string, Regex> regexDef)
		{
			return RegexParser.ParseRegex(pattern, option, regexDef);
		}
		/// <summary>
		/// 根据给定的源文件读取器解析正则表达式。
		/// </summary>
		/// <param name="reader">正则表达式的源文件读取器。</param>
		/// <returns>解析得到的正则表达式。</returns>
		public static Regex Parse(SourceReader reader)
		{
			return RegexParser.ParseRegex(reader, RegexOptions.None, null);
		}
		/// <summary>
		/// 根据给定的源文件读取器解析正则表达式。
		/// </summary>
		/// <param name="reader">正则表达式的源文件读取器。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <returns>解析得到的正则表达式。</returns>
		public static Regex Parse(SourceReader reader, RegexOptions option)
		{
			return RegexParser.ParseRegex(reader, option, null);
		}
		/// <summary>
		/// 根据给定的源文件读取器解析正则表达式。
		/// </summary>
		/// <param name="reader">正则表达式的源文件读取器。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		/// <returns>解析得到的正则表达式。</returns>
		public static Regex Parse(SourceReader reader, IDictionary<string, Regex> regexDef)
		{
			return RegexParser.ParseRegex(reader, RegexOptions.None, regexDef);
		}
		/// <summary>
		/// 根据给定的源文件读取器解析正则表达式。
		/// </summary>
		/// <param name="reader">正则表达式的源文件读取器。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		/// <returns>解析得到的正则表达式。</returns>
		public static Regex Parse(SourceReader reader, RegexOptions option, IDictionary<string, Regex> regexDef)
		{
			return RegexParser.ParseRegex(reader, option, regexDef);
		}

		#endregion // 解析正则表达式

		#region 静态方法

		/// <summary>
		/// 返回表示单个字符的正则表达式。
		/// </summary>
		/// <param name="ch">要表示的字符。</param>
		/// <returns>表示单个字符的正则表达式。</returns>
		public static Regex Symbol(char ch)
		{
			RegexCharClass cc = new RegexCharClass();
			cc.AddChar(ch);
			return new CharClassExp(cc.ToStringClass());
		}
		/// <summary>
		/// 返回表示单个不区分大小写的字符的正则表达式。
		/// </summary>
		/// <param name="ch">要表示的字符。</param>
		/// <returns>表示单个字符的正则表达式。</returns>
		public static Regex SymbolIgnoreCase(char ch)
		{
			return SymbolIgnoreCase(ch, false);
		}
		/// <summary>
		/// 返回表示单个不区分大小写的字符的正则表达式。
		/// </summary>
		/// <param name="ch">要表示的字符。</param>
		/// <param name="invariantCulture">是否忽略区域性差异。</param>
		/// <returns>表示单个字符的正则表达式。</returns>
		public static Regex SymbolIgnoreCase(char ch, bool invariantCulture)
		{
			CultureInfo culture = invariantCulture ?
				CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;
			return SymbolIgnoreCase(ch, culture);
		}
		/// <summary>
		/// 返回表示单个不区分大小写的字符的正则表达式。
		/// </summary>
		/// <param name="ch">要表示的字符。</param>
		/// <param name="culture">转换大小写使用的区域性信息。</param>
		/// <returns>表示单个字符的正则表达式。</returns>
		public static Regex SymbolIgnoreCase(char ch, CultureInfo culture)
		{
			char upperCase = char.ToUpper(ch, culture);
			char lowerCase = char.ToLower(ch, culture);
			RegexCharClass cc = new RegexCharClass();
			if (upperCase == lowerCase)
			{
				// 大小写相同。
				cc.AddChar(upperCase);
			}
			else
			{
				// 大小写不用。
				cc.AddChar(upperCase);
				cc.AddChar(lowerCase);
			}
			return new CharClassExp(cc.ToStringClass());
		}
		/// <summary>
		/// 返回表示字符串的正则表达式。
		/// </summary>
		/// <param name="text">要表示的字符串。</param>
		/// <returns>表示字符串的正则表达式。</returns>
		[SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo",
			MessageId = "Cyjb.Compiler.RegularExpressions.LiteralExp.#ctor(System.String)")]
		public static Regex Literal(string text)
		{
			ExceptionHelper.CheckArgumentNull(text, "text");
			if (text.Length == 1)
			{
				return Symbol(text[0]);
			}
			return new LiteralExp(text);
		}
		/// <summary>
		/// 返回表示不区分大小写的字符串的正则表达式。
		/// </summary>
		/// <param name="text">要表示的字符串。</param>
		/// <returns>表示字符串的正则表达式。</returns>
		public static Regex LiteralIgnoreCase(string text)
		{
			return LiteralIgnoreCase(text, CultureInfo.CurrentCulture);
		}
		/// <summary>
		/// 返回表示不区分大小写的字符串的正则表达式。
		/// </summary>
		/// <param name="text">要表示的字符串。</param>
		/// <param name="invariantCulture">是否忽略区域性差异。</param>
		/// <returns>表示字符串的正则表达式。</returns>
		public static Regex LiteralIgnoreCase(string text, bool invariantCulture)
		{
			return LiteralIgnoreCase(text,
				invariantCulture ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture);
		}
		/// <summary>
		/// 返回表示不区分大小写的字符串的正则表达式。
		/// </summary>
		/// <param name="text">要表示的字符串。</param>
		/// <param name="culture">转换大小写使用的区域性信息。</param>
		/// <returns>表示字符串的正则表达式。</returns>
		public static Regex LiteralIgnoreCase(string text, CultureInfo culture)
		{
			ExceptionHelper.CheckArgumentNull(text, "text");
			if (text.Length == 1)
			{
				return SymbolIgnoreCase(text[0], culture);
			}
			return new LiteralExp(text, culture);
		}
		/// <summary>
		/// 返回表示处换行以外任意字符的正则表达式。
		/// </summary>
		/// <returns>表示除换行以外任意字符的正则表达式。</returns>
		public static Regex AnyChar()
		{
			return new CharClassExp(AnyCharClass);
		}
		/// <summary>
		/// 返回表示任意字符的正则表达式。
		/// </summary>
		/// <returns>表示任意字符的正则表达式。</returns>
		public static Regex AnyChar(bool singleLine)
		{
			if (singleLine)
			{
				return new CharClassExp(AnyCharSingleLineClass);
			}
			else
			{
				return new CharClassExp(AnyCharClass);
			}
		}
		/// <summary>
		/// 返回表示文件结束的正则表达式。
		/// </summary>
		/// <returns>表示文件结束的正则表达式。</returns>
		public static Regex EndOfFile()
		{
			return EndOfFileExp.Default;
		}
		/// <summary>
		/// 返回表示字符类的正则表达式。
		/// </summary>
		/// <param name="pattern">正则表达式表示的字符类的模式。</param>
		/// <returns>表示字符类的正则表达式。</returns>
		public static Regex CharClassPattern(string pattern)
		{
			ExceptionHelper.CheckArgumentNull(pattern, "pattern");
			return new CharClassExp(RegexCharClass.ParsePattern(pattern).ToStringClass());
		}
		/// <summary>
		/// 返回表示字符类的正则表达式。
		/// </summary>
		/// <param name="pattern">正则表达式表示的字符类的模式。</param>
		/// <param name="options">解析模式使用的正则表达式选项。</param>
		/// <returns>表示字符类的正则表达式。</returns>
		public static Regex CharClassPattern(string pattern, RegexOptions options)
		{
			ExceptionHelper.CheckArgumentNull(pattern, "pattern");
			return new CharClassExp(RegexCharClass.ParsePattern(pattern, options).ToStringClass());
		}
		/// <summary>
		/// 返回表示字符类的正则表达式。
		/// </summary>
		/// <param name="cc">正则表达式表示的字符类。</param>
		/// <returns>表示字符类的正则表达式。</returns>
		public static Regex CharClass(string cc)
		{
			ExceptionHelper.CheckArgumentNull(cc, "cc");
			return new CharClassExp(cc);
		}
		/// <summary>
		/// 返回表示字符类的正则表达式。
		/// </summary>
		/// <param name="cc">正则表达式表示的字符类。</param>
		/// <returns>表示字符类的正则表达式。</returns>
		public static Regex CharClass(RegexCharClass cc)
		{
			ExceptionHelper.CheckArgumentNull(cc, "cc");
			return new CharClassExp(cc.ToStringClass());
		}
		/// <summary>
		/// 返回表示行的起始位置的正则表达式。
		/// </summary>
		/// <param name="innerExp">内部的正则表达式。</param>
		/// <returns>表示行的起始位置的正则表达式。</returns>
		public static Regex BeginningOfLine(Regex innerExp)
		{
			AnchorExp anchorExp = innerExp as AnchorExp;
			if (anchorExp == null)
			{
				anchorExp = new AnchorExp(innerExp);
			}
			anchorExp.BeginningOfLine = true;
			return anchorExp;
		}
		/// <summary>
		/// 返回表示行的结束位置的正则表达式。
		/// </summary>
		/// <param name="innerExp">内部的正则表达式。</param>
		/// <returns>表示行的结束位置的正则表达式。</returns>
		public static Regex EndOfLine(Regex innerExp)
		{
			return Trailing(innerExp, AnchorExp.EndOfLine);
		}
		/// <summary>
		/// 返回表示向前看的正则表达式。
		/// </summary>
		/// <param name="innerExp">内部的正则表达式。</param>
		/// <param name="regex">要向前看的正则表达式。</param>
		/// <returns>表示向前看的正则表达式。</returns>
		public static Regex Trailing(Regex innerExp, Regex regex)
		{
			AnchorExp anchorExp = innerExp as AnchorExp;
			if (anchorExp == null)
			{
				anchorExp = new AnchorExp(innerExp);
			}
			anchorExp.TrailingExpression = regex;
			return anchorExp;
		}
		/// <summary>
		/// 返回表示 Kleene 闭包的正则表达式。
		/// </summary>
		/// <param name="innerExp">Kleene 闭包的内部正则表达式。</param>
		/// <returns>表示 Kleene 闭包的正则表达式。</returns>
		public static Regex Star(Regex innerExp)
		{
			return new RepeatExp(innerExp, 0, int.MaxValue);
		}
		/// <summary>
		/// 返回表示正闭包的正则表达式。
		/// </summary>
		/// <param name="innerExp">正闭包的内部正则表达式。</param>
		/// <returns>表示正闭包的正则表达式。</returns>
		public static Regex Positive(Regex innerExp)
		{
			return new RepeatExp(innerExp, 1, int.MaxValue);
		}
		/// <summary>
		/// 返回表示可选的正则表达式。
		/// </summary>
		/// <param name="innerExp">可选的内部正则表达式。</param>
		/// <returns>表示可选的正则表达式。</returns>
		public static Regex Optional(Regex innerExp)
		{
			return new RepeatExp(innerExp, 0, 1);
		}
		/// <summary>
		/// 返回表示重复多次的正则表达式。
		/// </summary>
		/// <param name="innerExp">重复的的内部正则表达式。</param>
		/// <param name="times">重复次数。</param>
		/// <returns>表示重复多次的正则表达式。</returns>
		public static Regex Repeat(Regex innerExp, int times)
		{
			return new RepeatExp(innerExp, times, times);
		}
		/// <summary>
		/// 返回表示重复多次的正则表达式。
		/// </summary>
		/// <param name="innerExp">重复的的内部正则表达式。</param>
		/// <param name="minTimes">最少的重复次数。</param>
		/// <param name="maxTimes">最多的重复次数。</param>
		/// <returns>表示重复多次的正则表达式。</returns>
		public static Regex Repeat(Regex innerExp, int minTimes, int maxTimes)
		{
			return new RepeatExp(innerExp, minTimes, maxTimes);
		}
		/// <summary>
		/// 返回表示至少重复 <paramref name="minTimes"/> 次的正则表达式。
		/// </summary>
		/// <param name="innerExp">重复的的内部正则表达式。</param>
		/// <param name="minTimes">最少的重复次数。</param>
		/// <returns>表示至少重复 <paramref name="minTimes"/> 次的正则表达式。</returns>
		public static Regex RepeatMinTimes(Regex innerExp, int minTimes)
		{
			return new RepeatExp(innerExp, minTimes, int.MaxValue);
		}
		/// <summary>
		/// 返回表示至多重复 <paramref name="maxTimes"/> 次的正则表达式。
		/// </summary>
		/// <param name="innerExp">重复的的内部正则表达式。</param>
		/// <param name="maxTimes">最多的重复次数。</param>
		/// <returns>表示至多重复 <paramref name="maxTimes"/> 次的正则表达式。</returns>
		public static Regex RepeatMaxTimes(Regex innerExp, int maxTimes)
		{
			return new RepeatExp(innerExp, 0, maxTimes);
		}
		/// <summary>
		/// 返回表示两个正则表达式连接的正则表达式。
		/// </summary>
		/// <param name="left">要连接的第一个正则表达式。</param>
		/// <param name="right">要连接的第二个正则表达式。</param>
		/// <returns>表示两个正则表达式连接的正则表达式。</returns>
		public static Regex Concat(Regex left, Regex right)
		{
			return new ConcatenationExp(left, right);
		}
		/// <summary>
		/// 返回表示两个正则表达式并联的正则表达式。
		/// </summary>
		/// <param name="left">要并联的第一个正则表达式。</param>
		/// <param name="right">要并联的第二个正则表达式。</param>
		/// <returns>表示两个正则表达式并联的正则表达式。</returns>
		public static Regex Union(Regex left, Regex right)
		{
			return new AlternationExp(left, right);
		}
		/// <summary>
		/// 正则表达式的或运算符重载，表示正则表达式的并联。
		/// </summary>
		/// <param name="left">要相或的第一个正则表达式。</param>
		/// <param name="right">要相或的第二个正则表达式。</param>
		/// <returns>两个正则表达式并联的结果。</returns>
		public static Regex operator |(Regex left, Regex right)
		{
			return new AlternationExp(left, right);
		}

		#endregion // 静态方法

		#region 快捷方法

		/// <summary>
		/// 返回表示当前正则表达式从行的起始位置开始匹配的正则表达式。
		/// </summary>
		/// <returns>表示当前正则表达式从行的起始位置开始匹配的正则表达式。</returns>
		public Regex BeginningOfLine()
		{
			return BeginningOfLine(this);
		}
		/// <summary>
		/// 返回表示当前正则表达式要到达行的结束位置的正则表达式。
		/// </summary>
		/// <returns>表示当前正则表达式要到达行的结束位置的正则表达式。</returns>
		public Regex EndOfLine()
		{
			return EndOfLine(this);
		}
		/// <summary>
		/// 返回表示当前正则表达式要向前看指定内容的正则表达式。
		/// </summary>
		/// <param name="regex">要向前看的正则表达式。</param>
		/// <returns>表示当前正则表达式要向前看指定内容的正则表达式。</returns>
		public Regex Trailing(Regex regex)
		{
			return Trailing(this, regex);
		}
		/// <summary>
		/// 返回表示当前正则表达式的 Kleene 闭包的正则表达式。
		/// </summary>
		/// <returns>表示当前正则表达式的 Kleene 闭包的正则表达式。</returns>
		public Regex Star()
		{
			return Star(this);
		}
		/// <summary>
		/// 返回表示当前正则表达式的正闭包的正则表达式。
		/// </summary>
		/// <returns>表示当前正则表达式的正闭包的正则表达式。</returns>
		public Regex Positive()
		{
			return Positive(this);
		}
		/// <summary>
		/// 返回表示当前正则表达式可选的正则表达式。
		/// </summary>
		/// <returns>表示当前正则表达式可选的则表达式。</returns>
		public Regex Optional()
		{
			return Optional(this);
		}
		/// <summary>
		/// 返回表示当前正则表达式重复多次的正则表达式。
		/// </summary>
		/// <param name="times">重复次数。</param>
		/// <returns>表示当前正则表达式重复多次的正则表达式。</returns>
		public Regex Repeat(int times)
		{
			return Repeat(this, times);
		}
		/// <summary>
		/// 返回表示当前正则表达式重复多次的正则表达式。
		/// </summary>
		/// <param name="minTimes">最少的重复次数。</param>
		/// <param name="maxTimes">最多的重复次数。</param>
		/// <returns>表示当前正则表达式重复多次的正则表达式。</returns>
		public Regex Repeat(int minTimes, int maxTimes)
		{
			return Repeat(this, minTimes, maxTimes);
		}
		/// <summary>
		/// 返回表示当前正则表达式至少重复 <paramref name="minTimes"/> 次的正则表达式。
		/// </summary>
		/// <param name="minTimes">最少的重复次数。</param>
		/// <returns>表示当前正则表达式至少重复 <paramref name="minTimes"/> 次的正则表达式。</returns>
		public Regex RepeatMinTimes(int minTimes)
		{
			return Repeat(this, minTimes, int.MaxValue);
		}
		/// <summary>
		/// 返回表示当前正则表达式至多重复 <paramref name="maxTimes"/> 次的正则表达式。
		/// </summary>
		/// <param name="maxTimes">最多的重复次数。</param>
		/// <returns>表示当前正则表达式至多重复 <paramref name="maxTimes"/> 次的正则表达式。</returns>
		public Regex RepeatMaxTimes(int maxTimes)
		{
			return Repeat(this, 0, maxTimes);
		}
		/// <summary>
		/// 返回表示当前正则表达式与指定的正则表达式连接的正则表达式。
		/// </summary>
		/// <param name="right">要连接的正则表达式。</param>
		/// <returns>表示两个正则表达式连接的正则表达式。</returns>
		public Regex Concat(Regex right)
		{
			return Concat(this, right);
		}
		/// <summary>
		/// 返回表示当前正则表达式与指定的正则表达式并联的正则表达式。
		/// </summary>
		/// <param name="right">要并联的个正则表达式。</param>
		/// <returns>表示两个正则表达式并联的正则表达式。</returns>
		public Regex Union(Regex right)
		{
			return Union(this, right);
		}
		/// <summary>
		/// 返回表示当前正则表达式与指定的正则表达式并联的正则表达式。
		/// </summary>
		/// <param name="right">要并联的个正则表达式。</param>
		/// <returns>表示两个正则表达式并联的正则表达式。</returns>
		public Regex BitwiseOr(Regex right)
		{
			return Union(this, right);
		}

		#endregion // 快捷方法

		/// <summary>
		/// 根据当前的正则表达式构造 NFA。
		/// </summary>
		/// <param name="nfa">要构造的 NFA。</param>
		internal abstract void BuildNfa(Nfa nfa);
		/// <summary>
		/// 获取当前正则表达式匹配的字符长度。变长度则为 <c>-1</c>。
		/// </summary>
		public abstract int Length { get; }
	}
}
