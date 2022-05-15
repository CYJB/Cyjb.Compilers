using System.Diagnostics;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示确定有穷自动机（DFA）的状态。
/// </summary>
[DebuggerTypeProxy(typeof(DebugView))]
public sealed class DFAState : ReadOnlyCollectionBase<DFAState?>
{
	/// <summary>
	/// 包含当前状态的 DFA。
	/// </summary>
	private readonly DFA dfa;
	/// <summary>
	/// DFA 状态的转移。
	/// </summary>
	private readonly Dictionary<CharClass, DFAState> transitions = new();

	/// <summary>
	/// 初始化 <see cref="DFAState"/> 类的新实例。
	/// </summary>
	/// <param name="dfa">包含当前状态的 DFA。</param>
	/// <param name="index">状态的索引。</param>
	internal DFAState(DFA dfa, int index)
	{
		this.dfa = dfa;
		Index = index;
		Symbols = Array.Empty<int>();
	}

	/// <summary>
	/// 获取当前状态的索引。
	/// </summary>
	public int Index { get; internal set; }
	/// <summary>
	/// 获取当前状态的符号列表。
	/// </summary>
	public int[] Symbols { get; internal set; }

	/// <summary>
	/// 获取指定字符类转移到的状态。
	/// </summary>
	/// <param name="charClass">要获取转移的字符类。</param>
	/// <returns>转移到的状态。</returns>
	public DFAState? this[CharClass charClass]
	{
		get
		{
			if (transitions.TryGetValue(charClass, out DFAState? state))
			{
				return state;
			}
			return null;
		}
		internal set
		{
			if (value == null)
			{
				transitions.Remove(charClass);
			}
			else
			{
				transitions[charClass] = value;
			}
		}
	}

	/// <summary>
	/// 获取指定字符类索引转移到的状态。
	/// </summary>
	/// <param name="charClass">要获取转移的字符类索引。</param>
	/// <returns>转移到的状态。</returns>
	public DFAState? this[int charClass]
	{
		get
		{
			if (transitions.TryGetValue(dfa.CharClasses[charClass], out DFAState? state))
			{
				return state;
			}
			return null;
		}
	}

	/// <summary>
	/// 更新状态。
	/// </summary>
	/// <param name="map">状态映射表。</param>
	internal void UpdateState(Dictionary<DFAState, DFAState> map)
	{
		Dictionary<CharClass, DFAState> transitionMap = new();
		foreach (var (charClass, state) in transitions)
		{
			if (map.TryGetValue(state, out DFAState? newState))
			{
				transitionMap[charClass] = newState;
			}
		}
		foreach (var (charClass, state) in transitionMap)
		{
			transitions[charClass] = state;
		}
	}

	/// <summary>
	/// 移除指定的字符类。
	/// </summary>
	/// <param name="list">要移除的字符类集合。</param>
	internal void RemoveCharClass(IEnumerable<CharClass> list)
	{
		foreach (CharClass charClass in list)
		{
			transitions.Remove(charClass);
		}
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		if (Symbols.Length == 0)
		{
			return $"State #{Index}";
		}
		else
		{
			return $"State #{Index} [{string.Join(",", Symbols)}]";
		}
	}

	#region ReadOnlyCollectionBase<DFAState?> 成员

	/// <summary>
	/// 获取当前集合包含的元素数。
	/// </summary>
	/// <value>当前集合中包含的元素数。</value>
	public override int Count => transitions.Count;

	/// <summary>
	/// 确定当前集合是否包含指定对象。
	/// </summary>
	/// <param name="item">要在当前集合中定位的对象。</param>
	/// <returns>如果在当前集合中找到 <paramref name="item"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Contains(DFAState? item)
	{
		if (item == null)
		{
			return false;
		}
		return transitions.ContainsValue(item);
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<DFAState?> GetEnumerator()
	{
		return transitions.Values.GetEnumerator();
	}

	#endregion // ReadOnlyCollectionBase<DFAState?> 成员

	#region 调试视图

	/// <summary>
	/// 调试视图。
	/// </summary>
	private sealed class DebugView
	{
		/// <summary>
		/// 调试视图的源状态。
		/// </summary>
		private readonly DFAState state;
		/// <summary>
		/// 使用指定的源状态初始化 <see cref="DebugView"/> 类的实例。
		/// </summary>
		/// <param name="state">使用调试视图的源状态。</param>
		public DebugView(DFAState state)
		{
			this.state = state;
		}
		/// <summary>
		/// 获取源状态中的所有项。
		/// </summary>
		/// <value>包含了源状态中的所有项的数组。</value>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public string[] Items => state.transitions.Select(pair => $"{pair.Key} -> {pair.Value}").ToArray();
	}

	#endregion // 调试视图

}
