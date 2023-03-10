using System.Collections;

namespace Cyjb.Text;

/// <summary>
/// 表示支持重映射对词法单元位置的词法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class MappedTokenizer<T> : ITokenizer<T>
	where T : struct
{
	/// <summary>
	/// 被封装的词法分析器。
	/// </summary>
	private readonly ITokenizer<T> tokenizer;
	/// <summary>
	/// 映射关系。
	/// </summary>
	private readonly LocationMap map;

	/// <summary>
	/// 词法分析错误的事件。
	/// </summary>
	public event TokenizeErrorHandler<T>? TokenizeError
	{
		add => tokenizer.TokenizeError += value;
		remove => tokenizer.TokenizeError -= value;
	}

	/// <summary>
	/// 使用指定的词法分析器和位置映射关系初始化 <see cref="MappedTokenizer{T}"/> 类的新实例。
	/// </summary>
	/// <param name="tokenizer">要包装的词法分析器。</param>
	/// <param name="map">位置映射关系，会将 <see cref="Tuple{T1, T2}.Item1"/> 映射为
	/// <see cref="Tuple{T1, T2}.Item2"/>。未列出的值会根据前一映射关系计算。</param>
	public MappedTokenizer(ITokenizer<T> tokenizer, IEnumerable<Tuple<int, int>> map)
	{
		this.tokenizer = tokenizer;
		this.map = new LocationMap(map);
	}

	/// <summary>
	/// 获取词法分析器的解析状态。
	/// </summary>
	public ParseStatus Status => tokenizer.Status;
	/// <summary>
	/// 获取或设置共享的上下文对象。
	/// </summary>
	/// <remarks>可以与外部（例如语法分析器）共享信息。</remarks>
	public object? SharedContext
	{
		get => tokenizer.SharedContext;
		set => tokenizer.SharedContext = value;
	}

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <returns>输入流中的下一个词法单元。</returns>
	public Token<T> Read()
	{
		Token<T> token = tokenizer.Read();
		var (start, end) = token.Span;
		return token with
		{
			Span = new TextSpan(map.MapLocation(start), map.MapLocation(end))
		};
	}

	/// <summary>
	/// 取消后续词法分析。
	/// </summary>
	public void Cancel()
	{
		tokenizer.Cancel();
	}

	/// <summary>
	/// 重置词法分析的状态，允许在结束/取消后继续进行分析。
	/// </summary>
	public void Reset()
	{
		tokenizer.Reset();
	}

	/// <summary>
	/// 映射指定的位置。
	/// </summary>
	/// <param name="location">要映射的位置。</param>
	/// <returns>映射后的位置。</returns>
	public int MapLocation(int location)
	{
		return map.MapLocation(location);
	}

	#region IDisposable 成员

	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	public void Dispose()
	{
		tokenizer.Dispose();
	}

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
