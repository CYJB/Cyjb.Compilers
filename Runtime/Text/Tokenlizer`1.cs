using System.Collections;

namespace Cyjb.Text;

/// <summary>
/// 表示词法分析器。
/// </summary>
/// <seealso cref="Token{T}"/>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public abstract class Tokenlizer<T> : IDisposable, IEnumerable<Token<T>>
	where T : struct
{
	/// <summary>
	/// 使用要扫描的源文件初始化 <see cref="Tokenlizer{T}"/> 类的新实例。
	/// </summary>
	/// <param name="source">要使用的源文件读取器。</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	protected Tokenlizer(SourceReader source)
	{
		ArgumentNullException.ThrowIfNull(source);
		Source = source;
	}

	/// <summary>
	/// 获取要扫描的源文件。
	/// </summary>
	/// <value>要扫描的源文件。</value>
	public SourceReader Source { get; }

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <returns>输入流中的下一个词法单元。</returns>
	public abstract Token<T> Read();

	#region IDisposable 成员

	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	/// <overloads>
	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	/// </overloads>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	/// <param name="disposing">是否释放托管资源。</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Source.Dispose();
		}
	}

	#endregion // IDisposable 成员

	#region IEnumerable<Token<T>> 成员

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/>。</returns>
	/// <remarks>在枚举的时候，<see cref="Tokenlizer{T}"/> 会不断的读出词法单元，
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
