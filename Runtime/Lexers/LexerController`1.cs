using System.Diagnostics;
using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的控制器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public class LexerController<T>
	where T : struct
{
	/// <summary>
	/// 关联到的词法分析器。
	/// </summary>
	private TokenizerBase<T> tokenizer;
	/// <summary>
	/// 要扫描的源文件。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SourceReader source;
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

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

	/// <summary>
	/// 初始化 <see cref="LexerController{T}"/> 类的新实例。
	/// </summary>
	public LexerController() { }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

	/// <summary>
	/// 设置指定的词法分析控制器信息。
	/// </summary>
	/// <param name="source">源文件读取器。</param>
	/// <param name="contexts">词法分析的上下文。</param>
	/// <param name="actionHandler">动作的处理器。</param>
	/// <param name="rejectable">是否允许 Reject 动作。</param>
	internal void Init(SourceReader source, IReadOnlyDictionary<string, ContextData> contexts,
		Action<Delegate, LexerController<T>> actionHandler, bool rejectable)
	{
		this.source = source;
		this.contexts = contexts;
		context = contexts[ContextData.Initial];
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
	public SourceReader Source => source;
	/// <summary>
	/// 获取共享的上下文对象。
	/// </summary>
	/// <remarks>可以与外部（例如语法分析器）共享信息。</remarks>
	public object? SharedContext => tokenizer.SharedContext;
	/// <summary>
	/// 获取当前词法单元的标识符。
	/// </summary>
	public T? Kind { get; private set; }
	/// <summary>
	/// 获取或设置当前词法单元的文本。
	/// </summary>
	public string Text { get; set; } = string.Empty;
	/// <summary>
	/// 获取或设置当前词法单元的起始索引。
	/// </summary>
	public int Start { get; set; }
	/// <summary>
	/// 获取或设置当前词法单元的值。
	/// </summary>
	public object? Value { get; set; }

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
	/// 获取下次匹配成功时，是否不替换当前的文本，而是把新的匹配追加在后面。
	/// </summary>
	internal bool IsMore { get; private set; }

	/// <summary>
	/// 设置关联到的词法分析器。
	/// </summary>
	/// <param name="tokenizer">关联到的词法分析器。</param>
	internal void SetTokenizer(TokenizerBase<T> tokenizer)
	{
		this.tokenizer = tokenizer;
	}

	/// <summary>
	/// 开始一个新的词法分析环境。
	/// </summary>
	/// <param name="start">当前词法单元的起始索引。</param>
	/// <param name="terminal">当前词法单元的终结符数据。</param>
	internal void DoAction(int start, TerminalData<T> terminal)
	{
		Kind = terminal.Kind;
		Text = source.ReadedText();
		Start = start;
		Value = terminal.Value;
		if (terminal.Action == null)
		{
			userAccepted = true;
			IsReject = false;
			IsMore = false;
		}
		else
		{
			userAccepted = false;
			IsReject = false;
			IsMore = false;
			actionHandler(terminal.Action, this);
		}
	}

	/// <summary>
	/// 开始 EndOfFile 词法分析环境。
	/// </summary>
	/// <param name="start">当前词法单元的起始索引。</param>
	/// <param name="value">当前词法单元的值。</param>
	/// <param name="action">当前要执行的动作。</param>
	internal void DoEofAction(int start, object? value, Delegate? action)
	{
		Kind = Token<T>.EndOfFile;
		Text = source.ReadedText();
		Start = start;
		Value = value;
		if (action == null)
		{
			userAccepted = true;
			IsReject = false;
			IsMore = false;
		}
		else
		{
			userAccepted = false;
			IsReject = false;
			IsMore = false;
			actionHandler(action, this);
		}
	}

	/// <summary>
	/// 根据当前词法分析接受结果创建 <see cref="Token{T}"/> 的新实例。
	/// </summary>
	/// <returns><see cref="Token{T}"/> 的新实例。</returns>
	internal Token<T> CreateToken()
	{
		return new Token<T>(Kind!.Value, Text, new TextSpan(Start, source.Index), source.Locator, Value);
	}

	/// <summary>
	/// 触发词法分析错误的事件。
	/// </summary>
	/// <param name="text">未识别的字符串。</param>
	/// <param name="span">未识别的字符串范围。</param>
	internal protected virtual void EmitTokenizeError(string text, TextSpan span)
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
		Value = value;
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
		Value = value;
	}

	/// <summary>
	/// 拒绝当前的匹配，并尝试寻找下一个匹配。如果找不到下一个匹配，则会返回错误。
	/// </summary>
	public void Reject()
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
	}

	/// <summary>
	/// 在下次匹配成功时，不替换当前的文本，而是把新的匹配追加在后面。
	/// </summary>
	public void More()
	{
		IsMore = true;
	}

	#region 上下文切换

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
