using System.Diagnostics;
using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 词法分析运行器，适合直接在词法分析控制器中执行逻辑而不生成 <see cref="Token{T}"/> 的场景。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class LexerRunner<T> : IDisposable
	where T : struct
{
	/// <summary>
	/// 词法分析器的核心。
	/// </summary>
	private readonly LexerCore<T> core;
	/// <summary>
	/// 当前词法分析器的控制器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly LexerController<T> controller;
	/// <summary>
	/// 源码读取器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SourceReader source = SourceReader.Empty;
	/// <summary>
	/// 当前词法单元的起始位置。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int start;

	/// <summary>
	/// 使用指定的词法分析数据初始化 <see cref="LexerRunner{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">词法分析器数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	internal LexerRunner(LexerData<T> lexerData, LexerController<T> controller)
	{
		core = LexerCore<T>.Create(lexerData, controller);
		this.controller = controller;
	}

	/// <summary>
	/// 获取或设置共享的上下文对象。
	/// </summary>
	/// <remarks>可以与外部（例如语法分析器）共享信息。</remarks>
	public object? SharedContext { get; set; }

	/// <summary>
	/// 解析指定的源文件。
	/// </summary>
	/// <param name="source">要加载的源码。</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	public void Parse(string source)
	{
		ArgumentNullException.ThrowIfNull(source);
		Parse(new SourceReader(new StringReader(source)));
	}

	/// <summary>
	/// 解析指定的源文件。
	/// </summary>
	/// <param name="source">要加载的源读取器。</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	public void Parse(SourceReader source)
	{
		ArgumentNullException.ThrowIfNull(source);
		// 释放旧的源码读取器。
		this.source.Dispose();
		// 开始新的读取。
		this.source = source;
		start = 0;
		if (controller.SharedContext != SharedContext)
		{
			controller.SharedContext = SharedContext;
		}
		core.Load(source);
		while (source.Peek() != SourceReader.InvalidCharacter)
		{
			// 起始状态与当前上下文相关。
			int state = controller.CurrentContext.Index;
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
				if (!controller.IsMore && !controller.IsReject)
				{
					source.Drop();
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
					source.Drop();
				}
				controller.Start = start;
				controller.CreateTokenizeError(text, controller.Span);
			}
		}
		// 到达了流的结尾。
		ContextData context = controller.CurrentContext;
		controller.DoEofAction(source.Index, context.EofValue, context.EofAction);
	}

	/// <summary>
	/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
	/// </summary>
	public void Dispose()
	{
		source.Dispose();
		controller.Dispose();
		GC.SuppressFinalize(this);
	}
}
