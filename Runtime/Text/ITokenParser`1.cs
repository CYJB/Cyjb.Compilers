namespace Cyjb.Text;

/// <summary>
/// 表示语法分析错误的事件处理器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <param name="parser">出现错误的语法分析器。</param>
/// <param name="error">错误信息</param>
public delegate void TokenParseErrorHandler<T>(ITokenParser<T> parser, TokenParseError error)
	where T : struct;


/// <summary>
/// 表示语法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public interface ITokenParser<T> : IDisposable
	where T : struct
{
	/// <summary>
	/// 语法分析错误的事件。
	/// </summary>
	event TokenParseErrorHandler<T>? ParseError;
	/// <summary>
	/// 获取语法分析器的解析状态。
	/// </summary>
	ParseStatus Status { get; }
	/// <summary>
	/// 获取或设置共享的上下文对象。
	/// </summary>
	/// <remarks>可以与外部（例如词法分析器）共享信息，只能够在首次调用 <see cref="Parse()"/> 前设置。</remarks>
	object? SharedContext { get; set; }

	/// <summary>
	/// 使用默认的目标类型分析当前词法单元序列。
	/// </summary>
	/// <returns>语法分析的结果。</returns>
	public abstract ParserNode<T> Parse();

	/// <summary>
	/// 使用指定的目标类型分析当前词法单元序列。
	/// </summary>
	/// <param name="target">目标类型。</param>
	/// <returns>语法分析的结果。</returns>
	public abstract ParserNode<T> Parse(T target);

	/// <summary>
	/// 取消后续语法分析。
	/// </summary>
	void Cancel();

	/// <summary>
	/// 重置语法分析的状态，允许在结束/取消后继续进行分析。
	/// </summary>
	void Reset();
}
