using System;

namespace Cyjb.Compiler
{
	/// <summary>
	/// 提供用于异常处理的辅助方法。
	/// </summary>
	internal static class CompilerExceptionHelper
	{

		#region ArgumentException

		/// <summary>
		/// 返回参数异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="resName">异常信息的资源名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		private static ArgumentException GetArgumentException(string paramName, string resName)
		{
			return new ArgumentException(ExceptionResources.GetString(resName), paramName);
		}
		/// <summary>
		/// 返回参数异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="resName">异常信息的资源名称。</param>
		/// <param name="args">格式化信息的参数。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		private static ArgumentException GetArgumentException(string paramName, string resName, params object[] args)
		{
			string message = ExceptionResources.GetString(resName, args);
			return new ArgumentException(message, paramName);
		}

		#endregion // ArgumentException

		#region 正则表达式异常

		/// <summary>
		/// 返回正则表达式字符类不可以合并的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException RegexCharClassCannotMerge(string paramName)
		{
			throw GetArgumentException(paramName, "RegexCharClassCannotMerge");
		}
		/// <summary>
		/// 返回嵌套的行起始的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException NestedBeginningOfLine(string paramName)
		{
			throw GetArgumentException(paramName, "NestedBeginningOfLine");
		}
		/// <summary>
		/// 返回嵌套的行结束的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException NestedEndOfLine(string paramName)
		{
			throw GetArgumentException(paramName, "NestedEndOfLine");
		}
		/// <summary>
		/// 返回嵌套的向前看的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException NestedTrailing(string paramName)
		{
			throw GetArgumentException(paramName, "NestedTrailing");
		}
		/// <summary>
		/// 返回不能在字符范围中包括类的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <param name="charClass">出现异常的的字符类。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException BadClassInCharRange(string paramName, string pattern, string charClass)
		{
			string message = ExceptionResources.GetString("BadClassInCharRange", charClass);
			return ParsingException(paramName, pattern, message);
		}
		/// <summary>
		/// 返回排除的字符范围错误的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException SubtractionMustBeLast(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("SubtractionMustBeLast"));
		}
		/// <summary>
		/// 返回字符范围顺序相反的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException ReversedCharRange(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("ReversedCharRange"));
		}
		/// <summary>
		/// 返回未终止的内联注释的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException UnterminatedComment(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("UnterminatedComment"));
		}
		/// <summary>
		/// 返回未识别的分组的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException UnrecognizedGrouping(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("UnrecognizedGrouping"));
		}
		/// <summary>
		/// 返回太多的右括号的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException TooManyParens(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("TooManyParens"));
		}
		/// <summary>
		/// 返回右括号不足的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException NotEnoughParens(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("NotEnoughParens"));
		}
		/// <summary>
		/// 返回非法的结尾转义的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException IllegalEndEscape(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("IllegalEndEscape"));
		}
		/// <summary>
		/// 返回嵌套的数量词的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <param name="quantify">被嵌套的数量词。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException NestedQuantify(string paramName, string pattern, string quantify)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("NestedQuantify", quantify));
		}
		/// <summary>
		/// 返回数量词之前没有字符的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException QuantifyAfterNothing(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("QuantifyAfterNothing"));
		}
		/// <summary>
		/// 返回非法的范围的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException IllegalRange(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("IllegalRange"));
		}
		/// <summary>
		/// 返回未终止的 [] 集合的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException UnterminatedBracket(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("UnterminatedBracket"));
		}
		/// <summary>
		/// 返回不完整的 \p{X} 字符转义的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException IncompleteSlashP(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("IncompleteSlashP"));
		}
		/// <summary>
		/// 返回十六进制数字位数不足的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException TooFewHex(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("TooFewHex"));
		}
		/// <summary>
		/// 返回不完整的的 \p{X} 字符转义的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException MalformedSlashP(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("MalformedSlashP"));
		}
		/// <summary>
		/// 返回无法识别的转义序列的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <param name="ch">无法被识别的转义序列。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException UnrecognizedEscape(string paramName, string pattern, string ch)
		{
			string message = ExceptionResources.GetString("UnrecognizedEscape", ch);
			throw ParsingException(paramName, pattern, message);
		}
		/// <summary>
		/// 返回缺少控制字符的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException MissingControl(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("MissingControl"));
		}
		/// <summary>
		/// 返回未识别的控制字符的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException UnrecognizedControl(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("UnrecognizedControl"));
		}
		/// <summary>
		/// 返回未定义的正则表达式的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <param name="name">未定义的正则表达式的名字。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException UndefinedRegex(string paramName, string pattern, string name)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("UndefinedRegex", name));
		}
		/// <summary>
		/// 返回不完整正则表达式引用的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException IncompleteRegexReference(string paramName, string pattern)
		{
			throw ParsingException(paramName, pattern, ExceptionResources.GetString("IncompleteRegexReference"));
		}
		/// <summary>
		/// 返回分析异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <param name="message">分析的异常信息。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException ParsingException(string paramName, string pattern, string message)
		{
			return GetArgumentException(paramName, "ParsingException", pattern, message);
		}
		/// <summary>
		/// 返回内部的 ScanRegex 的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException InternalScanRegexError(string paramName)
		{
			throw GetArgumentException(paramName, "InternalScanRegexError");
		}

		#endregion // 正则表达式异常

	}
}
