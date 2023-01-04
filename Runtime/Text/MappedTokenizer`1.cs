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
	private readonly Tuple<int, int>[] map;
	/// <summary>
	/// 映射索引。
	/// </summary>
	private int mapIndex = 0;
	/// <summary>
	/// 当前索引。
	/// </summary>
	private int curIndex;
	/// <summary>
	/// 当前映射后的偏移。
	/// </summary>
	private int curOffset;
	/// <summary>
	/// 下一索引。
	/// </summary>
	private int nextIndex;
	/// <summary>
	/// 下一映射后的索引。
	/// </summary>
	private int nextMappedIndex;

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
		this.map = map.ToArray();
		Array.Sort(this.map, (left, right) => left.Item1 - right.Item1);
		if (this.map.Length == 0)
		{
			curIndex = 0;
			curOffset = 0;
		}
		else
		{
			curIndex = this.map[0].Item1;
			curOffset = this.map[0].Item2 - curIndex;
		}
		FindNextIndex();
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
			Span = new TextSpan(MapIndex(start), MapIndex(end))
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
	/// 寻找下一个索引。
	/// </summary>
	private void FindNextIndex()
	{
		mapIndex++;
		if (mapIndex < map.Length)
		{
			nextIndex = map[mapIndex].Item1;
			nextMappedIndex = map[mapIndex].Item2;
		}
		else
		{
			nextIndex = int.MaxValue;
			nextMappedIndex = int.MaxValue;
		}
	}

	/// <summary>
	/// 映射指定的索引，总是按照索引从小到大的顺序调用。
	/// </summary>
	/// <param name="index">要映射的索引。</param>
	/// <returns>映射后的索引。</returns>
	private int MapIndex(int index)
	{
		// 在首个索引之前。
		if (index < curIndex)
		{
			return index;
		}
		// 在当前索引范围之外，需要切换到下一索引。
		while (index >= nextIndex)
		{
			curIndex = nextIndex;
			curOffset = nextMappedIndex - nextIndex;
			FindNextIndex();
		}
		index += curOffset;
		// 避免 index 超出 nextMappedIndex
		if (index >= nextMappedIndex)
		{
			index = nextMappedIndex - 1;
		}
		return index;
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
