using System.Runtime.CompilerServices;

namespace Cyjb.Compilers;

/// <summary>
/// 提供用于异常处理的辅助方法。
/// </summary>
internal static class CompilerExceptions
{
	/// <summary>
	/// 返回重复的词法分析上下文的异常。
	/// </summary>
	/// <param name="context">词法分析上下文名称。</param>
	/// <returns><see cref="ArgumentException"/> 对象。</returns>
	public static ArgumentException DuplicateLexerContext(string? context)
	{
		return new ArgumentException(Resources.DuplicateLexerContext(context));
	}

	/// <summary>
	/// 返回重复的符号标识符的异常。
	/// </summary>
	/// <param name="id">重复的符号标识符。</param>
	/// <param name="paramName">产生异常的参数名称。</param>
	/// <returns><see cref="ArgumentException"/> 对象。</returns>
	public static ArgumentException DuplicatedSymbolId(string id, [CallerArgumentExpression("id")] string? paramName = null)
	{
		return new ArgumentException(Resources.DuplicatedSymbolId(id), paramName);
	}

	/// <summary>
	/// 返回无效的符号标识符的异常。
	/// </summary>
	/// <param name="id">无效的符号标识符。</param>
	/// <param name="paramName">产生异常的参数名称。</param>
	/// <returns><see cref="ArgumentException"/> 对象。</returns>
	public static ArgumentException InvalidSymbolId(string id, [CallerArgumentExpression("id")] string? paramName = null)
	{
		return new ArgumentException(Resources.InvalidSymbolId(id), paramName);
	}
}
