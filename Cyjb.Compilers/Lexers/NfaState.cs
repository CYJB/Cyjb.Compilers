using System.Collections.Generic;
using System.Diagnostics;
using Cyjb.Compilers.RegularExpressions;

namespace Cyjb.Compilers.Lexers
{
	/// <summary>
	/// 表示不确定有穷自动机（NFA）的状态。
	/// </summary>
	[DebuggerTypeProxy(typeof(NfaState.DebugView))]
	internal sealed class NfaState
	{
		/// <summary>
		/// 字符类的转移对应的字符类列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private HashSet<int> charClassTransition;
		/// <summary>
		/// ϵ 转移的集合。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<NfaState> epsilonTransitions = new List<NfaState>();
		/// <summary>
		/// 初始化 <see cref="NfaState"/> 类的新实例。
		/// </summary>
		/// <param name="nfa">包含状态的 NFA。</param>
		/// <param name="index">状态的索引。</param>
		public NfaState(Nfa nfa, int index)
		{
			Nfa = nfa;
			Index = index;
			SymbolIndex = Constants.None;
			StateType = NfaStateType.Normal;
		}
		/// <summary>
		/// 获取包含当前状态的 NFA。
		/// </summary>
		public Nfa Nfa { get; private set; }
		/// <summary>
		/// 获取当前状态的索引。
		/// </summary>
		public int Index { get; private set; }
		/// <summary>
		/// 获取或设置当前状态的符号索引。
		/// </summary>
		public int SymbolIndex { get; set; }
		/// <summary>
		/// 获取或设置当前状态的类型。
		/// </summary>
		public NfaStateType StateType { get; set; }
		/// <summary>
		/// 获取字符类的转移对应的字符类列表。
		/// </summary>
		public ISet<int> CharClassTransition
		{
			get { return charClassTransition; }
		}
		/// <summary>
		/// 获取字符类转移的目标状态。
		/// </summary>
		public NfaState CharClassTarget { get; private set; }
		/// <summary>
		/// 获取 ϵ 转移的集合。
		/// </summary>
		public IList<NfaState> EpsilonTransitions
		{
			get { return epsilonTransitions; }
		}
		/// <summary>
		/// 添加一个到特定状态的转移。
		/// </summary>
		/// <param name="state">要转移到的状态。</param>
		/// <param name="ch">转移的字符。</param>
		public void Add(NfaState state, char ch)
		{
			charClassTransition = Nfa.GetCharClass(ch);
			CharClassTarget = state;
		}
		/// <summary>
		/// 添加一个到特定状态的转移。
		/// </summary>
		/// <param name="state">要转移到的状态。</param>
		/// <param name="charClass">转移的字符类。</param>
		public void Add(NfaState state, string charClass)
		{
			if (!RegexCharClass.IsEmpty(charClass))
			{
				charClassTransition = Nfa.GetCharClass(charClass);
				CharClassTarget = state;
			}
		}
		/// <summary>
		/// 添加一个到特定状态的 ϵ 转移。
		/// </summary>
		/// <param name="state">要转移到的状态。</param>
		public void Add(NfaState state)
		{
			EpsilonTransitions.Add(state);
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			if (SymbolIndex == Constants.None)
			{
				return string.Concat("State #", Index);
			}
			else
			{
				return string.Concat("State #", Index, " [", SymbolIndex, "]");
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
			private readonly NfaState source;
			/// <summary>
			/// 使用指定的源状态初始化 <see cref="NfaState.DebugView"/> 类的实例。
			/// </summary>
			/// <param name="sourceCollection">使用调试视图的源状态。</param>
			public DebugView(NfaState sourceCollection)
			{
				this.source = sourceCollection;
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
					string[] items = null;
					int cnt = this.source.epsilonTransitions.Count;
					if (this.source.CharClassTarget != null)
					{
						items = new string[cnt + 1];
						items[cnt] = string.Concat(string.Join(",", this.source.charClassTransition),
							" -> ", this.source.CharClassTarget);
					}
					else
					{
						items = new string[cnt];
					}
					for (int i = 0; i < cnt; i++)
					{
						items[i] = string.Concat("ϵ -> ", this.source.epsilonTransitions[i]);
					}
					return items;
				}
			}
		}

		#endregion // 调试视图

	}
}
