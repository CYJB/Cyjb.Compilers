namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析器的工厂。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <typeparam name="TController">语法分析控制器的类型。</typeparam>
public sealed class ParserFactory<T, TController> : IParserFactory<T>
	where T : struct
	where TController : ParserController<T>, new()
{
	/// <summary>
	/// 动作处理器。
	/// </summary>
	private static readonly Func<Delegate, ParserController<T>, object?> actionHandler =
		(Delegate action, ParserController<T> controller) =>
		{
			return ((Func<TController, object?>)action)((TController)controller);
		};

	/// <summary>
	/// 语法分析器的数据。
	/// </summary>
	private readonly ParserData<T> parserData;

	/// <summary>
	/// 使用指定的语法分析器数据初始化 <see cref="ParserFactory{T,TController}"/> 类的新实例。
	/// </summary>
	/// <param name="parserData">语法分析器的数据。</param>
	public ParserFactory(ParserData<T> parserData)
	{
		this.parserData = parserData;
	}

	/// <summary>
	/// 创建语法分析器。
	/// </summary>
	/// <returns>已创建的语法分析器。</returns>
	public LRParser<T> CreateParser()
	{
		TController controller = new();
		LRParser<T> parser = new(parserData, controller);
		controller.Init(parserData, parser, actionHandler);
		return parser;
	}
}
