namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 词法分析的终结符构造器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public interface ITerminalBuilder<T>
	where T : struct
{
	/// <summary>
	/// 设置正则表达式的上下文。
	/// </summary>
	/// <param name="contexts">正则表达式的上下文。</param>
	/// <returns>终结符的构造器。</returns>
	ITerminalBuilder<T> Context(params string[] contexts);

	/// <summary>
	/// 设置终结符的词法单元类型。
	/// </summary>
	/// <param name="kind">词法单元的类型。</param>
	/// <returns>终结符的构造器。</returns>
	ITerminalBuilder<T> Kind(T kind);

	/// <summary>
	/// 设置终结符的动作。
	/// </summary>
	/// <param name="action">终结符的动作。</param>
	/// <returns>终结符的构造器。</returns>
	void Action(Action<LexerController<T>> action);
}
