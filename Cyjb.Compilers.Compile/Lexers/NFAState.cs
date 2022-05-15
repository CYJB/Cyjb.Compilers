using System.Diagnostics;
using Cyjb.Collections;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示不确定有穷自动机（NFA）的状态。
/// </summary>
[DebuggerTypeProxy(typeof(DebugView))]
public sealed class NFAState
{
	/// <summary>
	/// 包含当前状态的 NFA。
	/// </summary>
	private readonly NFA nfa;
	/// <summary>
	/// 当前状态的索引。
	/// </summary>
	private readonly int index;
	/// <summary>
	/// ϵ 转移的集合。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<NFAState> epsilonTransitions = new();

	/// <summary>
	/// 初始化 <see cref="NFAState"/> 类的新实例。
	/// </summary>
	/// <param name="nfa">包含状态的 NFA。</param>
	/// <param name="index">状态的索引。</param>
	internal NFAState(NFA nfa, int index)
	{
		this.nfa = nfa;
		this.index = index;
		Symbol = null;
		StateType = NFAStateType.Normal;
	}

	/// <summary>
	/// 获取当前状态的索引。
	/// </summary>
	public int Index => index;
	/// <summary>
	/// 获取或设置当前状态关联到的符号。
	/// </summary>
	public int? Symbol { get; set; }
	/// <summary>
	/// 获取或设置当前状态的类型。
	/// </summary>
	public NFAStateType StateType { get; set; }
	/// <summary>
	/// 获取字符类的转移对应的字符类集合。
	/// </summary>
	public CharClassSet? CharClassSet { get; private set; }
	/// <summary>
	/// 获取字符类转移的目标状态。
	/// </summary>
	public NFAState? CharClassTarget { get; private set; }
	/// <summary>
	/// 获取 ϵ 转移的集合。
	/// </summary>
	public IList<NFAState> EpsilonTransitions => epsilonTransitions;

	/// <summary>
	/// 添加一个到特定状态的转移。
	/// </summary>
	/// <param name="state">要转移到的状态。</param>
	/// <param name="ch">转移的字符。</param>
	public void Add(NFAState state, char ch)
	{
		CharClassSet = nfa.CharClasses.GetCharClassSet(ch);
		CharClassTarget = state;
	}

	/// <summary>
	/// 添加一个到特定状态的转移。
	/// </summary>
	/// <param name="state">要转移到的状态。</param>
	/// <param name="chars">转移的字符集合。</param>
	public void Add(NFAState state, CharSet chars)
	{
		if (state != null && chars != null && chars.Count > 0)
		{
			CharClassSet = nfa.CharClasses.GetCharClassSet(chars);
			CharClassTarget = state;
		}
	}

	/// <summary>
	/// 添加一个到特定状态的 ϵ 转移。
	/// </summary>
	/// <param name="state">要转移到的状态。</param>
	public void Add(NFAState state)
	{
		if (state != null)
		{
			EpsilonTransitions.Add(state);
		}
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		if (Symbol == null)
		{
			return $"State #{Index}";
		}
		else
		{
			return $"State #{Index} [{Symbol}]";
		}
	}

	#region 调试视图

	/// <summary>
	/// 调试视图。
	/// </summary>
	private sealed class DebugView
	{
		/// <summary>
		/// 调试视图的源状态。
		/// </summary>
		private readonly NFAState state;
		/// <summary>
		/// 使用指定的源状态初始化 <see cref="DebugView"/> 类的实例。
		/// </summary>
		/// <param name="state">使用调试视图的源状态。</param>
		public DebugView(NFAState state)
		{
			this.state = state;
		}
		/// <summary>
		/// 获取源状态中的所有项。
		/// </summary>
		/// <value>包含了源状态中的所有项的数组。</value>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public string[] Items
		{
			get
			{
				int cnt = state.epsilonTransitions.Count;
				string[] items;
				if (state.CharClassSet != null)
				{
					items = new string[cnt + 1];
					items[cnt] = $"{state.CharClassSet.Chars} -> {state.CharClassTarget}";
				}
				else
				{
					items = new string[cnt];
				}
				for (int i = 0; i < cnt; i++)
				{
					items[i] = $"ϵ -> {state.epsilonTransitions[i]}";
				}
				return items;
			}
		}
	}

	#endregion // 调试视图

}
