using System.Collections;
using System.Diagnostics;
using Cyjb.Collections;
using Cyjb.Collections.ObjectModel;
using Cyjb.Text;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析器的控制器。
/// </summary>
/// <typeparam name="T">语法节点标识符的类型，必须是一个枚举类型。</typeparam>
public class ParserController<T> : ReadOnlyListBase<ParserNode<T>>
	where T : struct
{
	/// <summary>
	/// 语法规则数据。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ParserData<T> data;
	/// <summary>
	/// 关联到的语法分析器。
	/// </summary>
	private LRParser<T> parser;
	/// <summary>
	/// 语法分析器的状态堆栈。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IReadOnlyList<int> stateStack;
	/// <summary>
	/// 词法分析器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ITokenizer<T> tokenizer;
	/// <summary>
	/// 动作的处理器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Func<Delegate, ParserController<T>, object?> actionHandler;
	/// <summary>
	/// 词法单元缓存。
	/// </summary>
	private readonly ListQueue<Token<T>> tokenCache = new();
	/// <summary>
	/// 当前语法节点列表。
	/// </summary>
	private readonly List<ParserNode<T>> nodes = new();
	/// <summary>
	/// 语法节点信息。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ParserNode<T> node = new(Token<T>.GetEndOfFile(0));
	/// <summary>
	/// 被过滤掉的词法单元列表。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly TokenCollection<T> hiddenTokens = new();

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

	/// <summary>
	/// 初始化 <see cref="ParserController{T}"/> 类的新实例。
	/// </summary>
	public ParserController() { }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

	/// <summary>
	/// 设置指定的词法分析控制器信息。
	/// </summary>
	/// <param name="data">语法规则数据。</param>
	/// <param name="tokenizer">词法分析器。</param>
	/// <param name="actionHandler">动作的处理器。</param>
	internal void Init(ParserData<T> data, ITokenizer<T> tokenizer, Func<Delegate, ParserController<T>, object?> actionHandler)
	{
		this.data = data;
		this.tokenizer = tokenizer;
		this.actionHandler = actionHandler;
	}

	/// <summary>
	/// 获取词法分析器。
	/// </summary>
	public ITokenizer<T> Tokenizer => tokenizer;
	/// <summary>
	/// 获取语法节点的范围。
	/// </summary>
	/// <value>语法节点的范围。</value>
	public TextSpan Span => node.Span;
	/// <summary>
	/// 获取语法节点的行列位置范围。
	/// </summary>
	/// <value>语法节点的行列位置范围。</value>
	public LinePositionSpan LinePositionSpan => node.LinePositionSpan;
	/// <summary>
	/// 获取共享的上下文对象。
	/// </summary>
	/// <remarks>可以与外部（例如语法分析器）共享信息。</remarks>
	public object? SharedContext
	{
		get { return tokenizer.SharedContext; }
		internal set { tokenizer.SharedContext = value; }
	}

	/// <summary>
	/// 获取语法分析器的状态堆栈。
	/// </summary>
	protected IReadOnlyList<int> StateStack => stateStack;
	/// <summary>
	/// 获取语法规则数据。
	/// </summary>
	protected ParserData<T> Data => data;

	/// <summary>
	/// 设置关联到的语法分析器。
	/// </summary>
	/// <param name="parser">关联到的语法分析器。</param>
	internal void SetParser(LRParser<T> parser)
	{
		this.parser = parser;
		stateStack = parser.StateStack;
	}

	/// <summary>
	/// 执行指定的规约动作。
	/// </summary>
	/// <param name="node">语法节点。</param>
	/// <param name="stack">要添加词法单元的堆栈。</param>
	/// <param name="size">要添加的词法单元的数量。</param>
	/// <param name="action">要执行的动作。</param>
	internal void Reduce(ParserNode<T> node, ListStack<ParserNode<T>> stack, int size, Delegate? action)
	{
		this.node = node;
		nodes.Clear();
		if (size > 0)
		{
			for (int i = size - 1; i >= 0; i--)
			{
				nodes.Add(stack[i]);
			}
			stack.Pop(size);
		}
		if (action == null)
		{
			node.Value = nodes.Select((item) => item.Value).ToArray();
		}
		else if (action == ProductionAction.Optional)
		{
			if (nodes.Count > 0)
			{
				node.Value = nodes[0].Value;
			}
		}
		else if (action == ProductionAction.More)
		{
			ArrayList list;
			if (nodes.Count <= 1)
			{
				list = new ArrayList();
				if (nodes.Count > 0)
				{
					list.Add(nodes[0].Value);
				}
			}
			else
			{
				list = (ArrayList)nodes[0].Value!;
				list.Add(nodes[1].Value);
			}
			node.Value = list;
		}
		else
		{
			node.Value = actionHandler(action, this);
		}
	}

	#region 错误恢复

	/// <summary>
	/// 获取或设置错误恢复时是否缺少了词法单元。
	/// </summary>
	internal protected bool IsMissingToken { get; set; }
	/// <summary>
	/// 获取错误恢复时被跳过的词法单元。
	/// </summary>
	internal protected List<Token<T>> SkippedTokens { get; } = new();

	/// <summary>
	/// 触发语法分析错误的事件。
	/// </summary>
	/// <param name="error">语法分析错误。</param>
	internal protected virtual void EmitParseError(TokenParseError error)
	{
		parser.ReportParseError(error);
	}

	/// <summary>
	/// 尝试从指定的错误状态恢复。
	/// </summary>
	/// <param name="state">当前状态。</param>
	/// <param name="token">需要错误恢复的词法单元。</param>
	/// <returns>如果成功从错误状态恢复，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	internal protected virtual bool TryRecover(int state, ref Token<T> token)
	{
		IsMissingToken = false;
		SkippedTokens.Clear();
		return TryDeleteToken(state, ref token) || TryInsertToken(state, ref token);
	}

	/// <summary>
	/// 使用恐慌模式进行错误恢复。
	/// </summary>
	internal protected virtual void PanicRecover()
	{
		// 找到所有可能的 FOLLOW 集合，决定何时停止抛弃词法单元。
		int index = 0;
		int prevState = -1;
		int curState = stateStack[0];
		HashSet<T> allFollows = new();
		List<IReadOnlySet<T>> followList = new();
		while (true)
		{
			ParserStateData<T> stateData = data.States[curState];
			if (prevState < 0)
			{
				// 当前状态的预期词法单元类型也需要纳入考虑，而不总是强制规约。
				IReadOnlySet<T> follow = data.GetExpecting(curState);
				allFollows.UnionWith(follow);
				followList.Add(follow);
				index += stateData.RecoverIndex;
			}
			else
			{
				IReadOnlySet<T>? follow = data.GetFollow(prevState, curState);
				if (follow == null)
				{
					followList.Add(new HashSet<T>());
				}
				else
				{
					allFollows.UnionWith(follow);
					followList.Add(follow);
				}
				// 在前一状态已被规约后，定点会向后移动一个位，因此索引要 +1。
				index += stateData.RecoverIndex + 1;
			}
			prevState = curState;
			if (index >= stateStack.Count)
			{
				break;
			}
			curState = stateStack[index];
		}
		// 跳过所有不在 FOLLOW 集中的词法单元。 
		ConsumeTokens(allFollows);
		// 跳过无效状态。
		SyncState();
	}

	/// <summary>
	/// 尝试通过删除当前词法单元从而恢复错误。
	/// </summary>
	/// <param name="state">当前状态。</param>
	/// <param name="token">需要错误恢复的词法单元。</param>
	/// <returns>如果成功从错误状态恢复，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	private bool TryDeleteToken(int state, ref Token<T> token)
	{
		if (data.GetAction(state, Peek(1).Kind).Type != ParserActionType.Error)
		{
			EmitParseError(new UnexpectedTokenError<T>(token, data.GetExpecting(state)));
			SkippedTokens.Add(token);
			// 读入下一个词法单元作为错误恢复的结果。
			Read();
			token = Peek();
			return true;
		}
		return false;
	}

	/// <summary>
	/// 尝试通过在当前词法单元前插入一个词法单元从而恢复错误。
	/// </summary>
	/// <param name="state">当前状态。</param>
	/// <param name="token">需要错误恢复的词法单元。</param>
	/// <returns>如果成功从错误状态恢复，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	private bool TryInsertToken(int state, ref Token<T> token)
	{
		InsertTokenCandidate<T>? candicate = null;
		foreach (T kind in data.States[state].Expecting)
		{
			// 检查插入一个词法单元后能到达的状态
			int nextState = GetNextState(state, kind);
			if (data.GetAction(nextState, token.Kind).Type != ParserActionType.Error)
			{
				InsertTokenCandidate<T>? newCandicate = new(kind, data.GetAction(state, kind));
				if (newCandicate > candicate)
				{
					candicate = newCandicate;
				}
			}
		}
		if (candicate != null)
		{
			int start = token.Span.Start;
			token = token with
			{
				Kind = candicate.Kind,
				Text = string.Empty,
				Value = null,
				Span = new TextSpan(start, start),
			};
			IsMissingToken = true;
			EmitParseError(new MissingTokenError<T>(token));
			return true;
		}
		return false;
	}

	/// <summary>
	/// 跳过词法单元，直到遇到指定类型的词法单元。
	/// </summary>
	/// <param name="targetKinds">目标词法单元类型集合。</param>
	protected void ConsumeTokens(IReadOnlySet<T> targetKinds)
	{
		if (Peek().IsEndOfFile || targetKinds.Contains(Peek().Kind))
		{
			return;
		}
		while (true)
		{
			SkippedTokens.Add(Read());
			Token<T> next = Peek();
			if (next.IsEndOfFile || targetKinds.Contains(next.Kind))
			{
				break;
			}
		}
	}

	/// <summary>
	/// 不断规约状态，直到能够接受当前词法单元。
	/// </summary>
	protected void SyncState()
	{
		parser.SyncState();
	}

	#endregion // 错误恢复

	/// <summary>
	/// 返回指定状态移入指定输入后的状态，但不会改变当前解析状态。
	/// </summary>
	/// <param name="state">要检查的状态。</param>
	/// <param name="kind">要移入的输入。</param>
	/// <returns>移入指定输入后的状态。</returns>
	protected int GetNextState(int state, T kind)
	{
		int top = 0;
		while (true)
		{
			ParserAction action = data.GetAction(state, kind);
			switch (action.Type)
			{
				case ParserActionType.Shift:
					return action.Index;
				case ParserActionType.Reduce:
					ProductionData<T> production = data.Productions[action.Index];
					top += production.BodySize;
					state = data.Goto(stateStack[top], production.Head);
					top--;
					break;
				default:
					// 一般不选择会导致完成识别的终结符。
					return -1;
			}
		}
	}

	#region 读取 Token

	/// <summary>
	/// 读取下一个词法单元，并提升输入流的位置。
	/// </summary>
	/// <returns>下一个词法单元。</returns>
	internal protected Token<T> Read()
	{
		if (tokenCache.Count > 0)
		{
			return tokenCache.Dequeue();
		}
		else
		{
			return NextToken();
		}
	}

	/// <summary>
	/// 读取后续词法单元，但不会提升输入流的位置。
	/// </summary>
	/// <param name="offset">读取的词法单元偏移。</param>
	/// <returns>要读取的词法单元。</returns>
	internal protected Token<T> Peek(int offset = 0)
	{
		if (offset < 0)
		{
			offset = 0;
		}
		while (tokenCache.Count <= offset)
		{
			tokenCache.Enqueue(NextToken());
		}
		return tokenCache[offset];
	}

	/// <summary>
	/// 读取下一个词法单元，并提升输入流的位置，不使用缓存。
	/// </summary>
	/// <returns>下一个词法单元。</returns>
	private Token<T> NextToken()
	{
		if (parser.Status == ParseStatus.Cancelled)
		{
			return Token<T>.GetEndOfFile(node.Span.End);
		}
		while (true)
		{
			Token<T> token = tokenizer.Read();
			if (token.IsEndOfFile || FilterTokens(token))
			{
				return token;
			}
			else
			{
				hiddenTokens.Add(token);
			}
		}
	}

	/// <summary>
	/// 获取被隐藏掉的词法单元集合。
	/// </summary>
	protected TokenCollection<T> HiddenTokens => hiddenTokens;

	/// <summary>
	/// 过滤指定的词法单元。
	/// </summary>
	/// <param name="token">要过滤的词法单元。</param>
	/// <returns>如果需要保留当前词法单元，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
	protected virtual bool FilterTokens(Token<T> token)
	{
		return true;
	}

	#endregion // 读取 Token

	#region ReadOnlyListBase<Token<T>> 成员

	/// <summary>
	/// 获取当前集合包含的元素数。
	/// </summary>
	/// <value>当前集合中包含的元素数。</value>
	public override int Count => nodes.Count;

	/// <summary>
	/// 返回指定索引处的元素。
	/// </summary>
	/// <param name="index">要返回元素的从零开始的索引。</param>
	/// <returns>位于指定索引处的元素。</returns>
	protected override ParserNode<T> GetItemAt(int index)
	{
		return nodes[index];
	}

	/// <summary>
	/// 确定当前列表中指定对象的索引。
	/// </summary>
	/// <param name="item">要在当前列表中定位的对象。</param>
	/// <returns>如果在当前列表中找到 <paramref name="item"/>，则为该对象的索引；否则为 <c>-1</c>。</returns>
	public override int IndexOf(ParserNode<T> item)
	{
		return nodes.IndexOf(item);
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<ParserNode<T>> GetEnumerator()
	{
		return nodes.GetEnumerator();
	}

	#endregion // ReadOnlyListBase<Token<T>> 成员

}
