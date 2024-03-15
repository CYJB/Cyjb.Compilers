using System.Collections;

namespace Cyjb.Text;

/// <summary>
/// 提供空的词法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class EmptyTokenizer<T> : ITokenizer<T>
	where T : struct
{
	/// <summary>
	/// 空的词法分析器实例。
	/// </summary>
	public static readonly ITokenizer<T> Empty = new EmptyTokenizer<T>();

	/// <summary>
	/// 词法分析错误的事件。
	/// </summary>
	public event TokenizeErrorHandler<T>? TokenizeError;

	/// <summary>
	/// 获取词法分析器的解析状态。
	/// </summary>
	public ParseStatus Status => ParseStatus.Finished;
	/// <summary>
	/// 获取或设置共享的上下文对象。
	/// </summary>
	/// <remarks>可以与外部（例如语法分析器）共享信息。</remarks>
	public object? SharedContext { get; set; }

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <returns>输入流中的下一个词法单元。</returns>
	public Token<T> Read()
	{
		return Token<T>.GetEndOfFile(0);
	}

	/// <summary>
	/// 取消后续词法分析。
	/// </summary>
	public void Cancel() { }

	/// <summary>
	/// 重置词法分析的状态，允许在结束/取消后继续进行分析。
	/// </summary>
	public void Reset() { }

	#region IDisposable 成员

	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	public void Dispose() { }

	#endregion // IDisposable 成员

	#region IEnumerable<Token<T>> 成员

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/>。</returns>
	/// <remarks>在枚举的时候，<see cref="ITokenizer{T}"/> 会不断的读出词法单元，
	/// 应当总是只使用一个枚举器。在使用多个枚举器时，他们之间会相互干扰，导致枚举值与期望的不同。
	/// 如果需要多次枚举，必须将词法单元缓存到数组中，再进行枚举。</remarks>
	public IEnumerator<Token<T>> GetEnumerator()
	{
		yield break;
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator"/>。</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion // IEnumerable<Token<T>> 成员

}
