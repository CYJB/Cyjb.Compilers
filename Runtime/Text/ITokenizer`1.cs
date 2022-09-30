namespace Cyjb.Text;

/// <summary>
/// 表示一个词法分析器。
/// </summary>
/// <seealso cref="Token{T}"/>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public interface ITokenizer<T> : IDisposable, IEnumerable<Token<T>>
	where T : struct
{
	/// <summary>
	/// 词法分析错误的事件。
	/// </summary>
	event Action<ITokenizer<T>, TokenizeError> TokenizeError;
	/// <summary>
	/// 获取词法分析器的解析状态。
	/// </summary>
	ParseStatus Status { get; }
	/// <summary>
	/// 获取或设置共享的上下文对象。
	/// </summary>
	/// <remarks>可以与外部（例如语法分析器）共享信息。</remarks>
	object? SharedContext { get; set; }

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <returns>输入流中的下一个词法单元。</returns>
	Token<T> Read();

	/// <summary>
	/// 取消后续词法分析。
	/// </summary>
	void Cancel();
}
