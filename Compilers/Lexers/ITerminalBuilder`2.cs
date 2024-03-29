namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 词法分析的终结符构造器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam> 
/// <typeparam name="TController">词法分析控制器的类型。</typeparam>
public interface ITerminalBuilder<T, TController>
	where T : struct
	where TController : LexerController<T>, new()
{
	/// <summary>
	/// 设置正则表达式的上下文。
	/// </summary>
	/// <param name="contexts">正则表达式的上下文。</param>
	/// <returns>终结符的构造器。</returns>
	ITerminalBuilder<T, TController> Context(params string[] contexts);

	/// <summary>
	/// 设置终结符的词法单元类型。
	/// </summary>
	/// <param name="kind">词法单元的类型。</param>
	/// <returns>终结符的构造器。</returns>
	ITerminalBuilder<T, TController> Kind(T kind);

	/// <summary>
	/// 设置终结符的值。
	/// </summary>
	/// <param name="value">词法单元的值。</param>
	/// <returns>终结符的构造器。</returns>
	ITerminalBuilder<T, TController> Value(object? value);

	/// <summary>
	/// 设置使用终结符的最短匹配。
	/// </summary>
	/// <returns>终结符的构造器。</returns>
	/// <remarks>默认都会使用正则表达式的最长匹配，允许指定为使用最短匹配，
	/// 会在遇到第一个匹配时立即返回结果。</remarks>
	ITerminalBuilder<T, TController> UseShortest();

	/// <summary>
	/// 设置终结符的动作。
	/// </summary>
	/// <param name="action">终结符的动作。</param>
	/// <returns>终结符的构造器。</returns>
	void Action(Action<TController> action);
}
