using System.Runtime.CompilerServices;

namespace Cyjb.Compilers
{
	/// <summary>
	/// 提供用于异常处理的辅助方法。
	/// </summary>
	internal static class CompilerExceptions
	{

		#region 正则表达式异常

		/// <summary>
		/// 返回嵌套的行起始的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="ArgumentException"/> 对象。</returns>
		public static ArgumentException NestedBeginningOfLine(string? paramName)
		{
			return new ArgumentException(Resources.NestedBeginningOfLine, paramName);
		}

		/// <summary>
		/// 返回嵌套的文件结束的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="ArgumentException"/> 对象。</returns>
		public static ArgumentException NestedEndOfFile(string? paramName)
		{
			return new ArgumentException(Resources.NestedEndOfFile, paramName);
		}

		/// <summary>
		/// 返回嵌套的行结束的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="ArgumentException"/> 对象。</returns>
		public static ArgumentException NestedEndOfLine(string? paramName)
		{
			return new ArgumentException(Resources.NestedEndOfLine, paramName);
		}

		/// <summary>
		/// 返回嵌套的向前看的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="ArgumentException"/> 对象。</returns>
		public static ArgumentException NestedTrailing(string? paramName)
		{
			return new ArgumentException(Resources.NestedTrailing, paramName);
		}

		/// <summary>
		/// 返回未识别的 Unicode 类别的异常。
		/// </summary>
		/// <param name="name">产生异常的参数名称。</param>
		/// <returns><see cref="ArgumentException"/> 对象。</returns>
		public static ArgumentException UnrecognizedUnicodeProperty(string? name)
		{
			return new ArgumentException(ResourcesUtil.Format(Resources.UnrecognizedUnicodeProperty, name));
		}

		#endregion // 正则表达式异常

		#region 词法分析异常

		/// <summary>
		/// 返回不完整正词法分析上下文的异常。
		/// </summary>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="ArgumentException"/> 对象。</returns>
		public static ArgumentException IncompleteLexerContext(string? paramName)
		{
			return new ArgumentException(Resources.IncompleteLexerContext, paramName);
		}

		/// <summary>
		/// 返回不允许拒绝动作的异常。
		/// </summary>
		/// <returns><see cref="System.InvalidOperationException"/> 对象。</returns>
		public static InvalidOperationException NotRejectable()
		{
			return new InvalidOperationException(Resources.NotRejectable);
		}

		#endregion // 词法分析异常

		/// <summary>
		/// 返回重复的符号标识符的异常。
		/// </summary>
		/// <param name="id">重复的符号标识符。</param>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="ArgumentException"/> 对象。</returns>
		public static ArgumentException DuplicatedSymbolId(string id, [CallerArgumentExpression("id")] string? paramName = null)
		{
			return new ArgumentException(ResourcesUtil.Format(Resources.DuplicatedSymbolId, id), paramName);
		}

		/// <summary>
		/// 返回无效的符号标识符的异常。
		/// </summary>
		/// <param name="id">无效的符号标识符。</param>
		/// <param name="paramName">产生异常的参数名称。</param>
		/// <returns><see cref="ArgumentException"/> 对象。</returns>
		public static ArgumentException InvalidSymbolId(string id, [CallerArgumentExpression("id")] string? paramName = null)
		{
			return new ArgumentException(ResourcesUtil.Format(Resources.InvalidSymbolId, id), paramName);
		}
	}
}
