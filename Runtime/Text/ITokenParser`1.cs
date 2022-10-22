namespace Cyjb.Text;

/// <summary>
/// 表示语法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public interface ITokenParser<T>
	where T : struct
{
	/// <summary>
	/// 语法分析错误的事件。
	/// </summary>
	event Action<ITokenParser<T>, TokenParseError> ParseError;
	/// <summary>
	/// 获取语法分析器的解析状态。
	/// </summary>
	ParseStatus Status { get; }
	/// <summary>
	/// 获取或设置共享的上下文对象。
	/// </summary>
	/// <remarks>可以与外部（例如语法分析器）共享信息。</remarks>
	object? SharedContext { get; set; }

	/// <summary>
	/// 使用默认的目标类型分析当前词法单元序列。
	/// </summary>
	public abstract ParserNode<T> Parse();

	/// <summary>
	/// 使用指定的目标类型分析当前词法单元序列。
	/// </summary>
	/// <param name="target">目标类型。</param>
	public abstract ParserNode<T> Parse(T target);

	/// <summary>
	/// 取消后续语法分析。
	/// </summary>
	void Cancel();
}
