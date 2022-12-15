using Cyjb.Text;

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
	/// 创建分析指定的词法单元序列语法分析器。
	/// </summary>
	/// <param name="tokenizer">要分析的词法单元序列。</param>
	/// <returns>指定词法单元序列的语法分析器。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="tokenizer"/> 为 <c>null</c>。</exception>
	public ITokenParser<T> CreateParser(ITokenizer<T> tokenizer)
	{
		ArgumentNullException.ThrowIfNull(tokenizer);
		TController controller = new();
		controller.Init(parserData, tokenizer, actionHandler);
		return new LRParser<T>(parserData, tokenizer, controller);
	}
}
