namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析器的工厂。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class ParserFactory<T> : IParserFactory<T>
	where T : struct
{
	/// <summary>
	/// 动作处理器。
	/// </summary>
	private static readonly Func<Delegate, ParserController<T>, object?> actionHandler =
		(Delegate action, ParserController<T> controller) =>
		{
			return ((Func<ParserController<T>, object?>)action)(controller);
		};

	/// <summary>
	/// 语法分析器的数据。
	/// </summary>
	private readonly ParserData<T> parserData;

	/// <summary>
	/// 使用指定的语法分析器数据初始化 <see cref="ParserFactory{T}"/> 类的新实例。
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
		ParserController<T> controller = new();
		LRParser<T> parser = new(parserData, controller);
		controller.Init(parserData, parser, actionHandler);
		return parser;
	}
}
