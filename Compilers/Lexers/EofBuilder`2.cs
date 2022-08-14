namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 词法分析的 End of File 动作构造器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <typeparam name="TController">词法分析控制器的类型。</typeparam>
internal sealed class EofBuilder<T, TController> : ITerminalBuilder<T, TController>
	where T : struct
	where TController : LexerController<T>, new()
{
	/// <summary>
	/// 词法分析器。
	/// </summary>
	private readonly Lexer<T, TController> lexer;
	/// <summary>
	/// 词法分析的上下文。
	/// </summary>
	private readonly HashSet<LexerContext> contexts = new();

	/// <summary>
	/// 使用词法分析器初始化 <see cref="TerminalBuilder{T,TController}"/> 类的新实例。
	/// </summary>
	/// <param name="lexer">词法分析器。</param>
	public EofBuilder(Lexer<T, TController> lexer)
	{
		this.lexer = lexer;
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
	public ITerminalBuilder<T, TController> Kind(T kind)
	{
		// EOF 动作无法指定类型。
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
			if (contexts.Count == 0)
			{
				// 未指定任何上下文时，设置到初始状态。
				lexer.GetContext(ContextData.Initial).EofAction = action;
			}
			else
			{
				foreach (LexerContext context in contexts)
				{
					context.EofAction = action;
				}
			}
		}
	}
}
