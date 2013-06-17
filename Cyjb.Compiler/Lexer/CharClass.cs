using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Cyjb.Collections;
using Cyjb.Compiler.RegularExpressions;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示有穷自动机中使用的字符类。
	/// </summary>
	[DebuggerTypeProxy(typeof(CharClass.DebugView))]
	internal sealed class CharClass
	{
		/// <summary>
		/// 字符类列表。
		/// </summary>
		private List<CharSet> charClassList;
		/// <summary>
		/// 已创建的可能需要被分割的字符类集合。
		/// </summary>
		private List<List<HashSet<int>>> charClassRecord = new List<List<HashSet<int>>>() { 
			new List<HashSet<int>>()
		};
		/// <summary>
		/// 初始化 <see cref="Cyjb.Compiler.Lexer.CharClass"/> 类的新实例。
		/// </summary>
		public CharClass()
		{
			CharSet defaultSet = new CharSet();
			for (int i = 0; i <= char.MaxValue; i++)
			{
				defaultSet.Add((char)i);
			}
			charClassList = new List<CharSet>();
			charClassList.Add(defaultSet);
		}
		/// <summary>
		/// 获取字符类的个数。
		/// </summary>
		public int Count { get { return charClassList.Count; } }
		/// <summary>
		/// 获取字符类的映射数据。
		/// </summary>
		/// <returns>字符类的映射数据。</returns>
		public int[] GetCharClassMap()
		{
			int[] arr = new int[char.MaxValue + 1];
			int cnt = charClassList.Count;
			for (int i = 0; i < cnt; i++)
			{
				foreach (char ch in charClassList[i])
				{
					arr[ch] = i;
				}
			}
			return arr;
		}
		/// <summary>
		/// 将给定的字符类合并，并返回更新的字符类的映射表。
		/// </summary>
		/// <param name="group">字符类的分组信息。</param>
		public Dictionary<int, int> MergeCharClass(IEnumerable<IEnumerable<int>> group)
		{
			Dictionary<int, int> map = new Dictionary<int, int>();
			List<CharSet> newList = new List<CharSet>();
			foreach (IEnumerable<int> charClasses in group)
			{
				int idx = -1;
				CharSet set = null;
				foreach (int cc in charClasses)
				{
					if (idx == -1)
					{
						set = this.charClassList[cc];
						idx = newList.Count;
						map[cc] = idx;
						newList.Add(set);
					}
					else
					{
						map[cc] = idx;
						set.UnionWith(this.charClassList[cc]);
					}
				}
			}
			this.charClassList = newList;
			return map;
		}
		/// <summary>
		/// 返回指定的字符类对应的字符类索引。
		/// </summary>
		/// <param name="charClass">要获取字符类索引的字符类。</param>
		/// <returns>字符类对应的字符类索引。</returns>
		public HashSet<int> GetCharClass(string charClass)
		{
			int cnt = charClassList.Count;
			HashSet<int> result = new HashSet<int>();
			CharSet set = GetCharClassSet(charClass);
			if (set.Count == 0)
			{
				// 不包含任何字符类。
				return result;
			}
			CharSet setClone = new CharSet(set);
			for (int i = 0; i < cnt && set.Count > 0; i++)
			{
				CharSet cc = charClassList[i];
				set.ExceptWith(cc);
				if (set.Count == setClone.Count)
				{
					// 当前字符类与 set 没有重叠。
					continue;
				}
				// 得到当前字符类与 set 重叠的部分。
				setClone.ExceptWith(set);
				if (setClone.Count == cc.Count)
				{
					// 完全被当前字符类包含，直接添加。
					result.Add(i);
					if (cc.Count > 1)
					{
						// 记录新字符类，以备之后修改。
						charClassRecord[i].Add(result);
					}
				}
				else
				{
					// 从当前的字符类中剔除被分割的部分。
					cc.ExceptWith(setClone);
					// 更新字符类。
					int newCC = charClassList.Count;
					result.Add(newCC);
					charClassList.Add(setClone);
					List<HashSet<int>> ccRecord = charClassRecord[i];
					int ccrCnt = ccRecord.Count;
					// 更新旧的字符类集合。
					for (int j = 0; j < ccrCnt; j++)
					{
						ccRecord[j].Add(newCC);
					}
					// 添加新的字符类集合。
					List<HashSet<int>> newRecord = null;
					if (setClone.Count == 1)
					{
						charClassRecord.Add(null);
					}
					else
					{
						newRecord = new List<HashSet<int>>(ccRecord);
						newRecord.Add(result);
						charClassRecord.Add(newRecord);
					}
				}
				// 重新复制 set。
				setClone = new CharSet(set);
			}
			return result;
		}
		/// <summary>
		/// 返回指定的字符对应的字符类索引。
		/// </summary>
		/// <param name="ch">要获取字符类索引的字符。</param>
		/// <returns>字符对应的字符类索引。</returns>
		public HashSet<int> GetCharClass(char ch)
		{
			int cnt = charClassList.Count;
			HashSet<int> result = new HashSet<int>();
			for (int i = 0; i < cnt; i++)
			{
				CharSet cc = charClassList[i];
				if (cc.Contains(ch))
				{
					if (cc.Count == 1)
					{
						// 完全包含当前字符类，直接添加。
						result.Add(i);
					}
					else
					{
						// 当前的字符类需要被分割，从当前的字符类中剔除被分割的部分。
						cc.Remove(ch);
						// 添加新字符类。
						int newCC = charClassList.Count;
						result.Add(newCC);
						charClassList.Add(new CharSet() { ch });
						// 这里虽然在 charClassRecord 里添加了新的项，但完全没有必要记录。
						// 因为当前的字符类只包含一个字符，不可能再被分割了。
						charClassRecord.Add(null);
						// 更新旧的字符类。
						List<HashSet<int>> record = charClassRecord[i];
						int rCnt = record.Count;
						for (int j = 0; j < rCnt; j++)
						{
							record[j].Add(newCC);
						}
					}
				}
			}
			return result;
		}
		/// <summary>
		/// 获取字符类中包含的所有字符。
		/// </summary>
		/// <param name="charClass">要获取所有字符的字符类。</param>
		/// <returns>字符类中包含的所有字符。</returns>
		private static CharSet GetCharClassSet(string charClass)
		{
			CharSet set = new CharSet();
			if (RegexCharClass.IsSubtraction(charClass) || RegexCharClass.ContainsCategory(charClass))
			{
				for (int i = 0; i <= char.MaxValue; i++)
				{
					if (RegexCharClass.CharInClass((char)i, charClass))
					{
						set.Add((char)i);
					}
				}
			}
			else
			{
				// 如果不包含差集和 Unicode 字符分类的话，可以更快。
				string ranges = RegexCharClass.GetCharClassRanges(charClass);
				if (RegexCharClass.IsNegated(charClass))
				{
					int s = 0;
					for (int i = 0; i < ranges.Length; i++)
					{
						for (int j = s; j < ranges[i]; j++)
						{
							set.Add((char)j);
						}
						i++;
						s = i < ranges.Length ? ranges[i] : char.MaxValue + 1;
					}
					for (int j = s; j <= char.MaxValue; j++)
					{
						set.Add((char)j);
					}
				}
				else
				{
					for (int i = 0; i < ranges.Length; i++)
					{
						int j = ranges[i++];
						int end = i < ranges.Length ? ranges[i] : char.MaxValue + 1;
						for (; j < end; j++)
						{
							set.Add((char)j);
						}
					}
				}
			}
			return set;
		}

		#region 调试视图

		/// <summary>
		/// 表示字符类的调试视图。
		/// </summary>
		private class DebugView
		{
			/// <summary>
			/// 字符类。
			/// </summary>
			private readonly CharClass charClass;
			/// <summary>
			/// 使用指定的字符类初始化 <see cref="DebugView"/> 类的实例。
			/// </summary>
			/// <param name="cc">使用视图的字符类。</param>
			public DebugView(CharClass cc)
			{
				this.charClass = cc;
			}
			/// <summary>
			/// 获取字符类的列表。
			/// </summary>
			/// <value>包含了所有字符类的数组。</value>
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public string[] Items
			{
				get
				{
					string[] strs = new string[charClass.charClassList.Count];
					StringBuilder text = new StringBuilder(100);
					for (int i = 0; i < strs.Length; i++)
					{
						text.Clear();
						text.Append(i);
						CharSet set = charClass.charClassList[i];
						if (set.Count > 0)
						{
							text.Append(' ');
							using (IEnumerator<char> iter = set.GetEnumerator())
							{
								iter.MoveNext();
								char first = iter.Current;
								char last = first;
								while (iter.MoveNext())
								{
									if (last + 1 != iter.Current)
									{
										text.Append(first.ToPrintableString());
										if (first != last)
										{
											text.Append('-');
											text.Append(last.ToPrintableString());
										}
										first = iter.Current;
									}
									last = iter.Current;
								}
								text.Append(first.ToPrintableString());
								if (first != last)
								{
									text.Append('-');
									text.Append(last.ToPrintableString());
								}
							}
						}
						strs[i] = text.ToString();
					}
					return strs;
				}
			}
		}

		#endregion // 调试视图

	}
}
