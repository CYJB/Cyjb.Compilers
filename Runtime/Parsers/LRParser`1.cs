using System.Diagnostics;
using Cyjb.Collections;
using Cyjb.Text;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 LR 词法单元分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
internal sealed class LRParser<T> : ITokenParser<T>
	where T : struct
{
	/// <summary>
	/// 语法规则数据。
	/// </summary>
	private readonly ParserData<T> data;
	/// <summary>
	/// 词法分析器。
	/// </summary>
	private readonly ITokenizer<T> tokenizer;
	/// <summary>
	/// 语法节点堆栈。
	/// </summary>
	private readonly ListStack<ParserNode<T>> nodeStack = new();
	/// <summary>
	/// 状态堆栈。
	/// </summary>
	private readonly ListStack<int> stateStack = new();
	/// <summary>
	/// 语法分析器的控制器。
	/// </summary>
	private readonly ParserController<T> controller;
	/// <summary>
	/// 解析状态。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ParseStatus status = ParseStatus.Ready;
	/// <summary>
	/// 是否已同步共享上下文。
	/// </summary>
	private bool contextSynced = false;

	/// <summary>
	/// 语法分析错误的事件。
	/// </summary>
	public event TokenParseErrorHandler<T>? ParseError;

	/// <summary>
	/// 使用指定的 LR 规则和词法分析器初始化 <see cref="LRParser{T}"/> 类的新实例。
	/// </summary>
	/// <param name="data">LR 语法分析器的规则。</param>
	/// <param name="tokenizer">词法分析器。</param>
	/// <param name="controller">语法分析控制器。</param>
	internal LRParser(ParserData<T> data, ITokenizer<T> tokenizer, ParserController<T> controller)
	{
		this.data = data;
		this.tokenizer = tokenizer;
		this.controller = controller;
		controller.SetParser(this);
	}

	/// <summary>
	/// 获取状态堆栈。
	/// </summary>
	internal ListStack<int> StateStack => stateStack;

	/// <summary>
	/// 获取语法分析器的解析状态。
	/// </summary>
	public ParseStatus Status => status;
	/// <summary>
	/// 获取或设置共享的上下文对象。
	/// </summary>
	/// <remarks>可以与外部（例如语法分析器）共享信息。</remarks>
	public object? SharedContext
	{
		get { return tokenizer.SharedContext; }
		set { tokenizer.SharedContext = value; }
	}

	/// <summary>
	/// 使用默认的目标类型分析当前词法单元序列。
	/// </summary>
	public ParserNode<T> Parse()
	{
		return ParseTokens(0);
	}

	/// <summary>
	/// 使用指定的目标类型分析当前词法单元序列。
	/// </summary>
	/// <param name="target">目标类型。</param>
	public ParserNode<T> Parse(T target)
	{
		int initialState = 0;
		if (data.StartStates != null)
		{
			initialState = data.StartStates.GetValueOrDefault(target, 0);
		}
		return ParseTokens(initialState);
	}

	/// <summary>
	/// 分析当前词法单元序列。
	/// </summary>
	private ParserNode<T> ParseTokens(int initialState)
	{
		if (!contextSynced)
		{
			contextSynced = true;
			controller.SharedContext = tokenizer.SharedContext;
		}
		if (status != ParseStatus.Ready)
		{
			return new ParserNode<T>(Token<T>.GetEndOfFile(controller.Span.End));
		}
		stateStack.Push(initialState);
		Token<T> token = controller.Peek();
		ParserNode<T> node = new(token);
		while (true)
		{
			if (status == ParseStatus.Cancelled)
			{
				CancelStates();
				break;
			}
			int state = stateStack.Peek();
			ParserAction action = data.GetAction(state, token.Kind);
			switch (action.Type)
			{
				case ParserActionType.Shift:
					if (!node.IsMissing)
					{
						// 消费词法单元。
						controller.Read();
					}
					stateStack.Push(action.Index);
					nodeStack.Push(node);
					PreReduce();
					// 读取下一词法单元。
					token = controller.Peek();
					node = new ParserNode<T>(token);
					break;
				case ParserActionType.Reduce:
					Reduce(data.Productions[action.Index], token.Span.Start);
					break;
				case ParserActionType.Accept:
					goto Accept;
				case ParserActionType.Error:
					// 错误恢复
					if (!controller.TryRecover(state, ref token))
					{
						controller.EmitParseError(new UnexpectedTokenError<T>(token, data.GetExpecting(state)));
						controller.PanicRecover();
						token = controller.Peek();
					}
					node = new ParserNode<T>(token)
					{
						IsMissing = controller.IsMissingToken,
						SkippedTokens = controller.SkippedTokens.ToArray(),
					};
					break;
			}
		}
	Accept:
		stateStack.Clear();
		ParserNode<T> result = nodeStack.Pop();
		if (status == ParseStatus.Ready && token.IsEndOfFile)
		{
			// 已达到词法单元的末尾。
			status = ParseStatus.Finished;
		}
		return result;
	}

	/// <summary>
	/// 尝试提前执行唯一的规约动作，可以提前执行部分 Action。
	/// </summary>
	private void PreReduce()
	{
		while (true)
		{
			ParserStateData<T> topState = data.States[stateStack.Peek()];
			// 只有唯一的规约动作，
			ParserAction action = topState.DefaultAction;
			if (action.Type == ParserActionType.Reduce && topState.Actions.Count == 0)
			{
				// 这里总是有 token，start 的值不影响。
				Reduce(data.Productions[action.Index], 0);
			}
			else
			{
				break;
			}
		}
	}

	/// <summary>
	/// 使用指定的产生式规约。
	/// </summary>
	/// <param name="production">规约时使用的产生式。</param>
	/// <param name="start">源码的起始位置。</param>
	private void Reduce(ProductionData<T> production, int start)
	{
		int size = production.BodySize;
		TextSpan span;
		if (size > 0)
		{
			span = TextSpan.Combine(nodeStack[0].Span, nodeStack[size - 1].Span);
		}
		else if (nodeStack.Count > 0)
		{
			int end = nodeStack[0].Span.End;
			span = new TextSpan(end, end);
		}
		else
		{
			span = new TextSpan(start, start);
		}
		ParserNode<T> node = new(production.Head, span);
		stateStack.Pop(size);
		controller.Reduce(node, nodeStack, size, production.Action);
		nodeStack.Push(node);
		stateStack.Push(data.Goto(stateStack.Peek(), production.Head));
	}

	/// <summary>
	/// 不断规约状态，直到能够接受当前词法单元。
	/// </summary>
	internal void SyncState()
	{
		Token<T> token = controller.Peek();
		TextSpan span = new(token.Span.Start, token.Span.Start);
		// 不对最后的增广非终结符规约
		while (stateStack.Count > 1)
		{
			ParserStateData<T> state = data.States[stateStack.Peek()];
			if (state.Expecting.Contains(token.Kind))
			{
				// 能接受指定词法单元。
				return;
			}
			// 补充缺失的词法单元。
			ProductionData<T> production = state.RecoverProduction;
			for (int i = state.RecoverIndex; i < production.BodySize; i++)
			{
				nodeStack.Push(new ParserNode<T>(new Token<T>(production.Body[i], StringView.Empty, span))
				{
					IsMissing = true,
				});
				// 用于占位的无效状态。
				stateStack.Push(ParserData.InvalidState);
			}
			// 执行规约
			Reduce(production, token.Span.Start);
		}
	}

	/// <summary>
	/// 上报语法分析错误。
	/// </summary>
	/// <param name="error">语法分析错误。</param>
	internal void ReportParseError(TokenParseError error)
	{
		ParseError?.Invoke(this, error);
	}

	/// <summary>
	/// 取消后续语法分析。
	/// </summary>
	public void Cancel()
	{
		if (status == ParseStatus.Ready)
		{
			status = ParseStatus.Cancelled;
		}
	}

	/// <summary>
	/// 重置语法分析的状态，允许在结束/取消后继续进行分析。
	/// </summary>
	public void Reset()
	{
		status = ParseStatus.Ready;
	}

	/// <summary>
	/// 取消所有状态，会将状态归约到接受状态。
	/// </summary>
	private void CancelStates()
	{
		int start = controller.Peek().Span.Start;
		TextSpan span = new(start, start);
		while (true)
		{
			ParserStateData<T> state = data.States[stateStack.Peek()];
			if (state.GetAction(Token<T>.EndOfFile).Type == ParserActionType.Accept)
			{
				break;
			}
			// 补充缺失的词法单元。
			ProductionData<T> production = state.RecoverProduction;
			for (int i = state.RecoverIndex; i < production.BodySize; i++)
			{
				nodeStack.Push(new ParserNode<T>(new Token<T>(production.Body[i], StringView.Empty, span))
				{
					IsMissing = true,
				});
				// 用于占位的无效状态。
				stateStack.Push(ParserData.InvalidState);
			}
			// 执行规约
			Reduce(production, start);
		}
	}
}
