using System.Collections;
using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class LexerTokenizer<T> : ITokenizer<T>
	where T : struct
{
	/// <summary>
	/// 词法分析器的核心。
	/// </summary>
	private readonly LexerCore<T> core;
	/// <summary>
	/// 当前词法分析器的控制器。
	/// </summary>
	private readonly LexerController<T> controller;
	/// <summary>
	/// 源码读取器。
	/// </summary>
	private SourceReader source = SourceReader.Empty;
	/// <summary>
	/// 解析状态。
	/// </summary>
	private ParseStatus status = ParseStatus.Ready;
	/// <summary>
	/// 当前词法单元的起始位置。
	/// </summary>
	private int start;
	/// <summary>
	/// 是否已同步共享上下文。
	/// </summary>
	private bool contextSynced = false;

	/// <summary>
	/// 词法分析错误的事件。
	/// </summary>
	public event TokenizeErrorHandler<T>? TokenizeError;

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="LexerTokenizer{T}"/> 类的新实例。
	/// </summary>
	/// <param name="data">要使用的词法分析器数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	internal LexerTokenizer(LexerData<T> data, LexerController<T> controller)
	{
		core = LexerCore<T>.Create(data, controller);
		this.controller = controller;
	}

	/// <summary>
	/// 加载指定的源码。
	/// </summary>
	/// <param name="source">要加载的源码。</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	public void Load(string source)
	{
		ArgumentNullException.ThrowIfNull(source);
		Load(SourceReader.Create(source));
	}

	/// <summary>
	/// 加载指定的源读取器。
	/// </summary>
	/// <param name="source">要加载的源读取器。</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	public void Load(SourceReader source)
	{
		ArgumentNullException.ThrowIfNull(source);
		if (this.source == source)
		{
			// 相同源不重复加载。
			return;
		}
		this.source = source;
		core.Load(source);
		// 重置状态。
		start = 0;
		status = ParseStatus.Ready;
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
		if (!contextSynced)
		{
			contextSynced = true;
			controller.SharedContext = SharedContext;
		}
		if (status != ParseStatus.Ready)
		{
			controller.DoEofAction(source.Index, null, null);
			return controller.CreateToken();
		}
		while (true)
		{
			ContextData context = controller.CurrentContext;
			if (source.Peek() == SourceReader.InvalidCharacter)
			{
				// 到达了流的结尾。
				controller.DoEofAction(source.Index, context.EofValue, context.EofAction);
				status = ParseStatus.Finished;
				return controller.CreateToken();
			}
			// 起始状态与当前上下文相关。
			int state = context.Index;
			if (core.ContainsBeginningOfLine)
			{
				state *= 2;
				if (source.IsLineStart)
				{
					// 行首规则。
					state++;
				}
			}
			if (!controller.IsMore)
			{
				start = source.Index;
			}
			if (core.NextToken(state, start))
			{
				// 需要先 CreateToken 确保文本已读入。
				Token<T>? token = null;
				if (controller.IsAccept)
				{
					token = controller.CreateToken();
				}
				// 再丢弃不需要的数据。
				if (!controller.IsMore && !controller.IsReject)
				{
					source.Drop();
				}
				if (token != null)
				{
					return token;
				}
			}
			else
			{
				// 到达死状态。
				StringView text = source.Accept();
				if (text.IsEmpty)
				{
					// 如果没有匹配任何字符，强制读入一个字符，可以防止死循环出现。
					source.Read();
					text = source.Accept();
				}
				controller.Start = start;
				if (TokenizeError != null)
				{
					var error = controller.CreateTokenizeError(text, controller.Span);
					if (error != null)
					{
						TokenizeError(this, error);
					}
				}
			}
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
	public void Dispose()
	{
		source.Dispose();
		controller.Dispose();
		GC.SuppressFinalize(this);
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
