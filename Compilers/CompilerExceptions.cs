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
	/// 返回产生式的优先级必须是一个终结符的异常。
	/// </summary>
	/// <param name="name">异常的符号名称。</param>
	/// <returns><see cref="InvalidOperationException"/> 对象。</returns>
	public static InvalidOperationException PrecedenceMustBeTerminal(string name)
	{
		return new InvalidOperationException(Resources.PrecedenceMustBeTerminal(name));
	}
}
