using System.Diagnostics;
using System.Text;
using Cyjb.Collections;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示自动机中使用的字符类集合。
/// </summary>
[DebuggerTypeProxy(typeof(DebugView))]
internal sealed class CharClassCollection : ReadOnlyListBase<CharClass>
{
	/// <summary>
	/// 字符类列表。
	/// </summary>
	private readonly List<CharClass> charClasses = new();
	/// <summary>
	/// 字符类集合列表。
	/// </summary>
	private readonly Dictionary<CharSet, CharClassSet> charClassSets = new();

	/// <summary>
	/// 初始化 <see cref="CharClassCollection"/> 类的新实例。
	/// </summary>
	public CharClassCollection()
	{
		charClasses.Add(new CharClass(0, new CharSet() {{ '\0', char.MaxValue }}));
	}

	/// <summary>
	/// 返回指定的字符对应的字符类集合。
	/// </summary>
	/// <param name="ch">要获取字符类集合的字符。</param>
	/// <returns>字符对应的字符类集合。</returns>
	public CharClassSet GetCharClassSet(char ch)
	{
		CharSet set = new() { ch };
		if (charClassSets.TryGetValue(set, out CharClassSet? charClass))
		{
			return charClass;
		}
		int cnt = charClasses.Count;
		for (int i = 0; i < cnt; i++)
		{
			CharClass item = charClasses[i];
			if (item.Chars.Contains(ch))
			{
				charClass = new(set);
				if (item.Chars.Count == 1)
				{
					// 完全匹配
					charClass.Add(item);
					item.Containers.Add(charClass);
				}
				else
				{
					// 部分匹配，分割当前单元。
					CharClass newItem = item.Split(cnt, set);
					charClasses.Add(newItem);
					charClass.Add(newItem);
					newItem.Containers.Add(charClass);
				}
				charClassSets[set] = charClass;
				return charClass;
			}
		}
		throw new InvalidOperationException("Unreachable code.");
	}

	/// <summary>
	/// 返回指定的字符集合对应的字符类集合。
	/// </summary>
	/// <param name="charSet">要获取字符类集合的字符集合。</param>
	/// <returns>字符集合对应的字符类集合。</returns>
	public CharClassSet GetCharClassSet(CharSet charSet)
	{
		if (charSet.Count == 0)
		{
			// 不包含任何字符类。
			return CharClassSet.Empty;
		}
		if (charClassSets.TryGetValue(charSet, out CharClassSet? charClass))
		{
			return charClass;
		}
		int cnt = charClasses.Count;
		int index = cnt;
		charClass = new CharClassSet(charSet);
		CharSet resetChars = new(charSet);
		for (int i = 0; i < cnt && resetChars.Count > 0; i++)
		{
			CharClass item = charClasses[i];
			if (!item.Chars.Overlaps(resetChars))
			{
				continue;
			}
			if (resetChars.IsSupersetOf(item.Chars))
			{
				// 完全包含当前单元，直接添加即可。
				charClass.Add(item);
				item.Containers.Add(charClass);
				resetChars.ExceptWith(item.Chars);
				continue;
			}
			// 部分匹配，分割当前单元。
			CharSet curSet = new(resetChars);
			curSet.IntersectWith(item.Chars);
			resetChars.ExceptWith(item.Chars);
			CharClass newItem = item.Split(index++, curSet);
			charClasses.Add(newItem);
			newItem.Containers.Add(charClass);
			charClass.Add(newItem);
		}
		charClassSets[charSet] = charClass;
		return charClass;
	}

	/// <summary>
	/// 将给定的字符类合并。
	/// </summary>
	/// <param name="map">字符类的映射信息。</param>
	public void MergeCharClass(Dictionary<CharClass, CharClass> map)
	{
		int cnt = charClasses.Count;
		int idx = 0;
		int i = 0;
		if (charClasses[0].Containers.Count == 0)
		{
			// 如果首个字符类（默认字符类）没有被使用，可以直接删掉表示不存在。
			i++;
		}
		for (; i < cnt; i++)
		{
			CharClass charClass = charClasses[i];
			// 合并字符类。
			if (map.TryGetValue(charClass, out CharClass? target))
			{
				target.Chars.UnionWith(charClass.Chars);
				continue;
			}
			if (idx != i)
			{
				charClass.Index = idx;
				charClasses[idx] = charClass;
			}
			idx++;
		}
		if (idx < cnt)
		{
			charClasses.RemoveRange(idx, cnt - idx);
		}
	}

	/// <summary>
	/// 返回字符类的映射表。
	/// </summary>
	/// <returns>字符类的映射表。</returns>
	public CharClassMap GetCharClassMap()
	{
		CharClassMapBuilder builder = new();
		return builder.Build(charClasses);
	}

	#region ReadOnlyListBase<CharClass> 成员

	/// <summary>
	/// 获取当前集合包含的元素数。
	/// </summary>
	/// <value>当前集合中包含的元素数。</value>
	public override int Count => charClasses.Count;

	/// <summary>
	/// 返回指定索引处的元素。
	/// </summary>
	/// <param name="index">要返回元素的从零开始的索引。</param>
	/// <returns>位于指定索引处的元素。</returns>
	protected override CharClass GetItemAt(int index)
	{
		return charClasses[index];
	}

	/// <summary>
	/// 确定当前列表中指定对象的索引。
	/// </summary>
	/// <param name="item">要在当前列表中定位的对象。</param>
	/// <returns>如果在当前列表中找到 <paramref name="item"/>，则为该对象的索引；否则为 <c>-1</c>。</returns>
	public override int IndexOf(CharClass item)
	{
		return charClasses.IndexOf(item);
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<CharClass> GetEnumerator()
	{
		return charClasses.GetEnumerator();
	}

	#endregion // ReadOnlyListBase<CharClass> 成员

	#region 调试视图

	/// <summary>
	/// 表示字符类的调试视图。
	/// </summary>
	private class DebugView
	{
		/// <summary>
		/// 字符类集合。
		/// </summary>
		private readonly CharClassCollection collection;

		/// <summary>
		/// 使用指定的字符类集合初始化 <see cref="DebugView"/> 类的实例。
		/// </summary>
		/// <param name="collection">使用视图的字符类集合。</param>
		public DebugView(CharClassCollection collection)
		{
			this.collection = collection;
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
				string[] strs = new string[collection.charClasses.Count];
				StringBuilder text = new(100);
				for (int i = 0; i < strs.Length; i++)
				{
					text.Append(i);
					text.Append(' ');
					text.Append(collection[i].Chars);
					strs[i] = text.ToString();
					text.Clear();
				}
				return strs;
			}
		}
	}

	#endregion // 调试视图

}
