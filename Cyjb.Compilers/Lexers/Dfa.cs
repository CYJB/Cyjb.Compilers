using System.Collections.Generic;
using System.Linq;
using Cyjb.Collections;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compilers.Lexers
{
	/// <summary>
	/// 表示确定有穷自动机（DFA）。
	/// </summary>
	internal sealed class Dfa : ReadOnlyList<DfaState>
	{
		/// <summary>
		/// 初始化 <see cref="Dfa"/> 类的新实例。
		/// </summary>
		/// <param name="charClass">DFA 使用的字符类。</param>
		public Dfa(CharClass charClass)
		{
			CharClass = charClass;
		}
		/// <summary>
		/// 获取 DFA 使用的字符类。
		/// </summary>
		public CharClass CharClass { get; private set; }
		/// <summary>
		/// 在当前 DFA 中创建一个新状态。
		/// </summary>
		/// <returns>新创建的状态。</returns>
		public DfaState NewState()
		{
			DfaState state = new DfaState(this, base.Items.Count);
			base.Items.Add(state);
			return state;
		}

		#region 化简 DFA

		/// <summary>
		/// 使当前的 DFA 最小化。
		/// </summary>
		/// <param name="headCnt">DFA 中头节点的数目。</param>
		public void Minimize(int headCnt)
		{
			List<IEnumerable<DfaState>> groups = MapGroup();
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
		private List<IEnumerable<DfaState>> MapGroup()
		{
			int[] groupIdx = new int[this.Count];
			int[] newGroupIdx = new int[this.Count];
			List<IEnumerable<DfaState>> groups = new List<IEnumerable<DfaState>>();
			List<IEnumerable<DfaState>> newGroups = new List<IEnumerable<DfaState>>();
			// 首先按照接受状态划分组。
			int groupIndex = 0;
			IEqualityComparer<int[]> iarrCmp = ListEqualityComparer<int>.Default;
			foreach (IGrouping<int[], DfaState> states in base.Items.GroupBy(
				state => state.SymbolIndex, iarrCmp))
			{
				int sCnt = 0;
				foreach (DfaState s in states)
				{
					groupIdx[s.Index] = groupIndex;
					sCnt++;
				}
				groupIndex++;
				// 只有包含多个元素的分组才需要再细分下去。
				if (sCnt > 1)
				{
					groups.Add(states);
				}
			}
			// 按照转移划分组。
			while (true)
			{
				newGroups.Clear();
				// 只包含一个状态的分组的个数。
				int singleCnt = 0;
				int cnt = groups.Count;
				for (int i = 0; i < cnt; i++)
				{
					// 分割新分组，使用状态的转移对应的分组信息进行分组。
					foreach (IGrouping<int[], DfaState> states in groups[i].GroupBy<DfaState, int[]>(
						state => MapGroupIndex(state, groupIdx), iarrCmp))
					{
						int sCnt = 0;
						foreach (DfaState s in states)
						{
							newGroupIdx[s.Index] = groupIndex;
							sCnt++;
						}
						groupIndex++;
						if (sCnt == 1)
						{
							singleCnt++;
						}
						else
						{
							newGroups.Add(states);
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
					List<IEnumerable<DfaState>> tmpGroup = groups;
					groups = newGroups;
					newGroups = tmpGroup;
				}
			}
		}
		/// <summary>
		/// 返回指定 DFA 状态的转移对应的组。
		/// </summary>
		/// <param name="state">要获取组的 DFA 状态</param>
		/// <param name="groupIdx">分组信息。</param>
		/// <returns>指定 DFA 状态的转移对应的组。</returns>
		private int[] MapGroupIndex(DfaState state, int[] groupIdx)
		{
			int[] idx = new int[this.CharClass.Count];
			for (int i = 0; i < idx.Length; i++)
			{
				if (state[i] == null)
				{
					idx[i] = -1;
				}
				else
				{
					idx[i] = groupIdx[state[i].Index];
				}
			}
			return idx;
		}
		/// <summary>
		/// 根据分组信息合并等价状态。
		/// </summary>
		/// <param name="headCnt">DFA 中头节点的数目。</param>
		/// <param name="groups">分组信息。</param>
		private void MergeState(int headCnt, List<IEnumerable<DfaState>> groups)
		{
			// 状态映射表。
			Dictionary<DfaState, DfaState> stateMap = new Dictionary<DfaState, DfaState>();
			int cnt = groups.Count;
			for (int i = 0; i < cnt; i++)
			{
				DfaState first = null;
				foreach (DfaState state in groups[i])
				{
					if (first == null)
					{
						first = state;
					}
					else
					{
						// 将其它状态都合并到第一个状态上。
						stateMap.Add(state, first);
						// 移除多余的状态（头节点不能移除）。
						if (state.Index >= headCnt)
						{
							base.Items[state.Index] = null;
						}
					}
				}
			}
			// 更新状态映射，移除多余状态。
			cnt = this.Count;
			int ccCnt = this.CharClass.Count;
			int idx = 0;
			for (int i = 0; i < cnt; i++)
			{
				DfaState state = base.Items[i];
				if (state == null)
				{
					continue;
				}
				if (idx != i)
				{
					state.Index = idx;
					base.Items[idx] = state;
				}
				idx++;
				// 更新状态映射。
				for (int j = 0; j < ccCnt; j++)
				{
					if (state[j] == null)
					{
						continue;
					}
					DfaState newState;
					if (stateMap.TryGetValue(state[j], out newState))
					{
						state[j] = newState;
					}
				}
			}
			// 移除最后的多余状态。
			while (cnt-- > idx)
			{
				base.Items.RemoveAt(idx);
			}
		}
		/// <summary>
		/// 使字符类最小化。
		/// </summary>
		private void MinimizeCharClass()
		{
			// 构造转置的 DFA 表格。
			int cnt = this.Count;
			int ccCnt = this.CharClass.Count;
			int[][] charClassMap = new int[ccCnt][];
			for (int i = 0; i < charClassMap.Length; i++)
			{
				charClassMap[i] = new int[cnt];
			}
			for (int i = 0; i < cnt; i++)
			{
				DfaState state = base.Items[i];
				for (int j = 0; j < ccCnt; j++)
				{
					if (state[j] == null)
					{
						charClassMap[j][i] = -1;
					}
					else
					{
						charClassMap[j][i] = state[j].Index;
					}
				}
			}
			// 得到字符类的等价类。
			IEnumerable<IEnumerable<int>> charClassGroup = 0.To(ccCnt - 1).GroupBy(i => charClassMap[i],
				ListEqualityComparer<int>.Default);
			if (charClassGroup.Count() < ccCnt)
			{
				// 需要更新字符类。
				Dictionary<int, int> charClassUpdate = CharClass.MergeCharClass(charClassGroup);
				for (int i = 0; i < cnt; i++)
				{
					base.Items[i].UdpateCharClass(charClassUpdate);
				}
			}
		}

		#endregion // 化简 DFA

	}
}
