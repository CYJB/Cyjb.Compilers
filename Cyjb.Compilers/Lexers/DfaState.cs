using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cyjb.Compilers.Lexers
{
	/// <summary>
	/// 表示确定有穷自动机（DFA）的状态。
	/// </summary>
	[DebuggerTypeProxy(typeof(DfaState.DebugView))]
	internal sealed class DfaState : IEnumerable<DfaState>
	{
		/// <summary>
		/// DFA 状态的转移。
		/// </summary>
		private DfaState[] transitions;
		/// <summary>
		/// 初始化 <see cref="DfaState"/> 类的新实例。
		/// </summary>
		/// <param name="dfa">状态所在的 DFA。</param>
		/// <param name="index">状态的索引。</param>
		public DfaState(Dfa dfa, int index)
		{
			this.Dfa = dfa;
			this.Index = index;
			this.transitions = new DfaState[dfa.CharClass.Count];
		}
		/// <summary>
		/// 获取包含当前状态的 DFA。
		/// </summary>
		public Dfa Dfa { get; private set; }
		/// <summary>
		/// 获取或设置当前状态的索引。
		/// </summary>
		public int Index { get; set; }
		/// <summary>
		/// 获取或设置当前状态的符号索引。
		/// </summary>
		public int[] SymbolIndex { get; set; }
		/// <summary>
		/// 获取或设置特定字符类转移到的状态。
		/// </summary>
		public DfaState this[int charClass]
		{
			get { return transitions[charClass]; }
			set { transitions[charClass] = value; }
		}
		/// <summary>
		/// 更新字符类。
		/// </summary>
		/// <param name="map">字符类映射表。</param>
		public void UdpateCharClass(Dictionary<int, int> map)
		{
			DfaState[] temp = new DfaState[this.Dfa.CharClass.Count];
			for (int i = 0; i < transitions.Length; i++)
			{
				temp[map[i]] = transitions[i];
			}
			transitions = temp;
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			string symIdxStr = string.Empty;
			if (SymbolIndex.Length > 0)
			{
				symIdxStr = string.Concat(" [", string.Join(",", SymbolIndex), "]");
			}
			return string.Concat("State #", Index, symIdxStr);
		}

		#region IEnumerable<DfaState> 成员

		/// <summary>
		/// 返回一个循环访问集合的枚举器。
		/// </summary>
		/// <returns>可用于循环访问集合的 <see cref="System.Collections.Generic.IEnumerator&lt;T&gt;"/>。</returns>
		public IEnumerator<DfaState> GetEnumerator()
		{
			return transitions.AsEnumerable<DfaState>().GetEnumerator();
		}

		#endregion // IEnumerable<DfaState> 成员

		#region IEnumerable 成员

		/// <summary>
		/// 返回一个循环访问集合的枚举器。
		/// </summary>
		/// <returns>可用于循环访问集合的 <see cref="System.Collections.IEnumerator"/>。</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return transitions.GetEnumerator();
		}

		#endregion // IEnumerable 成员

		#region 调试视图

		/// <summary>
		/// 调试视图。
		/// </summary>
		private sealed class DebugView
		{
			/// <summary>
			/// 调试视图的源状态。
			/// </summary>
			private readonly DfaState source;
			/// <summary>
			/// 使用指定的源状态初始化 <see cref="DfaState.DebugView"/> 类的实例。
			/// </summary>
			/// <param name="sourceCollection">使用调试视图的源状态。</param>
			public DebugView(DfaState sourceCollection)
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
					string[] items = new string[this.source.transitions.Length];
					for (int i = 0; i < items.Length; i++)
					{
						if (this.source.transitions[i] == null)
						{
							items[i] = "Φ";
						}
						else
						{
							items[i] = this.source.transitions[i].ToString();
						}
					}
					return items;
				}
			}
		}

		#endregion // 调试视图

	}
}
