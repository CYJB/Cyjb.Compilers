using Cyjb.Collections;
using Cyjb.Collections.ObjectModel;
using Cyjb.Compilers.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示不确定有穷自动机（NFA）。
/// </summary>
public sealed class NFA : ReadOnlyListBase<NFAState>
{
	/// <summary>
	/// NFA 状态列表。
	/// </summary>
	private readonly List<NFAState> states = new();
	/// <summary>
	/// NFA 使用的字符类。
	/// </summary>
	private readonly CharClassCollection charClasses = new();
	/// <summary>
	/// 向前看状态的类型。
	/// </summary>
	private TrailingType trailingType = TrailingType.None;

	/// <summary>
	/// 获取当前 NFA 的向前看状态的类型。
	/// </summary>
	/// <value>指示 NFA 中是否使用了向前看状态，以及向前看状态的类型。</value>
	public TrailingType TrailingType => trailingType;

	/// <summary>
	/// 在当前 NFA 中创建一个新状态。
	/// </summary>
	/// <returns>新创建的状态。</returns>
	public NFAState NewState()
	{
		NFAState state = new(this, states.Count);
		states.Add(state);
		return state;
	}

	/// <summary>
	/// 获取 NFA 使用的字符类。
	/// </summary>
	internal CharClassCollection CharClasses => charClasses;

	#region 添加正则表达式

	/// <summary>
	/// 使用指定正则表达式构造 NFA，并返回构造结果。
	/// </summary>
	/// <param name="regex">使用的正则表达式。</param>
	/// <param name="symbol">正则表达式关联到的符号。</param>
	/// <returns>构造结果。</returns>
	public NFABuildResult BuildRegex(LexRegex regex, int symbol)
	{
		NFABuildResult result = new();
		var (head, tail) = BuildNFA(regex);
		tail.Symbol = symbol;
		result.Head = head;
		result.Tail = tail;
		if (regex is AnchorExp anchor)
		{
			if (anchor.BeginningOfLine)
			{
				result.BeginningOfLine = true;
			}
			LexRegex? trailingExp = anchor.TrailingExpression;
			if (trailingExp != null)
			{
				// 设置向前看状态类型。
				result.UseTrailing = true;
				tail.StateType = NFAStateType.TrailingHead;
				var (trailingHead, trailingTail) = BuildNFA(trailingExp);
				tail.Add(trailingHead);
				trailingTail.StateType = NFAStateType.Trailing;
				trailingTail.Symbol = symbol;
				result.Tail = trailingTail;
				// 检查向前看的长度。
				int? len = trailingExp.Length;
				if (len != null)
				{
					result.TrailingLength = -len.Value;
				}
				else
				{
					len = anchor.InnerExpression.Length;
					if (len != null)
					{
						result.TrailingLength = len.Value;
					}
				}
				if (len == null)
				{
					if (trailingType == TrailingType.None)
					{
						trailingType = TrailingType.Fixed;
					}
				}
				else
				{
					trailingType = TrailingType.Variable;
				}
			}
		}
		return result;
	}

	/// <summary>
	/// 使用指定正则表达式构造 NFA，并返回构造结果。
	/// </summary>
	/// <param name="regex">使用的正则表达式。</param>
	private (NFAState head, NFAState tail) BuildNFA(LexRegex regex)
	{
		NFAState head = NewState();
		NFAState tail = NewState();
		if (regex is AlternationExp alternation)
		{
			foreach (LexRegex subRegex in alternation.Expressions)
			{
				var (subHead, subTail) = BuildNFA(subRegex);
				head.Add(subHead);
				subTail.Add(tail);
			}
		}
		else if (regex is ConcatenationExp concatenation)
		{
			tail = head;
			foreach (LexRegex subRegex in concatenation.Expressions)
			{
				var (subHead, subTail) = BuildNFA(subRegex);
				tail.Add(subHead);
				tail = subTail;
			}
		}
		else if (regex is CharClassExp charClass)
		{
			head.Add(tail, charClass.CharClass.GetCharSet());
		}
		else if (regex is LiteralExp literal)
		{
			tail = head;
			foreach (char ch in literal.Literal)
			{
				NFAState state = NewState();
				if (literal.IgnoreCase)
				{
					// 不区分大小写。
					CharSet set = new();
					set.AddIgnoreCase(ch, literal.Culture);
					tail.Add(state, set);
				}
				else
				{
					// 区分大小写。
					tail.Add(state, ch);
				}
				tail = state;
			}
		}
		else if (regex is QuantifierExp quantifier)
		{
			NFAState lastHead = head;
			// 如果没有上限，则需要特殊处理。
			int times = quantifier.MaxTimes == int.MaxValue ? quantifier.MinTimes : quantifier.MaxTimes;
			if (times == 0)
			{
				// 至少要构造一次。
				times = 1;
			}
			for (int i = 0; i < times; i++)
			{
				var (subHead, subTail) = BuildNFA(quantifier.InnerExpression);
				lastHead.Add(subHead);
				if (i >= quantifier.MinTimes)
				{
					// 添加到最终的尾状态的转移。
					lastHead.Add(tail);
				}
				lastHead = subTail;
			}
			// 为最后一个节点添加转移。
			lastHead.Add(tail);
			// 无上限的情况。
			if (quantifier.MaxTimes == int.MaxValue)
			{
				// 在尾部添加一个无限循环。
				tail.Add(head);
			}
		}
		else if (regex is AnchorExp anchor)
		{
			return BuildNFA(anchor.InnerExpression);
		}
		return (head, tail);
	}

	#endregion // 添加正则表达式

	#region 构造 DFA

	/// <summary>
	/// 根据当前的 NFA 构造 DFA，采用子集构造法。
	/// </summary>
	/// <param name="headCnt">头节点的个数。</param>
	public DFA BuildDFA(int headCnt)
	{
		DFA dfa = new(charClasses);
		// DFA 和 NFA 的状态映射表，DFA 的一个状态对应 NFA 的一个状态集合。
		Dictionary<DFAState, HashSet<NFAState>> stateMap = new();
		// 由 NFA 状态集合到对应的 DFA 状态的映射表（与上表互逆）。
		Dictionary<HashSet<NFAState>, DFAState> dfaStateMap = new(SetEqualityComparer<NFAState>.Default);
		Stack<DFAState> stack = new();
		// 添加头节点。
		for (int i = 0; i < headCnt; i++)
		{
			DFAState head = dfa.NewState();
			head.Symbols = Array.Empty<int>();
			HashSet<NFAState> headStates = EpsilonClosure(Enumerable.Repeat(this[i], 1));
			stateMap.Add(head, headStates);
			dfaStateMap.Add(headStates, head);
			stack.Push(head);
		}
		while (stack.Count > 0)
		{
			DFAState state = stack.Pop();
			HashSet<NFAState> stateSet = stateMap[state];
			// 遍历字符类单元。
			foreach (CharClass charClass in charClasses)
			{
				// 对于 NFA 中的每个转移，寻找 Move 集合。
				HashSet<NFAState> set = Move(stateSet, charClass);
				if (set.Count > 0)
				{
					set = EpsilonClosure(set);
					if (!dfaStateMap.TryGetValue(set, out DFAState? newState))
					{
						// 添加新状态.
						newState = dfa.NewState();
						stateMap.Add(newState, set);
						dfaStateMap.Add(set, newState);
						stack.Push(newState);
						// 合并符号索引。
						newState.Symbols = set.Where(state => state.Symbol != null)
							.Select(state =>
							{
								if (state.StateType == NFAStateType.TrailingHead)
								{
									return int.MaxValue - state.Symbol!.Value;
								}
								else
								{
									return state.Symbol!.Value;
								}
							}).OrderBy(idx => idx).ToArray();
					}
					// 添加 DFA 的转移。
					state[charClass] = newState;
				}
			}
		}
		dfa.Minimize(headCnt);
		return dfa;
	}

	/// <summary>
	/// 返回指定 NFA 状态集合的 ϵ 闭包。 
	/// </summary>
	/// <param name="states">要获取 ϵ 闭包的 NFA 状态集合。</param>
	/// <returns>得到的 ϵ 闭包。</returns>
	private static HashSet<NFAState> EpsilonClosure(IEnumerable<NFAState> states)
	{
		HashSet<NFAState> set = new();
		Stack<NFAState> stack = new(states);
		while (stack.Count > 0)
		{
			NFAState state = stack.Pop();
			set.Add(state);
			// 这里只需遍历 ϵ 转移。
			foreach (NFAState target in state.EpsilonTransitions)
			{
				if (set.Add(target))
				{
					stack.Push(target);
				}
			}
		}
		return set;
	}

	/// <summary>
	/// 返回指定 NFA 状态集合的字符类单元转移集合。 
	/// </summary>
	/// <param name="states">要获取字符类转移集合的 NFA 状态集合。</param>
	/// <param name="charClassItem">转移使用的字符类单元。</param>
	/// <returns>得到的字符类转移集合。</returns>
	private static HashSet<NFAState> Move(IEnumerable<NFAState> states, CharClass charClassItem)
	{
		HashSet<NFAState> set = new();
		foreach (NFAState state in states)
		{
			if (state.CharClassSet != null && state.CharClassSet.Contains(charClassItem))
			{
				set.Add(state.CharClassTarget!);
			}
		}
		return set;
	}

	#endregion // 构造 DFA

	#region ReadOnlyListBase<NfaState> 成员

	/// <summary>
	/// 获取当前集合包含的元素数。
	/// </summary>
	/// <value>当前集合中包含的元素数。</value>
	public override int Count => states.Count;

	/// <summary>
	/// 返回指定索引处的元素。
	/// </summary>
	/// <param name="index">要返回元素的从零开始的索引。</param>
	/// <returns>位于指定索引处的元素。</returns>
	protected override NFAState GetItemAt(int index)
	{
		return states[index];
	}

	/// <summary>
	/// 确定当前列表中指定对象的索引。
	/// </summary>
	/// <param name="item">要在当前列表中定位的对象。</param>
	/// <returns>如果在当前列表中找到 <paramref name="item"/>，则为该对象的索引；否则为 <c>-1</c>。</returns>
	public override int IndexOf(NFAState item)
	{
		return states.IndexOf(item);
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<NFAState> GetEnumerator()
	{
		return states.GetEnumerator();
	}

	#endregion // ReadOnlyListBase<NfaState> 成员

}
