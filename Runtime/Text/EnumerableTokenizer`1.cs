using System.Collections;
using System.Diagnostics;

namespace Cyjb.Text;

/// <summary>
/// 提供将 <see cref="Token{T}"/> 枚举封装为词法分析器的能力。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class EnumerableTokenizer<T> : ITokenizer<T>
	where T : struct
{
	/// <summary>
	/// 被封装的枚举器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IEnumerator<Token<T>> enumerator;
	/// <summary>
	/// 最近一次读取的词法单元。
	/// </summary>
	private Token<T> lastToken = Token<T>.Empty;
	/// <summary>
	/// 解析状态。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ParseStatus status = ParseStatus.Ready;

	/// <summary>
	/// 词法分析错误的事件。
	/// </summary>
	public event TokenizeErrorHandler<T>? TokenizeError;

	/// <summary>
	/// 使用指定的词法单元枚举初始化 <see cref="EnumerableTokenizer{T}"/> 类的新实例。
	/// </summary>
	/// <param name="tokens">要包装的词法单元枚举。</param>
	public EnumerableTokenizer(IEnumerable<Token<T>> tokens)
	{
		enumerator = tokens.GetEnumerator();
	}

	/// <summary>
	/// 使用指定的词法单元列表初始化 <see cref="EnumerableTokenizer{T}"/> 类的新实例。
	/// </summary>
	/// <param name="tokens">要包装的词法单元枚举。</param>
	public EnumerableTokenizer(params Token<T>[] tokens)
	{
		enumerator = tokens.Cast<Token<T>>().GetEnumerator();
	}

	/// <summary>
	/// 获取词法分析器的解析状态。
	/// </summary>
	public ParseStatus Status => status;
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
		if (status == ParseStatus.Ready && enumerator.MoveNext())
		{
			lastToken = enumerator.Current;
			return lastToken;
		}
		else
		{
			return Token<T>.GetEndOfFile(lastToken.Span.End);
		}
	}

	/// <summary>
	/// 取消后续词法分析。
	/// </summary>
	public void Cancel()
	{
		if (status == ParseStatus.Ready)
		{
			status = ParseStatus.Cancelled;
		}
	}

	/// <summary>
	/// 重置词法分析的状态，允许在结束/取消后继续进行分析。
	/// </summary>
	public void Reset()
	{
		status = ParseStatus.Ready;
	}

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
		while (true)
		{
			Token<T> token = Read();
			yield return token;
			if (token.IsEndOfFile)
			{
				break;
			}
		}
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
