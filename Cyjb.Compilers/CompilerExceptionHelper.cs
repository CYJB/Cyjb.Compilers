using System;
using Cyjb.IO;

namespace Cyjb.Compilers
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

		#region InvalidOperationException

		/// <summary>
		/// 返回方法调用异常。
		/// </summary>
		/// <param name="resName">异常信息的资源名称。</param>
		/// <returns><see cref="System.InvalidOperationException"/> 对象。</returns>
		private static InvalidOperationException GetInvalidOperationException(string resName)
		{
			return new InvalidOperationException(ExceptionResources.GetString(resName));
		}

		#endregion // InvalidOperationException

		#region 正则表达式异常

		/// <summary>
		/// 返回嵌套的行起始的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException NestedBeginningOfLine(string paramName)
		{
			return GetArgumentException(paramName, "NestedBeginningOfLine");
		}
		/// <summary>
		/// 返回嵌套的文件结束的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException NestedEndOfFile(string paramName)
		{
			return GetArgumentException(paramName, "NestedEndOfFile");
		}
		/// <summary>
		/// 返回嵌套的行结束的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException NestedEndOfLine(string paramName)
		{
			return GetArgumentException(paramName, "NestedEndOfLine");
		}
		/// <summary>
		/// 返回嵌套的向前看的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException NestedTrailing(string paramName)
		{
			return GetArgumentException(paramName, "NestedTrailing");
		}
		/// <summary>
		/// 返回分析异常。
		/// </summary>
		/// <param name="pattern">正在分析的模式字符串。</param>
		/// <param name="message">分析的异常信息。</param>
		/// <param name="start">分析异常的起始位置。</param>
		/// <param name="end">分析异常的结束位置。</param>
		/// <returns><see cref="Cyjb.IO.SourceException"/> 对象。</returns>
		internal static SourceException ParsingException(string pattern, string message,
			SourceLocation start, SourceLocation end)
		{
			return new SourceException(ExceptionResources.GetString("ParsingException", pattern, message), start, end);
		}
		/// <summary>
		/// 返回正则表达式字符类不可以合并的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException RegexCharClassCannotMerge(string paramName)
		{
			return GetArgumentException(paramName, "RegexCharClassCannotMerge");
		}

		#endregion // 正则表达式异常

		#region 词法分析异常

		/// <summary>
		/// 返回冲突的接受动作的异常。
		/// </summary>
		/// <returns><see cref="System.InvalidOperationException"/> 对象。</returns>
		public static InvalidOperationException ConflictingAcceptAction()
		{
			return GetInvalidOperationException("ConflictingAcceptAction");
		}
		/// <summary>
		/// 返回冲突的拒绝动作的异常。
		/// </summary>
		/// <returns><see cref="System.InvalidOperationException"/> 对象。</returns>
		public static InvalidOperationException ConflictingRejectAction()
		{
			return GetInvalidOperationException("ConflictingRejectAction");
		}
		/// <summary>
		/// 返回不完整正词法分析上下文的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		public static ArgumentException IncompleteLexerContext(string paramName)
		{
			return GetArgumentException(paramName, "IncompleteLexerContext");
		}
		/// <summary>
		/// 返回词法分析器的上下文无效的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="context">发生异常的上下文。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		public static ArgumentException InvalidLexerContext(string paramName, string context)
		{
			return GetArgumentException(paramName, "InvalidLexerContext", context);
		}
		/// <summary>
		/// 返回不允许拒绝动作的异常。
		/// </summary>
		/// <returns><see cref="System.InvalidOperationException"/> 对象。</returns>
		public static InvalidOperationException NotRejectable()
		{
			return GetInvalidOperationException("NotRejectable");
		}
		/// <summary>
		/// 返回未识别的词法单元的异常。
		/// </summary>
		/// <param name="text">未被识别的词法单元的文本。</param>
		/// <param name="start">词法单元的起始位置。</param>
		/// <param name="end">词法单元的结束位置。</param>
		/// <returns><see cref="Cyjb.IO.SourceException"/> 对象。</returns>
		public static SourceException UnrecognizedToken(string text, SourceLocation start, SourceLocation end)
		{
			return new SourceException(ExceptionResources.GetString("UnrecognizedToken", text), start, end);
		}

		#endregion // 词法分析异常

		/// <summary>
		/// 返回重复的符号标识符的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="id">重复的符号标识符。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		public static ArgumentException DuplicatedSymbolId(string paramName, string id)
		{
			return GetArgumentException(paramName, "DuplicatedSymbolId", id);
		}
		/// <summary>
		/// 返回无效的符号标识符的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <param name="id">无效的符号标识符。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		public static ArgumentException InvalidSymbolId(string paramName, string id)
		{
			return GetArgumentException(paramName, "InvalidSymbolId", id);
		}
	}
}
