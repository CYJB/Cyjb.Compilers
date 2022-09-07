using Cyjb.Collections;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// <see cref="DfaData"/> 的构造器。
/// </summary>
internal class DfaDataBuilder
{
	/// <summary>
	/// DFA 状态列表。
	/// </summary>
	private readonly List<DfaState> states;
	/// <summary>
	/// 状态数据列表。
	/// </summary>
	private readonly List<DfaStateData> stateDataList = new();
	/// <summary>
	/// 下一状态列表。
	/// </summary>
	private readonly List<int> next = new();
	/// <summary>
	/// 状态检查。
	/// </summary>
	private readonly List<int> check = new();
	/// <summary>
	/// 下一状态列表的可用空当列表。
	/// </summary>
	private readonly BitList spaces = new();
	/// <summary>
	/// 下一个可用的 next 空当。
	/// </summary>
	private int nextSpaceIndex = 0;
	/// <summary>
	/// 最后一个被填充的位置。
	/// </summary>
	private int lastFilledIndex = 0;
	/// <summary>
	/// 状态对应的默认状态。
	/// </summary>
	private readonly Dictionary<DfaState, DefaultState> defaults = new();

	/// <summary>
	/// 使用 DFA 状态列表初始化 <see cref="DfaDataBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="states">DFA 状态列表。</param>
	public DfaDataBuilder(List<DfaState> states)
	{
		this.states = states;
	}

	/// <summary>
	/// 将当前 DFA 构造为 <see cref="DfaData"/>。
	/// </summary>
	/// <returns>DFA 数据。</returns>
	public DfaData Build()
	{
		for (int i = 0; i < states.Count; i++)
		{
			DfaState state = states[i];
			DfaState? defaultState = GetDefaultState(i);
			KeyValuePair<int, DfaState>[] transitions = state.GetTransitions(defaultState);
			if (transitions.Length == 0)
			{
				// 无有效转移。
				stateDataList.Add(new DfaStateData(int.MinValue, DfaStateData.InvalidState, state.Symbols));
				continue;
			}
			// 找到合适的 next 空当。
			int minIndex = transitions[0].Key;
			int length = transitions[^1].Key + 1 - minIndex;
			BitList pattern = new(length, false);
			foreach (KeyValuePair<int, DfaState> pair in transitions)
			{
				pattern[pair.Key - minIndex] = true;
			}
			int index = spaces.FindSpace(pattern, nextSpaceIndex);
			// 确保空白记录包含足够的空间
			if (spaces.Count < index + length)
			{
				spaces.Resize(index + length);
			}
			int restCount = index + length - next.Count;
			if (restCount > 0)
			{
				next.AddRange(Enumerable.Repeat(DfaStateData.InvalidState, restCount));
				check.AddRange(Enumerable.Repeat(DfaStateData.InvalidState, restCount));
			}
			index -= minIndex;
			foreach (KeyValuePair<int, DfaState> pair in transitions)
			{
				int idx = pair.Key + index;
				spaces[idx] = true;
				next[idx] = pair.Value.Index;
				check[idx] = i;
			}
			// 更新 nextSpaceIndex 和 lastFilledIndex
			nextSpaceIndex = spaces.IndexOf(false, nextSpaceIndex);
			if (nextSpaceIndex < 0)
			{
				nextSpaceIndex = spaces.Count;
			}
			for (int j = spaces.Count - 1; j >= lastFilledIndex; j--)
			{
				if (spaces[j])
				{
					lastFilledIndex = j;
					break;
				}
			}
			stateDataList.Add(new DfaStateData(index, defaultState?.Index ?? DfaStateData.InvalidState, state.Symbols));
		}
		return new DfaData(stateDataList.ToArray(), next.ToArray(), check.ToArray());
	}

	/// <summary>
	/// 寻找默认状态。
	/// </summary>
	/// <param name="index">要寻找默认状态的状态索引。</param>
	/// <returns>默认状态。</returns>
	private DfaState? GetDefaultState(int index)
	{
		DfaState state = states[index];
		int transCount = state.Count;
		if (transCount < 5)
		{
			// 转移个数过少，不检测默认状态。
			return null;
		}
		int maxCount = 0;
		DfaState? result = null;
		if (defaults.TryGetValue(state, out DefaultState? defaultState))
		{
			maxCount = defaultState.CoverCount;
			result = defaultState.State;
		}
		for (int i = index + 1; i < states.Count; i++)
		{
			DfaState other = states[i];
			int otherTransCount = other.Count;
			if (otherTransCount < 5)
			{
				continue;
			}
			if (state.Count >= other.Count)
			{
				int coverCount = state.CountCoverTransition(other);
				if (coverCount > maxCount)
				{
					maxCount = coverCount;
					result = other;
				}
			}
			else
			{
				// other 的转移数更多，需要先将结果保存下来。
				int coverCount = other.CountCoverTransition(state);
				if (defaults.TryGetValue(other, out DefaultState? otherDefault))
				{
					if (coverCount > otherDefault.CoverCount)
					{
						otherDefault.CoverCount = coverCount;
						otherDefault.State = state;
					}
				}
				else
				{
					defaults[other] = new DefaultState(state, coverCount);
				}
			}
		}
		return result;
	}

	/// <summary>
	/// 默认状态。
	/// </summary>
	private class DefaultState
	{
		/// <summary>
		/// 默认状态。
		/// </summary>
		public DfaState State;
		/// <summary>
		/// 覆盖的转移个数。
		/// </summary>
		public int CoverCount;

		/// <summary>
		/// 使用指定的默认状态和转移个数初始化。
		/// </summary>
		/// <param name="state">默认状态。</param>
		/// <param name="count">转移个数。</param>
		public DefaultState(DfaState state, int count)
		{
			State = state;
			CoverCount = count;
		}
	}
}
