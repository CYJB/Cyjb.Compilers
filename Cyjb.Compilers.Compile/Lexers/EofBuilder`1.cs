namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 词法分析的 End of File 动作构造器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class EofBuilder<T> : ITerminalBuilder<T>
	where T : struct
{
	/// <summary>
	/// 词法分析器。
	/// </summary>
	private readonly Lexer<T> lexer;
	/// <summary>
	/// 词法分析的上下文。
	/// </summary>
	private readonly HashSet<LexerContext> contexts = new();

	/// <summary>
	/// 使用词法分析器初始化 <see cref="TerminalBuilder{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexer">词法分析器。</param>
	public EofBuilder(Lexer<T> lexer)
	{
		this.lexer = lexer;
	}

	/// <summary>
	/// 添加正则表达式所属的上下文。
	/// </summary>
	/// <param name="contexts">词法分析器的上下文。</param>
	/// <returns>终结符的构造器。</returns>
	public ITerminalBuilder<T> Context(params string[] contexts)
	{
		foreach (string label in contexts)
		{
			if (label != null)
			{
				this.contexts.Add(lexer.GetContext(label));
			}
		}
		return this;
	}

	/// <summary>
	/// 添加正则表达式对应的词法单元类型。
	/// </summary>
	/// <param name="kind">词法单元的类型。</param>
	/// <returns>终结符的构造器。</returns>
	public ITerminalBuilder<T> Kind(T kind)
	{
		// EOF 动作无法指定类型。
		return this;
	}

	/// <summary>
	/// 添加正则表达式对应的词法单元动作。
	/// </summary>
	/// <param name="action">词法单元的动作。</param>
	/// <returns>终结符的构造器。</returns>
	public void Action(Action<LexerController<T>> action)
	{
		if (action != null)
		{
			foreach (LexerContext context in contexts)
			{
				context.EofAction = action;
			}
		}
	}
}
