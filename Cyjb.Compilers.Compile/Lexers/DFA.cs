using Cyjb.Collections;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示确定有穷自动机（DFA）。
/// </summary>
public sealed class DFA : ReadOnlyListBase<DFAState>
{
	/// <summary>
	/// DFA 状态列表。
	/// </summary>
	private readonly List<DFAState> states = new();
	/// <summary>
	/// DFA 使用的字符类。
	/// </summary>
	private readonly CharClassCollection charClasses;

	/// <summary>
	/// 初始化 <see cref="DFA"/> 类的新实例。
	/// </summary>
	/// <param name="charClasses">DFA 使用的字符类。</param>
	internal DFA(CharClassCollection charClasses)
	{
		this.charClasses = charClasses;
	}

	/// <summary>
	/// 获取 DFA 使用的字符类。
	/// </summary>
	public IReadOnlyList<CharClass> CharClasses => charClasses;

	/// <summary>
	/// 在当前 DFA 中创建一个新状态。
	/// </summary>
	/// <returns>新创建的状态。</returns>
	internal DFAState NewState()
	{
		DFAState state = new(this, states.Count);
		states.Add(state);
		return state;
	}

	#region 化简 DFA

	/// <summary>
	/// 最小化当前 DFA。
	/// </summary>
	/// <param name="headCnt">DFA 中头节点的数目。</param>
	internal void Minimize(int headCnt)
	{
		List<IEnumerable<DFAState>> groups = MapGroup();
		if (groups.Count > 0)
		{
			// 根据分组信息合并等价状态。
			MergeState(headCnt, groups);
			MinimizeCharClass();
		}
	}

	/// <summary>
	/// 将 DFA 的状态划分为等价状态的组。仅包含等价的状态。
	/// </summary>
	/// <returns>划分得到的等价状态的组。</returns>
	private List<IEnumerable<DFAState>> MapGroup()
	{
		int[] groupIdx = new int[Count];
		int[] newGroupIdx = new int[Count];
		List<IEnumerable<DFAState>> groups = new();
		List<IEnumerable<DFAState>> newGroups = new();
		// 首先按照接受状态划分组。
		int groupIndex = 0;
		IEqualityComparer<int[]> cmp = ListEqualityComparer<int>.Default;
		foreach (var group in states.GroupBy(state => state.Symbols, cmp))
		{
			int cnt = FillGroupIdx(group, groupIdx, groupIndex);
			groupIndex++;
			// 只有包含多个元素的分组才需要再细分下去。
			if (cnt > 1)
			{
				groups.Add(group);
			}
		}
		// 按照转移划分组。
		while (true)
		{
			newGroups.Clear();
			// 只包含一个状态的分组的个数。
			int singleCnt = 0;
			int groupCount = groups.Count;
			for (int i = 0; i < groupCount; i++)
			{
				// 分割新分组，使用状态的转移对应的分组信息进行分组。
				foreach (var group in groups[i].GroupBy<DFAState, int[]>(state => MapGroupIndex(state, groupIdx), cmp))
				{
					int cnt = FillGroupIdx(group, newGroupIdx, groupIndex);
					groupIndex++;
					if (cnt == 1)
					{
						singleCnt++;
					}
					else
					{
						newGroups.Add(group);
					}
				}
			}
			if (groups.Count == newGroups.Count + singleCnt)
			{
				return groups;
			}
			else
			{
				int[] tmpGroupIdx = groupIdx;
				groupIdx = newGroupIdx;
				newGroupIdx = tmpGroupIdx;
				List<IEnumerable<DFAState>> tmpGroup = groups;
				groups = newGroups;
				newGroups = tmpGroup;
			}
		}
	}

	/// <summary>
	/// 填充分组索引。
	/// </summary>
	/// <param name="group">DFA 状态的分组。</param>
	/// <param name="groupIdx">分组索引数组。</param>
	/// <param name="groupIndex">分组的索引。</param>
	/// <returns>分组中包含的状态数。</returns>
	private static int FillGroupIdx(IEnumerable<DFAState> group, int[] groupIdx, int groupIndex)
	{
		int cnt = 0;
		foreach (DFAState state in group)
		{
			groupIdx[state.Index] = groupIndex;
			cnt++;
		}
		return cnt;
	}

	/// <summary>
	/// 返回指定 DFA 状态的转移对应的组。
	/// </summary>
	/// <param name="state">要获取组的 DFA 状态</param>
	/// <param name="groupIdx">分组信息。</param>
	/// <returns>指定 DFA 状态的转移对应的组。</returns>
	private int[] MapGroupIndex(DFAState state, int[] groupIdx)
	{
		int[] idx = new int[charClasses.Count];
		int index = 0;
		foreach (CharClass charClass in charClasses)
		{
			DFAState? nextState = state[charClass];
			if (nextState == null)
			{
				idx[index++] = -1;
			}
			else
			{
				idx[index++] = groupIdx[nextState.Index];
			}
		}
		return idx;
	}

	/// <summary>
	/// 根据分组信息合并等价状态。
	/// </summary>
	/// <param name="headCnt">DFA 中头节点的数目。</param>
	/// <param name="groups">分组信息。</param>
	private void MergeState(int headCnt, List<IEnumerable<DFAState>> groups)
	{
		// 状态映射表。
		Dictionary<DFAState, DFAState> mappedState = new();
		foreach (IEnumerable<DFAState> group in groups)
		{
			DFAState? first = null;
			foreach (DFAState state in group)
			{
				if (first == null)
				{
					first = state;
				}
				else
				{
					// 将其它状态都合并到第一个状态上。
					mappedState[state] = first;
				}
			}
		}
		// 更新状态映射，移除多余状态。
		int cnt = states.Count;
		int idx = 0;
		for (int i = 0; i < cnt; i++)
		{
			DFAState state = states[i];
			// 头节点不能移除。
			if (i >= headCnt && mappedState.ContainsKey(state))
			{
				continue;
			}
			if (idx != i)
			{
				state.Index = idx;
				states[idx] = state;
			}
			idx++;
			// 更新状态映射。
			state.UpdateState(mappedState);
		}
		if (idx < cnt)
		{
			states.RemoveRange(idx, cnt - idx);
		}
	}

	/// <summary>
	/// 使字符类最小化。
	/// </summary>
	private void MinimizeCharClass()
	{
		// 构造转置的 DFA 表格。
		int[][] transitions = new int[charClasses.Count][].Fill(i => new int[states.Count]);
		foreach (DFAState state in states)
		{
			GetTransition(state, transitions);
		}
		// 得到字符类的等价类。
		Dictionary<CharClass, CharClass> mappedCharClass = new();
		var cmp = ListEqualityComparer<int>.Default;
		var groups = charClasses.GroupBy(charClass => transitions[charClass.Index], cmp);
		foreach (IEnumerable<CharClass> group in groups)
		{
			CharClass? first = null;
			foreach (CharClass charClass in group)
			{
				if (first == null)
				{
					first = charClass;
				}
				else
				{
					// 将其它字符类都合并到第一个字符类上。
					mappedCharClass[charClass] = first;
				}
			}
		}
		// 更新字符类。
		charClasses.MergeCharClass(mappedCharClass);
		foreach (DFAState state in states)
		{
			state.RemoveCharClass(mappedCharClass.Keys);
		}
	}

	/// <summary>
	/// 提取指定状态的转移数组。
	/// </summary>
	/// <param name="state">要获取转移的状态。</param>
	/// <param name="transitions">转移的数组。</param>
	private void GetTransition(DFAState state, int[][] transitions)
	{
		int index = state.Index;
		foreach (CharClass charClass in charClasses)
		{
			DFAState? nextState = state[charClass];
			if (nextState == null)
			{
				transitions[charClass.Index][index] = -1;
			}
			else
			{
				transitions[charClass.Index][index] = nextState.Index;
			}
		}
	}

	#endregion // 化简 DFA

	#region ReadOnlyListBase<DfaState> 成员

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
	protected override DFAState GetItemAt(int index)
	{
		return states[index];
	}

	/// <summary>
	/// 确定当前列表中指定对象的索引。
	/// </summary>
	/// <param name="item">要在当前列表中定位的对象。</param>
	/// <returns>如果在当前列表中找到 <paramref name="item"/>，则为该对象的索引；否则为 <c>-1</c>。</returns>
	public override int IndexOf(DFAState item)
	{
		return states.IndexOf(item);
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<DFAState> GetEnumerator()
	{
		return states.GetEnumerator();
	}

	#endregion // ReadOnlyListBase<DfaState> 成员

}
