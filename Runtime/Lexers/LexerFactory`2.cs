namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的工厂。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <typeparam name="TController">词法分析控制器的类型。</typeparam>
/// <remarks>关于如何构造自己的词法分析器，可以参考我的博文
/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html">
/// 《C# 词法分析器（六）构造词法分析器》</see>。</remarks>
/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html">
/// 《C# 词法分析器（六）构造词法分析器》</seealso>
public sealed class LexerFactory<T, TController> : ILexerFactory<T>
	where T : struct
	where TController : LexerController<T>, new()
{
	/// <summary>
	/// 动作处理器。
	/// </summary>
	private static readonly Action<Delegate, LexerController<T>> actionHandler =
		(Delegate action, LexerController<T> controller) =>
		{
			((Action<TController>)action)((TController)controller);
		};

	/// <summary>
	/// 词法分析器的数据。
	/// </summary>
	private readonly LexerData<T> lexerData;

	/// <summary>
	/// 使用指定的词法分析器数据初始化 <see cref="LexerFactory{T,TController}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">词法分析器的数据。</param>
	public LexerFactory(LexerData<T> lexerData)
	{
		this.lexerData = lexerData;
	}

	/// <summary>
	/// 创建词法分析器。
	/// </summary>
	/// <returns>已创建的词法分析器。</returns>
	public LexerTokenizer<T> CreateTokenizer()
	{
		TController controller = new()
		{
			ActionHandler = actionHandler
		};
		LexerCore<T> core = LexerCore<T>.Create(lexerData, controller);
		controller.SetCore(core, lexerData.Contexts, lexerData.Rejectable);
		return new LexerTokenizer<T>(core);
	}

	/// <summary>
	/// 创建词法分析运行器。
	/// </summary>
	/// <returns>已创建的词法分析运行器。</returns>
	public LexerRunner<T> CreateRunner()
	{
		TController controller = new()
		{
			ActionHandler = actionHandler
		};
		LexerCore<T> core = LexerCore<T>.Create(lexerData, controller);
		controller.SetCore(core, lexerData.Contexts, lexerData.Rejectable);
		return new LexerRunner<T>(core);
	}
}
