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
	private readonly List<int> stateDataList = new();
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
		stateDataList.Clear();
		int symbolStartIndex = states.Count * 4;
		stateDataList.AddRange(Enumerable.Repeat(0, symbolStartIndex));
		TripleArrayCompress<int> compress = new(DfaStateData.InvalidState);
		for (int i = 0; i < states.Count; i++)
		{
			DfaState state = states[i];
			DfaState? defaultState = GetDefaultState(i);
			KeyValuePair<int, int>[] transitions = state.GetTransitions(defaultState);
			// 这里使用 short.MinValue 作为默认基线，原因是
			// charClass 可能的范围是 [-1, char.MaxValue]，与 short.MinVaue 相加再 * 2
			// 后仍能确保是负数且不会溢出 int 范围。
			int baseIndex = short.MinValue;
			if (transitions.Length > 0)
			{
				// 找到合适的 next 空当。
				baseIndex = compress.AddNode(i, transitions);
			}
			int offset = i * 4;
			stateDataList[offset] = baseIndex;
			stateDataList[offset + DfaStateData.DefaultStateOffset] = defaultState?.Index ?? DfaStateData.InvalidState;
			int symbolLength = state.Symbols.Length;
			if (symbolLength > 0)
			{
				stateDataList[offset + DfaStateData.SymbolsLengthOffset] = symbolLength;
				stateDataList[offset + DfaStateData.SymbolIndexOffset] = AppendSymbols(state.Symbols, symbolStartIndex);
			}
		}
		// 将 Check 和 Next 拼接到同一个数组内，内存的局部性会让性能更高。
		int transLen = compress.Next.Count * 2;
		int[] trans = new int[transLen];
		for (int i = 0, j = 0; i < compress.Next.Count; i++)
		{
			trans[j++] = compress.Check[i];
			trans[j++] = compress.Next[i];
		}
		return new DfaData(stateDataList.ToArray(), trans);
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
		// 至少要求 70% 的覆盖。
		int maxCount = (int)(transCount * 0.7);
		DfaState? result = null;
		if (defaults.TryGetValue(state, out DefaultState? defaultState))
		{
			maxCount = defaultState.CoverCount;
			result = defaultState.State;
		}
		// 总是从之前的状态中寻找默认状态。
		// 此时状态 0 总是没有默认状态，也避免了状态 0 的转移索引小于 0 被误认为不存在的场景。
		for (int i = 0; i < index; i++)
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
	/// 将指定符号列表添加到状态数据列表的末尾。
	/// </summary>
	/// <param name="symbols">要添加的符号列表。</param>
	/// <param name="startIndex">能够添加的起始索引。</param>
	/// <returns>符号列表添加到的索引。</returns>
	private int AppendSymbols(int[] symbols, int startIndex)
	{
		for (; startIndex < stateDataList.Count; startIndex++)
		{
			if (CompareSymbols(symbols, startIndex))
			{
				break;
			}
		}
		stateDataList.AddRange(symbols.Skip(stateDataList.Count - startIndex));
		return startIndex;
	}

	/// <summary>
	/// 比较符号列表与指定索引的数据。
	/// </summary>
	/// <param name="symbols">要比较的符号列表。</param>
	/// <param name="startIndex">要比较的数据的起始索引。</param>
	/// <returns>如果符号列表与数据一致，返回 <c>true</c>；否则返回 <c>false</c>。</returns>
	private bool CompareSymbols(int[] symbols, int startIndex)
	{
		for (int i = 0; i < symbols.Length; i++)
		{
			if (startIndex + i >= stateDataList.Count)
			{
				break;
			}
			if (symbols[i] != stateDataList[startIndex + i])
			{
				return false;
			}
		}
		return true;
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
