using System.Collections;
using System.Diagnostics;
using Cyjb.Collections;
using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public abstract class LexerTokenizer<T> : ITokenizer<T>
	where T : struct
{
	/// <summary>
	/// 创建词法分析器。
	/// </summary>
	/// <param name="lexerData">词法分析器数据。</param>
	/// <param name="controller">词法分析器的控制器。</param>
	/// <returns>词法分析器。</returns>
	internal static LexerTokenizer<T> Create(LexerData<T> lexerData, LexerController<T> controller)
	{
		if (lexerData.Rejectable)
		{
			if (lexerData.TrailingType == TrailingType.None)
			{
				return new TokenizerRejectable<T>(lexerData, controller);
			}
		}
		else
		{
			if (lexerData.TrailingType == TrailingType.None)
			{
				return new TokenizerSimpler<T>(lexerData, controller);
			}
			else if (lexerData.TrailingType == TrailingType.Fixed)
			{
				return new TokenizerFixedTrailing<T>(lexerData, controller);
			}
		}
		return new TokenizerRejectableTrailing<T>(lexerData, controller);
	}

	/// <summary>
	/// 词法分析器的数据。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly LexerData<T> data;
	/// <summary>
	/// 当前词法分析器的控制器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly LexerController<T> controller;
	/// <summary>
	/// 源码读取器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	protected SourceReader source = SourceReader.Empty;
	/// <summary>
	/// 解析状态。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ParseStatus status = ParseStatus.Ready;
	/// <summary>
	/// 当前词法单元的起始位置。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int start;
	/// <summary>
	/// 是否已同步共享上下文。
	/// </summary>
	private bool contextSynced = false;
	/// <summary>
	/// 已拒绝的状态列表。
	/// </summary>
	private readonly HashSet<int> rejectedStates = new();

	/// <summary>
	/// 词法分析错误的事件。
	/// </summary>
	public event TokenizeErrorHandler<T>? TokenizeError;

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="LexerTokenizer{T}"/> 类的新实例。
	/// </summary>
	/// <param name="data">要使用的词法分析器数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	protected LexerTokenizer(LexerData<T> data, LexerController<T> controller)
	{
		this.data = data;
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
		Load(new SourceReader(new StringReader(source)));
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
		// 重置状态。
		status = ParseStatus.Ready;
		rejectedStates.Clear();
		start = 0;
		controller.Source = source;
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
	/// 获取词法分析器数据。
	/// </summary>
	protected LexerData<T> Data => data;
	/// <summary>
	/// 获取词法分析器的控制器。
	/// </summary>
	protected LexerController<T> Controller => controller;
	/// <summary>
	/// 获取当前词法单元的起始位置。
	/// </summary>
	/// <value>当前词法单元的起始位置。</value>
	protected int Start => start;
	/// <summary>
	/// 获取当前词法分析器剩余的候选类型。
	/// </summary>
	/// <remarks>仅在允许 Reject 动作的词法分析器中，返回剩余的候选类型。</remarks>
	internal virtual IReadOnlySet<T> Candidates => SetUtil.Empty<T>();

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
			if (data.ContainsBeginningOfLine)
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
			if (NextToken(state))
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
				controller.EmitTokenizeError(text, controller.Span);
			}
		}
	}

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <param name="startState">DFA 的起始状态。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected abstract bool NextToken(int startState);

	/// <summary>
	/// 使用源文件中的下一个字符转移到后续状态。
	/// </summary>
	/// <param name="state">当前状态索引。</param>
	/// <returns>转以后的状态，使用 <c>-1</c> 表示没有找到合适的状态。</returns>
	protected int NextState(int state)
	{
		char ch = source.Read();
		return data.NextState(state, ch);
	}

	/// <summary>
	/// 上报词法分析错误。
	/// </summary>
	/// <param name="error">词法分析错误。</param>
	internal void ReportTokenizeError(TokenizeError error)
	{
		TokenizeError?.Invoke(this, error);
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

	/// <summary>
	/// 返回指定符号列表中的候选类型。
	/// </summary>
	/// <param name="symbols">要检查的符号列表。</param>
	/// <returns><paramref name="symbols"/> 中包含的候选状态。</returns>
	protected IEnumerable<T> GetCandidates(ArraySegment<int> symbols)
	{
		foreach (int acceptState in symbols)
		{
			if (acceptState < 0)
			{
				// 跳过向前看的头状态。
				break;
			}
			if (rejectedStates.Contains(acceptState))
			{
				continue;
			}
			var kind = Data.Terminals[acceptState].Kind;
			if (kind.HasValue)
			{
				yield return kind.Value;
			}
		}
	}

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
			source.Dispose();
			controller.Dispose();
		}
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
