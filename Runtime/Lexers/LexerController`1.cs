using System.Diagnostics;
using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的控制器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public class LexerController<T> : IDisposable
	where T : struct
{
	/// <summary>
	/// 关联到的词法分析器。
	/// </summary>
	private LexerTokenizer<T> tokenizer;
	/// <summary>
	/// 要扫描的源文件。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SourceReader source = SourceReader.Empty;
	/// <summary>
	/// 当前词法分析的上下文。
	/// </summary>
	private IReadOnlyDictionary<string, ContextData> contexts;
	/// <summary>
	/// 当前的上下文。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ContextData context;
	/// <summary>
	/// 动作的处理器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Action<Delegate, LexerController<T>> actionHandler;
	/// <summary>
	/// 上下文的堆栈。
	/// </summary>
	private readonly Stack<ContextData> contextStack = new();
	/// <summary>
	/// 是否允许 Reject 动作。
	/// </summary>
	private bool rejectable;
	/// <summary>
	/// 是否用户指定了接受。
	/// </summary>
	private bool userAccepted = false;
	/// <summary>
	/// 当前词法单元的文本。
	/// </summary>
	private StringView? text;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

	/// <summary>
	/// 初始化 <see cref="LexerController{T}"/> 类的新实例。
	/// </summary>
	public LexerController() { }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

	/// <summary>
	/// 设置指定的词法分析控制器信息。
	/// </summary>
	/// <param name="tokenizer">关联到的词法分析器。</param>
	/// <param name="contexts">词法分析的上下文。</param>
	/// <param name="actionHandler">动作的处理器。</param>
	/// <param name="rejectable">是否允许 Reject 动作。</param>
	internal void Init(LexerTokenizer<T> tokenizer, IReadOnlyDictionary<string, ContextData> contexts,
		Action<Delegate, LexerController<T>> actionHandler, bool rejectable)
	{
		this.tokenizer = tokenizer;
		this.contexts = contexts;
		context = contexts[InitialContext];
		this.actionHandler = actionHandler;
		this.rejectable = rejectable;
	}

	/// <summary>
	/// 获取当前的上下文标签。
	/// </summary>
	/// <value>当前的上下文标签。</value>
	public string Context => context.Label;
	/// <summary>
	/// 获取要扫描的源文件。
	/// </summary>
	/// <value>要扫描的源文件。</value>
	public SourceReader Source
	{
		get => source;
		internal set
		{
			if (source != value)
			{
				source = value;
				SourceLoaded();
			}
		}
	}
	/// <summary>
	/// 获取或设置共享的上下文对象。
	/// </summary>
	/// <remarks>可以与外部（例如语法分析器）共享信息，
	/// 会在首次调用 <see cref="ITokenizer{T}.Read"/> 前设置。</remarks>
	public virtual object? SharedContext { get; set; }
	/// <summary>
	/// 获取当前词法单元的标识符。
	/// </summary>
	public T? Kind { get; private set; }
	/// <summary>
	/// 获取或设置当前词法单元的文本。
	/// </summary>
	public StringView Text
	{
		get => text ?? Source.GetReadedText();
		set => text = value;
	}
	/// <summary>
	/// 获取或设置当前词法单元的起始索引。
	/// </summary>
	public int Start { get; set; }
	/// <summary>
	/// 获取或设置当前词法单元的值。
	/// </summary>
	public object? Value { get; set; }
	/// <summary>
	/// 获取当前词法单元的文本范围。
	/// </summary>
	public virtual TextSpan Span => new(Start, source.Index);
	/// <summary>
	/// 获取当前词法分析器剩余的候选类型。
	/// </summary>
	/// <remarks>仅在允许 Reject 动作的词法分析器中，返回剩余的候选类型。</remarks>
	public IReadOnlySet<T> Candidates => tokenizer.Candidates;

	/// <summary>
	/// 获取当前的上下文数据。
	/// </summary>
	internal ContextData CurrentContext => context;
	/// <summary>
	/// 获取是否接受了当前的词法单元。
	/// </summary>
	internal bool IsAccept => !IsReject && Kind != null;
	/// <summary>
	/// 获取是否拒绝了当前的词法单元。
	/// </summary>
	internal bool IsReject { get; private set; }
	/// <summary>
	/// 获取是否拒绝了当前状态。
	/// </summary>
	internal bool IsRejectState { get; private set; }
	/// <summary>
	/// 获取下次匹配成功时，是否不替换当前的文本，而是把新的匹配追加在后面。
	/// </summary>
	internal bool IsMore { get; private set; }

	/// <summary>
	/// 开始一个新的词法分析环境。
	/// </summary>
	/// <param name="start">当前词法单元的起始索引。</param>
	/// <param name="terminal">当前词法单元的终结符数据。</param>
	internal void DoAction(int start, TerminalData<T> terminal)
	{
		DoAction(start, terminal.Kind, terminal.Value, terminal.Action);
	}

	/// <summary>
	/// 开始 EndOfFile 词法分析环境。
	/// </summary>
	/// <param name="start">当前词法单元的起始索引。</param>
	/// <param name="value">当前词法单元的值。</param>
	/// <param name="action">当前要执行的动作。</param>
	internal void DoEofAction(int start, object? value, Delegate? action)
	{
		DoAction(start, Token<T>.EndOfFile, value, action);
	}

	/// <summary>
	/// 开始 EndOfFile 词法分析环境。
	/// </summary>
	/// <param name="start">当前词法单元的起始索引。</param>
	/// <param name="kind">当前词法单元的标识符。</param>
	/// <param name="value">当前词法单元的值。</param>
	/// <param name="action">当前要执行的动作。</param>
	private void DoAction(int start, T? kind, object? value, Delegate? action)
	{
		Kind = kind;
		Start = start;
		text = null;
		Value = value;
		IsReject = false;
		IsRejectState = false;
		IsMore = false;
		if (action == null)
		{
			userAccepted = true;
		}
		else
		{
			userAccepted = false;
			actionHandler(action, this);
		}
	}

	/// <summary>
	/// 已加载源读取器。
	/// </summary>
	protected virtual void SourceLoaded()
	{
		contextStack.Clear();
	}

	/// <summary>
	/// 根据当前词法分析接受结果创建 <see cref="Token{T}"/> 的新实例。
	/// </summary>
	/// <returns><see cref="Token{T}"/> 的新实例。</returns>
	internal protected virtual Token<T> CreateToken()
	{
		if (text.HasValue)
		{
			// 使用自定义文本。
			return new Token<T>(Kind!.Value, text.Value, Span, Value);
		}
		else
		{
			// 未设置文本，使用原始文本。
			return source.AcceptToken(Kind!.Value, Span, Value);
		}
	}

	/// <summary>
	/// 触发词法分析错误的事件。
	/// </summary>
	/// <param name="text">未识别的字符串。</param>
	/// <param name="span">未识别的字符串范围。</param>
	internal protected virtual void EmitTokenizeError(StringView text, TextSpan span)
	{
		tokenizer.ReportTokenizeError(new TokenizeError(text, span, source.Locator));
	}

	/// <summary>
	/// 接受当前的匹配。
	/// </summary>
	/// <param name="value">词法单元的值。</param>
	/// <overloads>
	/// <summary>
	/// 接受当前的匹配。
	/// </summary>
	/// </overloads>
	public void Accept(object? value = null)
	{
		if (IsReject)
		{
			throw new InvalidOperationException(Resources.ConflictingAcceptAction);
		}
		if (Kind == null)
		{
			throw new InvalidOperationException(Resources.InvalidLexerKind);
		}
		userAccepted = true;
		if (value != null)
		{
			Value = value;
		}
	}

	/// <summary>
	/// 接受当前的匹配，使用指定的标识符。
	/// </summary>
	/// <param name="kind">匹配的标识符。</param>
	/// <param name="value">词法单元的值。</param>
	public void Accept(T kind, object? value = null)
	{
		if (IsReject)
		{
			throw new InvalidOperationException(Resources.ConflictingAcceptAction);
		}
		userAccepted = true;
		Kind = kind;
		if (value != null)
		{
			Value = value;
		}
	}

	/// <summary>
	/// 拒绝当前的匹配，并尝试寻找下一个匹配。如果找不到下一个匹配，则会返回错误。
	/// </summary>
	/// <param name="options">拒绝匹配的选项。</param>
	public void Reject(RejectOptions options = RejectOptions.Default)
	{
		if (!rejectable)
		{
			throw new InvalidOperationException(Resources.NotRejectable);
		}
		// 仅在用户指定了接受时才报错。
		if (userAccepted)
		{
			throw new InvalidOperationException(Resources.ConflictingAcceptAction);
		}
		IsReject = true;
		if (options == RejectOptions.State)
		{
			IsRejectState = true;
		}
	}

	/// <summary>
	/// 在下次匹配成功时，不替换当前的文本，而是把新的匹配追加在后面。
	/// </summary>
	public void More()
	{
		IsMore = true;
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
	}

	#endregion // IDisposable 成员

	#region 上下文切换

	/// <summary>
	/// 获取初始上下文。
	/// </summary>
	protected virtual string InitialContext => ContextData.Initial;

	/// <summary>
	/// 将指定标签的上下文设置为当前的上下文。
	/// </summary>
	/// <param name="label">要设置的上下文的标签。</param>
	public void BeginContext(string label)
	{
		context = GetContext(label);
	}

	/// <summary>
	/// 将当前上下文压入堆栈，并将上下文设置为指定的值。
	/// </summary>
	/// <param name="label">要设置的上下文的标签。</param>
	public void PushContext(string label)
	{
		contextStack.Push(context);
		context = GetContext(label);
	}

	/// <summary>
	/// 弹出堆栈顶的上下文，并把它设置为当前的上下文。
	/// </summary>
	public void PopContext()
	{
		if (contextStack.Count > 0)
		{
			context = contextStack.Pop();
		}
		else
		{
			context = contexts[ContextData.Initial];
		}
	}

	/// <summary>
	/// 进入指定上下文。
	/// </summary>
	/// <remarks>如果当前上下文不是 <paramref name="label"/>，则将其压入堆栈，
	/// 并将 <paramref name="label"/> 设置为当前上下文。否则什么都不做。</remarks>
	/// <param name="label">要进入的上下文的标签。</param>
	public void EnterContext(string label)
	{
		if (context.Label != label)
		{
			PushContext(label);
		}
	}

	/// <summary>
	/// 退出指定上下文。
	/// </summary>
	/// <remarks>如果当前上下文是 <paramref name="label"/>，则弹出堆栈顶的上下文用作当前上下文。
	/// 否则什么都不做。</remarks>
	/// <param name="label">要退出的上下文的标签。</param>

	public void ExitContext(string label)
	{
		if (context.Label == label)
		{
			PopContext();
		}
	}

	/// <summary>
	/// 返回指定标签的词法分析器上下文。
	/// </summary>
	/// <param name="label">要获取的词法分析器上下文的标签。</param>
	/// <returns>有效的词法分析器上下文。</returns>
	private ContextData GetContext(string label)
	{
		if (contexts.TryGetValue(label, out ContextData? context))
		{
			return context;
		}
		throw new ArgumentException(Resources.InvalidLexerContext(context), nameof(label));
	}

	#endregion // 上下文切换

}
