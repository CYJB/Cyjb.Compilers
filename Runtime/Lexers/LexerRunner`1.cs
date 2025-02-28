using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 词法分析运行器，适合直接在词法分析控制器中执行逻辑而不生成 <see cref="Token{T}"/> 的场景。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class LexerRunner<T>
	where T : struct
{
	/// <summary>
	/// 词法分析器的核心。
	/// </summary>
	private readonly LexerCore<T> core;

	/// <summary>
	/// 使用指定的词法分析器核心初始化 <see cref="LexerRunner{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerCore">词法分析器核心。</param>
	internal LexerRunner(LexerCore<T> lexerCore)
	{
		core = lexerCore;
	}

	/// <summary>
	/// 是否在读入词法单元后自动释放已读取的字符。
	/// </summary>
	/// <value>如果为 <c>true</c>，那么在读取词法单元后会自动释放已读取的字符以节约内存占用；
	/// 如果为 <c>false</c>，那么只会设置 <c>StartIndex</c>。默认为 <c>false</c></value>
	public bool AutoFree { get; set; } = false;
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
		Parse(SourceReader.Create(source));
	}

	/// <summary>
	/// 解析指定的源文件。
	/// </summary>
	/// <param name="source">要加载的源读取器。</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	public void Parse(SourceReader source)
	{
		ArgumentNullException.ThrowIfNull(source);
		LexerController<T> controller = core.Controller;
		if (controller.SharedContext != SharedContext)
		{
			controller.SharedContext = SharedContext;
		}
		core.Load(source);
		int start = 0;
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
					if (AutoFree)
					{
						source.Free();
					}
					else
					{
						source.Drop();
					}
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
				if (AutoFree)
				{
					source.Free();
				}
				controller.Start = start;
				controller.CreateTokenizeError(text, controller.Span);
			}
		}
		// 到达了流的结尾。
		ContextData context = controller.CurrentContext;
		controller.DoAction(source.Index, Token<T>.EndOfFile, context.EofValue, context.EofAction);

		// 释放数据。
		source.Dispose();
		controller.Dispose();
	}
}
