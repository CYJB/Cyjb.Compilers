namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示文法的产生式构造器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <typeparam name="TController">语法分析控制器的类型。</typeparam>
public sealed class ProductionBuilder<T, TController>
	where T : struct
	where TController : ParserController<T>, new()
{
	/// <summary>
	/// 关联到的语法分析器。
	/// </summary>
	private readonly Parser<T, TController> parser;
	/// <summary>
	/// 关联到的产生式。
	/// </summary>
	private readonly Production<T> production;

	/// <summary>
	/// 使用语法分析器和产生式初始化 <see cref="ProductionBuilder{T, TController}"/> 类的新实例。
	/// </summary>
	/// <param name="parser">关联到的语法分析器。</param>
	/// <param name="production">关联到的产生式。</param>
	internal ProductionBuilder(Parser<T, TController> parser, Production<T> production)
	{
		this.parser = parser;
		this.production = production;
	}

	/// <summary>
	/// 设置当前产生式的动作。
	/// </summary>
	/// <param name="action">当前产生式体的动作。</param>
	/// <returns>当前产生式。</returns>
	public ProductionBuilder<T, TController> Action(Func<TController, object?> action)
	{
		production.Action = action;
		return this;
	}

	/// <summary>
	/// 设置当前产生式的优先级与指定的非终结符相同。
	/// </summary>
	/// <param name="kind">非终结符的标识符。</param>
	/// <returns>当前产生式。</returns>
	public ProductionBuilder<T, TController> Prec(T kind)
	{
		production.Precedence = parser.GetOrCreateSymbol(kind);
		return this;
	}
}
