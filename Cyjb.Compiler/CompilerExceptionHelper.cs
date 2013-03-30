using System;
using Cyjb.IO;

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
		///// <summary>
		///// 返回参数异常。
		///// </summary>
		///// <param name="paramName">产生异常的参数名称。</param>
		///// <param name="resName">异常信息的资源名称。</param>
		///// <param name="args">格式化信息的参数。</param>
		///// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		//private static ArgumentException GetArgumentException(string paramName, string resName, params object[] args)
		//{
		//	string message = ExceptionResources.GetString(resName, args);
		//	return new ArgumentException(message, paramName);
		//}

		#endregion // ArgumentException

		#region 正则表达式异常

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
		/// 返回嵌套的文件结束的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException NestedEndOfFile(string paramName)
		{
			throw GetArgumentException(paramName, "NestedEndOfFile");
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
		/// 返回正则表达式字符类不可以合并的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="System.ArgumentException"/> 对象。</returns>
		internal static ArgumentException RegexCharClassCannotMerge(string paramName)
		{
			throw GetArgumentException(paramName, "RegexCharClassCannotMerge");
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

		#endregion // 正则表达式异常

	}
}
