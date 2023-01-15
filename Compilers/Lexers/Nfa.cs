using Cyjb.Collections;
using Cyjb.Collections.ObjectModel;
using Cyjb.Compilers.RegularExpressions;
using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示不确定有穷自动机（NFA）。
/// </summary>
public sealed class Nfa : ReadOnlyListBase<NfaState>
{
	/// <summary>
	/// NFA 状态列表。
	/// </summary>
	private readonly List<NfaState> states = new();
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
	public NfaState NewState()
	{
		NfaState state = new(this, states.Count);
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
	public NfaBuildResult BuildRegex(LexRegex regex, int symbol)
	{
		NfaBuildResult result = new();
		var (head, tail) = BuildNFA(regex, false);
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
				tail.StateType = NfaStateType.TrailingHead;
				var (trailingHead, trailingTail) = BuildNFA(trailingExp, true);
				tail.Add(trailingHead);
				trailingTail.StateType = NfaStateType.Trailing;
				trailingTail.Symbol = symbol;
				result.Tail = trailingTail;
				// 检查向前看的长度。
				int? len = trailingExp.Length ?? anchor.InnerExpression.Length;
				if (len == null)
				{
					trailingType = TrailingType.Variable;
				}
				else if (trailingType == TrailingType.None)
				{
					trailingType = TrailingType.Fixed;
				}
			}
		}
		return result;
	}

	/// <summary>
	/// 使用指定正则表达式构造 NFA，并返回构造结果。
	/// </summary>
	/// <param name="regex">使用的正则表达式。</param>
	/// <param name="trailing">当前是否在向前看表达式中。</param>
	private (NfaState head, NfaState tail) BuildNFA(LexRegex regex, bool trailing)
	{
		NfaState head = NewState();
		NfaState tail = NewState();
		if (regex is AlternationExp alternation)
		{
			foreach (LexRegex subRegex in alternation.Expressions)
			{
				var (subHead, subTail) = BuildNFA(subRegex, trailing);
				head.Add(subHead);
				subTail.Add(tail);
			}
		}
		else if (regex is ConcatenationExp concatenation)
		{
			tail = head;
			foreach (LexRegex subRegex in concatenation.Expressions)
			{
				var (subHead, subTail) = BuildNFA(subRegex, trailing);
				tail.Add(subHead);
				tail = subTail;
			}
		}
		else if (regex is CharClassExp charClass)
		{
			CharSet charSet = charClass.CharClass.GetCharSet();
			// 确保在非向前看表达式中不能包含 EOF。
			if (!trailing)
			{
				charSet.Remove(SourceReader.InvalidCharacter);
			}
			head.Add(tail, charSet);
		}
		else if (regex is LiteralExp literal)
		{
			tail = head;
			foreach (char ch in literal.Literal)
			{
				// 确保在非向前看表达式中不能包含 EOF。
				if (!trailing && ch == SourceReader.InvalidCharacter)
				{
					continue;
				}
				NfaState state = NewState();
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
			NfaState lastTail = head;
			NfaState lastHead = head;
			// 如果没有上限，则需要特殊处理。
			int times = quantifier.MaxTimes == int.MaxValue ? quantifier.MinTimes : quantifier.MaxTimes;
			if (times == 0)
			{
				// 至少要构造一次。
				times = 1;
			}
			for (int i = 0; i < times; i++)
			{
				var (subHead, subTail) = BuildNFA(quantifier.InnerExpression, trailing);
				lastTail.Add(subHead);
				if (i >= quantifier.MinTimes)
				{
					// 添加到最终的尾状态的转移。
					lastTail.Add(tail);
				}
				lastTail = subTail;
				lastHead = subHead;
			}
			// 为最后一个节点添加转移。
			lastTail.Add(tail);
			// 无上限的情况。
			if (quantifier.MaxTimes == int.MaxValue)
			{
				// 在尾部添加一个无限循环。
				lastTail.Add(lastHead);
			}
		}
		else if (regex is AnchorExp anchor)
		{
			return BuildNFA(anchor.InnerExpression, trailing);
		}
		else if (regex is EndOfFileExp)
		{
			if (trailing)
			{
				// 向前看表达式中，相当于 Symbol(EOF)。
				head.Add(tail, SourceReader.InvalidCharacter);
			}
			else
			{
				// 非向前看表达式中，相当于一个空转移。
				head.Add(tail);
			}
		}
		return (head, tail);
	}

	#endregion // 添加正则表达式

	#region 构造 DFA

	/// <summary>
	/// 根据当前的 NFA 构造 DFA，采用子集构造法。
	/// </summary>
	/// <param name="headCnt">头节点的个数。</param>
	/// <param name="rejectable">是否用到了 Reject 动作。</param>
	/// <returns>构造得到的最简 DFA。</returns>
	public Dfa BuildDFA(int headCnt, bool rejectable = false)
	{
		Dfa dfa = new(charClasses);
		// DFA 和 NFA 的状态映射表，DFA 的一个状态对应 NFA 的一个状态集合。
		Dictionary<DfaState, HashSet<NfaState>> stateMap = new();
		// 由 NFA 状态集合到对应的 DFA 状态的映射表（与上表互逆）。
		Dictionary<HashSet<NfaState>, DfaState> dfaStateMap = new(SetEqualityComparer<NfaState>.Default);
		Stack<DfaState> stack = new();
		// 添加头节点。
		for (int i = 0; i < headCnt; i++)
		{
			DfaState head = dfa.NewState();
			head.Symbols = Array.Empty<int>();
			HashSet<NfaState> headStates = EpsilonClosure(Enumerable.Repeat(this[i], 1));
			stateMap.Add(head, headStates);
			dfaStateMap.Add(headStates, head);
			stack.Push(head);
		}
		while (stack.Count > 0)
		{
			DfaState state = stack.Pop();
			HashSet<NfaState> stateSet = stateMap[state];
			// 遍历字符类单元。
			foreach (CharClass charClass in charClasses)
			{
				// 对于 NFA 中的每个转移，寻找 Move 集合。
				HashSet<NfaState> set = Move(stateSet, charClass);
				if (set.Count > 0)
				{
					set = EpsilonClosure(set);
					if (!dfaStateMap.TryGetValue(set, out DfaState? newState))
					{
						// 添加新状态.
						newState = dfa.NewState();
						stateMap.Add(newState, set);
						dfaStateMap.Add(set, newState);
						stack.Push(newState);
						// 合并符号索引，确保正常符号的索引排在向前看符号头状态之前。
						newState.Symbols = set.Where(state => state.Symbol != null).Select(state =>
						{
							int value = state.Symbol!.Value;
							if (state.StateType == NfaStateType.TrailingHead)
							{
								// 确保 0 得到的结果是负数。
								value = -value - 1;
							}
							return value;
						}).OrderBy(idx => idx < 0 ? int.MaxValue + idx : idx).ToArray();
						// 未用到 Reject 动作时，只需要保留第一个非向前看符号。
						if (newState.Symbols.Length > 1 && newState.Symbols[0] >= 0 && !rejectable)
						{
							int headIdx = Array.FindIndex(newState.Symbols, (idx) => idx < 0);
							if (headIdx < 0)
							{
								newState.ConflictedSymbols = newState.Symbols[1..];
								newState.Symbols = newState.Symbols[0..1];
							}
							else if (headIdx > 1)
							{
								newState.ConflictedSymbols = newState.Symbols[1..headIdx];
								newState.Symbols = newState.Symbols[0..1].Concat(newState.Symbols[headIdx..]);
							}
						}
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
	private static HashSet<NfaState> EpsilonClosure(IEnumerable<NfaState> states)
	{
		HashSet<NfaState> set = new();
		Stack<NfaState> stack = new(states);
		while (stack.Count > 0)
		{
			NfaState state = stack.Pop();
			set.Add(state);
			// 这里只需遍历 ϵ 转移。
			foreach (NfaState target in state.EpsilonTransitions)
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
	private static HashSet<NfaState> Move(IEnumerable<NfaState> states, CharClass charClassItem)
	{
		HashSet<NfaState> set = new();
		foreach (NfaState state in states)
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
	protected override NfaState GetItemAt(int index)
	{
		return states[index];
	}

	/// <summary>
	/// 确定当前列表中指定对象的索引。
	/// </summary>
	/// <param name="item">要在当前列表中定位的对象。</param>
	/// <returns>如果在当前列表中找到 <paramref name="item"/>，则为该对象的索引；否则为 <c>-1</c>。</returns>
	public override int IndexOf(NfaState item)
	{
		return states.IndexOf(item);
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<NfaState> GetEnumerator()
	{
		return states.GetEnumerator();
	}

	#endregion // ReadOnlyListBase<NfaState> 成员

}
