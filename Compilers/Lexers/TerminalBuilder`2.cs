namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 词法分析的终结符构造器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <typeparam name="TController">词法分析控制器的类型。</typeparam>
internal sealed class TerminalBuilder<T, TController> : ITerminalBuilder<T, TController>
	where T : struct
	where TController : LexerController<T>, new()
{
	/// <summary>
	/// 词法分析器。
	/// </summary>
	private readonly Lexer<T, TController> lexer;
	/// <summary>
	/// 词法分析的终结符。
	/// </summary>
	private readonly Terminal<T> terminal;

	/// <summary>
	/// 使用词法分析器和终结符初始化 <see cref="TerminalBuilder{T,TController}"/> 类的新实例。
	/// </summary>
	/// <param name="lexer">词法分析器。</param>
	/// <param name="terminal">终结符。</param>
	public TerminalBuilder(Lexer<T, TController> lexer, Terminal<T> terminal)
	{
		this.lexer = lexer;
		this.terminal = terminal;
	}

	/// <summary>
	/// 添加正则表达式所属的上下文。
	/// </summary>
	/// <param name="contexts">词法分析器的上下文。</param>
	/// <returns>终结符的构造器。</returns>
	public ITerminalBuilder<T, TController> Context(params string[] contexts)
	{
		foreach (string label in contexts)
		{
			if (label != null)
			{
				terminal.Matches[0].Context.Add(lexer.GetContext(label));
			}
		}
		return this;
	}

	/// <summary>
	/// 添加正则表达式对应的词法单元类型。
	/// </summary>
	/// <param name="kind">词法单元的类型。</param>
	/// <returns>终结符的构造器。</returns>
	public ITerminalBuilder<T, TController> Kind(T kind)
	{
		terminal.Kind = kind;
		return this;
	}

	/// <summary>
	/// 设置终结符的值。
	/// </summary>
	/// <param name="value">词法单元的值。</param>
	/// <returns>终结符的构造器。</returns>
	public ITerminalBuilder<T, TController> Value(object? value)
	{
		terminal.Value = value;
		return this;
	}

	/// <summary>
	/// 设置使用终结符的最短匹配。
	/// </summary>
	/// <returns>终结符的构造器。</returns>
	/// <remarks>默认都会使用正则表达式的最长匹配，允许指定为使用最短匹配，
	/// 会在遇到第一个匹配时立即返回结果。</remarks>
	public ITerminalBuilder<T, TController> UseShortest()
	{
		terminal.UseShortest = true;
		return this;
	}

	/// <summary>
	/// 添加正则表达式对应的词法单元动作。
	/// </summary>
	/// <param name="action">词法单元的动作。</param>
	/// <returns>终结符的构造器。</returns>
	public void Action(Action<TController> action)
	{
		if (action != null)
		{
			terminal.Action = action;
		}
	}
}
