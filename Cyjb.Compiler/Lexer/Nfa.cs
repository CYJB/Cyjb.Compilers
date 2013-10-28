using System.Collections.Generic;
using System.Linq;
using Cyjb.Collections;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示不确定有穷自动机（NFA）。
	/// </summary>
	internal sealed class Nfa : ReadOnlyList<NfaState>
	{
		/// <summary>
		/// NFA 使用的字符类。
		/// </summary>
		private CharClass charClass = new CharClass();
		/// <summary>
		/// 获取或设置 NFA 的首状态。
		/// </summary>
		public NfaState HeadState { get; set; }
		/// <summary>
		/// 获取或设置 NFA 的尾状态。
		/// </summary>
		public NfaState TailState { get; set; }
		/// <summary>
		/// 在当前 NFA 中创建一个新状态。
		/// </summary>
		/// <returns>新创建的状态。</returns>
		public NfaState NewState()
		{
			NfaState state = new NfaState(this, base.Items.Count);
			base.Items.Add(state);
			return state;
		}
		/// <summary>
		/// 返回指定的字符类对应的字符类索引。
		/// </summary>
		/// <param name="regCharClass">要获取字符类索引的字符类。</param>
		/// <returns>字符类对应的字符类索引。</returns>
		public HashSet<int> GetCharClass(string regCharClass)
		{
			return charClass.GetCharClass(regCharClass);
		}
		/// <summary>
		/// 返回指定的字符对应的字符类索引。
		/// </summary>
		/// <param name="ch">要获取字符类索引的字符。</param>
		/// <returns>字符对应的字符类索引。</returns>
		public HashSet<int> GetCharClass(char ch)
		{
			return charClass.GetCharClass(ch);
		}
		/// <summary>
		/// 根据当前的 NFA 构造 DFA，采用子集构造法。
		/// </summary>
		/// <param name="headCnt">头节点的个数。</param>
		internal Dfa BuildDfa(int headCnt)
		{
			Dfa dfa = new Dfa(charClass);
			// DFA 和 NFA 的状态映射表，DFA 的一个状态对应 NFA 的一个状态集合。
			Dictionary<DfaState, HashSet<NfaState>> stateMap =
				new Dictionary<DfaState, HashSet<NfaState>>();
			// 由 NFA 状态集合到对应的 DFA 状态的映射表（与上表互逆）。
			Dictionary<HashSet<NfaState>, DfaState> dfaStateMap =
				new Dictionary<HashSet<NfaState>, DfaState>(SetEqualityComparer<NfaState>.Default);
			Stack<DfaState> stack = new Stack<DfaState>();
			// 添加头节点。
			for (int i = 0; i < headCnt; i++)
			{
				DfaState head = dfa.NewState();
				head.SymbolIndex = new int[0];
				HashSet<NfaState> headStates = EpsilonClosure(Enumerable.Repeat(this[i], 1));
				stateMap.Add(head, headStates);
				dfaStateMap.Add(headStates, head);
				stack.Push(head);
			}
			int charClassCnt = charClass.Count;
			while (stack.Count > 0)
			{
				DfaState state = stack.Pop();
				HashSet<NfaState> stateSet = stateMap[state];
				// 遍历字符类。
				for (int i = 0; i < charClassCnt; i++)
				{
					// 对于 NFA 中的每个转移，寻找 Move 集合。
					HashSet<NfaState> set = Move(stateSet, i);
					if (set.Count > 0)
					{
						set = EpsilonClosure(set);
						DfaState newState;
						if (!dfaStateMap.TryGetValue(set, out newState))
						{
							// 添加新状态.
							newState = dfa.NewState();
							stateMap.Add(newState, set);
							dfaStateMap.Add(set, newState);
							stack.Push(newState);
							// 合并符号索引。
							newState.SymbolIndex = set.Where(s => s.SymbolIndex != Constants.None)
								.Select(s =>
								{
									if (s.StateType == NfaStateType.TrailingHead)
									{
										return int.MaxValue - s.SymbolIndex;
									}
									else
									{
										return s.SymbolIndex;
									}
								}).OrderBy(idx => idx).ToArray();
						}
						// 添加 DFA 的转移。
						state[i] = newState;
					}
				}
			}
			return dfa;
		}
		/// <summary>
		/// 返回指定 NFA 状态集合的 ϵ 闭包。 
		/// </summary>
		/// <param name="states">要获取 ϵ 闭包的 NFA 状态集合。</param>
		/// <returns>得到的 ϵ 闭包。</returns>
		private static HashSet<NfaState> EpsilonClosure(IEnumerable<NfaState> states)
		{
			HashSet<NfaState> set = new HashSet<NfaState>();
			Stack<NfaState> stack = new Stack<NfaState>(states);
			while (stack.Count > 0)
			{
				NfaState state = stack.Pop();
				set.Add(state);
				// 这里只需遍历 ϵ 转移。
				int cnt = state.EpsilonTransitions.Count;
				for (int i = 0; i < cnt; i++)
				{
					NfaState target = state.EpsilonTransitions[i];
					if (set.Add(target))
					{
						stack.Push(target);
					}
				}
			}
			return set;
		}
		/// <summary>
		/// 返回指定 NFA 状态集合的字符类转移集合。 
		/// </summary>
		/// <param name="states">要获取字符类转移集合的 NFA 状态集合。</param>
		/// <param name="charClass">转移使用的字符类。</param>
		/// <returns>得到的字符类转移集合。</returns>
		private static HashSet<NfaState> Move(IEnumerable<NfaState> states, int charClass)
		{
			HashSet<NfaState> set = new HashSet<NfaState>();
			foreach (NfaState state in states)
			{
				if (state.CharClassTransition != null && state.CharClassTransition.Contains(charClass))
				{
					set.Add(state.CharClassTarget);
				}
			}
			return set;
		}
	}
}
