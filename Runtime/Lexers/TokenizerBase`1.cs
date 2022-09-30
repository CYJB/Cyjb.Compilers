using System.Collections;
using System.Diagnostics;
using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的基类。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal abstract class TokenizerBase<T> : ITokenizer<T>
	where T : struct
{
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
	protected readonly SourceReader source;
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
	/// 词法分析错误的事件。
	/// </summary>
	public event Action<ITokenizer<T>, TokenizeError>? TokenizeError;

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="TokenizerBase{T}"/> 类的新实例。
	/// </summary>
	/// <param name="data">要使用的词法分析器数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	/// <param name="source">要使用的源文件读取器。</param>
	protected TokenizerBase(LexerData<T> data, LexerController<T> controller, SourceReader source)
	{
		this.data = data;
		this.controller = controller;
		this.source = source;
		controller.Tokenizer = this;
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
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <returns>输入流中的下一个词法单元。</returns>
	public Token<T> Read()
	{
		if (status != ParseStatus.Ready)
		{
			return Token<T>.GetEndOfFile(source.Index);
		}
		while (true)
		{
			ContextData<T> context = controller.CurrentContext;
			if (source.Peek() == SourceReader.InvalidCharacter)
			{
				// 到达了流的结尾。
				if (context.EofAction != null)
				{
					controller.DoAction(source.Index, null, context.EofAction);
					if (controller.IsAccept)
					{
						return controller.CreateToken();
					}
				}
				status = ParseStatus.Finished;
				return Token<T>.GetEndOfFile(source.Index);
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
				if (!controller.IsMore && !controller.IsReject)
				{
					source.Drop();
				}
				if (controller.IsAccept)
				{
					return controller.CreateToken();
				}
			}
			else
			{
				// 到达死状态。
				string text = source.Accept();
				if (text.Length == 0)
				{
					// 如果没有匹配任何字符，强制读入一个字符，可以防止死循环出现。
					source.Read();
					text = source.Accept();
				}
				controller.EmitTokenizeError(text, new TextSpan(start, source.Index));
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
