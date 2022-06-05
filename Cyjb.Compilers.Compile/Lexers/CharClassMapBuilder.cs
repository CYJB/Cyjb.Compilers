using System.Globalization;
using Cyjb.Collections;
using Cyjb.Compilers.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// <see cref="CharClassMap"/> 的构造器。
/// </summary>
internal class CharClassMapBuilder
{
	/// <summary>
	/// ASCII 范围的长度。
	/// </summary>
	private const int AsciiLength = 0x80;
	/// <summary>
	/// 需要合并的字符范围长度。
	/// </summary>
	private const int MergedRangeLength = 5;
	/// <summary>
	/// Unicode 类别对应的字符集合。
	/// </summary>
	private static readonly Dictionary<UnicodeCategory, CharSet> UnicodeCategories = BuildUnicodeCategories();

	/// <summary>
	/// 构造 Unicode 类别对应的字符集合。
	/// </summary>
	/// <returns>Unicode 类别对应的字符集合。</returns>
	private static Dictionary<UnicodeCategory, CharSet> BuildUnicodeCategories()
	{
		IReadOnlyDictionary<UnicodeCategory, CharSet> map = UnicodeCategoryCharSet.CategoryCharSets;
		Dictionary<UnicodeCategory, CharSet> unicodeCategories = new(map.Count);
		foreach (KeyValuePair<UnicodeCategory, CharSet> pair in map)
		{
			// 只需要 ASCII 之外的部分。
			CharSet set = new(pair.Value);
			set.Remove('\0', '\x7F');
			// 忽略空和只包含一个字符范围的 Unicode 类别
			if (set.Count > 0 || set.Ranges().Count() > 1)
			{
				unicodeCategories[pair.Key] = set;
			}
		}
		return unicodeCategories;
	}

	/// <summary>
	/// 字符类。
	/// </summary>
	private readonly List<int> charClasses;
	/// <summary>
	/// Unicode 类别对应的字符类。
	/// </summary>
	private readonly Dictionary<UnicodeCategory, int> categories = new();

	/// <summary>
	/// 初始化 <see cref="CharClassMapBuilder"/> 类的新实例。
	/// </summary>
	public CharClassMapBuilder()
	{
		// 前 128 个位置表示 ASCII。
		charClasses = new List<int>(AsciiLength);
		charClasses.AddRange(Enumerable.Repeat(-1, AsciiLength));
	}

	/// <summary>
	/// 将指定的字符类列表构造为 <see cref="CharClassMap"/>。
	/// </summary>
	/// <param name="charClasses">字符类列表。</param>
	/// <returns>字符类映射。</returns>
	public CharClassMap Build(IList<CharClass> charClasses)
	{
		List<CharClassItem> items = FilterNonAsciiItems(charClasses);
		CheckUnicodeCategory(items);
		List<CharClassRange> ranges = BuildRanges(items);
		this.charClasses.AddRange(Enumerable.Repeat(0, ranges.Count));
		List<int> resultRange = new();
		for (int i = 0; i < ranges.Count; i++)
		{
			CharClassRange range = ranges[i];
			resultRange.Add((range.Start << 0x10) | range.End);
			if (range.CharClassList == null)
			{
				this.charClasses[AsciiLength + i] = range.CharClass;
			}
			else
			{
				this.charClasses[AsciiLength + 1] = -this.charClasses.Count;
				this.charClasses.AddRange(range.CharClassList);
			}
		}
		Dictionary<UnicodeCategory, int>? categories = this.categories.Count == 0 ? null : this.categories;
		return new CharClassMap(resultRange.ToArray(), this.charClasses.ToArray(), categories);
	}

	/// <summary>
	/// 过滤非 ASCII 范围的字符类，并返回其列表。
	/// </summary>
	/// <param name="charClasses">字符类列表。</param>
	/// <returns>过滤后的字符类列表。</returns>
	private List<CharClassItem> FilterNonAsciiItems(IList<CharClass> charClasses)
	{
		int cnt = charClasses.Count;
		List<CharClassItem> items = new(cnt);
		for (int i = 0; i < cnt; i++)
		{
			CharSet set = new(charClasses[i].Chars);
			foreach (char ch in set)
			{
				if (ch < AsciiLength)
				{
					this.charClasses[ch] = i;
				}
				else
				{
					break;
				}
			}
			set.Remove('\0', '\x7F');
			if (set.Count > 0)
			{
				items.Add(new CharClassItem(i, set));
			}
		}
		return items;
	}

	/// <summary>
	/// 检测 Unicode 类别。
	/// </summary>
	/// <param name="items">字符类列表。</param>
	private void CheckUnicodeCategory(List<CharClassItem> items)
	{
		int cnt = items.Count;
		if (cnt == 0)
		{
			return;
		}
		// 计算所有被用到的字符，并筛选需要检查的字符集合。
		List<int> rangeCount = new(cnt);
		CharSet usedChars = new();
		for (int i = 0; i < cnt; i++)
		{
			CharSet set = items[i].Chars;
			usedChars.UnionWith(set);
			rangeCount.Add(set.Ranges().Count());
		}
		CharSet extraChars = new();
		// 检查每个 Unicode 类别，找到最适合的简化。
		foreach (KeyValuePair<UnicodeCategory, CharSet> pair in UnicodeCategories)
		{
			CharSet categoryChars = pair.Value;
			int maxCharCount = -1;
			int index = -1;
			CharSet extra = new();
			for (int i = 0; i < cnt; i++)
			{
				CharSet set = items[i].Chars;
				if (!set.Overlaps(categoryChars))
				{
					continue;
				}
				// 确保应用 Unicode 类别后能够减少范围个数。
				CharSet usedSet = new(usedChars);
				usedSet.IntersectWith(categoryChars);
				CharSet appliedSet = new(set);
				appliedSet.UnionWith(categoryChars);
				appliedSet.ExceptWith(usedSet);
				if (appliedSet.Ranges().Count() > rangeCount[i])
				{
					continue;
				}
				// 找到简化字符个数最多的字符类。
				CharSet coveredChars = new(set);
				coveredChars.IntersectWith(categoryChars);
				if (coveredChars.Count > maxCharCount)
				{
					index = i;
					maxCharCount = coveredChars.Count;
					extra = usedSet;
					extra.SymmetricExceptWith(categoryChars);
				}
			}
			if (index == -1)
			{
				// 没有找到可以化简的字符类。
				continue;
			}
			// 简化字符类。
			categories[pair.Key] = items[index].Index;
			CharSet targetChars = items[index].Chars;
			targetChars.ExceptWith(categoryChars);
			if (targetChars.Count == 0)
			{
				items.RemoveAt(index);
				rangeCount.RemoveAt(index);
				cnt--;
			}
			extraChars.UnionWith(extra);
		}
		if (extraChars.Count > 0)
		{
			// 添加需要被额外指定为 -1（不存在）的字符
			items.Add(new CharClassItem(-1, extraChars));
		}
	}

	/// <summary>
	/// 构造字符范围列表。
	/// </summary>
	/// <param name="items">字符类列表。</param>
	/// <returns>字符范围列表。</returns>
	private static List<CharClassRange> BuildRanges(List<CharClassItem> items)
	{
		List<CharClassRange> ranges = new();
		int cnt = items.Count;
		for (int i = 0; i < cnt; i++)
		{
			CharSet set = items[i].Chars;
			int index = items[i].Index;
			foreach (var (start, end) in set.Ranges())
			{
				ranges.Add(new CharClassRange(start, end, index));
			}
		}
		ranges.Sort((CharClassRange left, CharClassRange right) => left.Start - right.Start);
		// 合并短范围
		CharClassRange? lastRange = null;
		for (int i = 0; i < ranges.Count; i++)
		{
			CharClassRange range = ranges[i];
			if (range.Length < MergedRangeLength)
			{
				// 只合并较短的范围。
				if (lastRange == null)
				{
					lastRange = range;
				}
				else if (range.Start - lastRange.End <= MergedRangeLength)
				{
					// 合并范围
					lastRange.Merge(range);
					ranges.RemoveAt(i);
					i--;
				}
				else
				{
					lastRange = null;
				}
			}
			else
			{
				lastRange = null;
			}
		}
		return ranges;
	}

	/// <summary>
	/// 字符类项。
	/// </summary>
	private class CharClassItem
	{
		/// <summary>
		/// 字符类的索引。
		/// </summary>
		public int Index;
		/// <summary>
		/// 字符类包含的字符集合。
		/// </summary>
		public CharSet Chars;

		/// <summary>
		/// 使用字符类的索引和包含的字符集合初始化 <see cref="CharClassItem"/> 类的新实例。
		/// </summary>
		/// <param name="index">字符类的索引。</param>
		/// <param name="chars">字符类包含的字符集合。</param>
		public CharClassItem(int index, CharSet chars)
		{
			Index = index;
			Chars = chars;
		}
	}

	/// <summary>
	/// 字符类的范围。
	/// </summary>
	private class CharClassRange
	{
		/// <summary>
		/// 起始字符（包含）。
		/// </summary>
		public char Start;
		/// <summary>
		/// 结束字符（包含）。
		/// </summary>
		public char End;
		/// <summary>
		/// 字符类索引。
		/// </summary>
		public int CharClass;
		/// <summary>
		/// 字符类列表。
		/// </summary>
		public List<int>? CharClassList;
		/// <summary>
		/// 范围的长度。
		/// </summary>
		public int Length => End - Start + 1;

		/// <summary>
		/// 使用指定的字符类范围初始化 <see cref="CharClassRange"/> 类的新实例。
		/// </summary>
		/// <param name="start">起始字符（包含）。</param>
		/// <param name="end">结束字符（包含）。</param>
		/// <param name="charClass">字符类索引。</param>
		public CharClassRange(char start, char end, int charClass)
		{
			Start = start;
			End = end;
			CharClass = charClass;
		}

		/// <summary>
		/// 将指定字符类范围合并到当前范围中。
		/// </summary>
		/// <param name="range">要合并的范围。</param>
		public void Merge(CharClassRange range)
		{
			if (CharClassList == null)
			{
				CharClassList = new();
				CharClassList.AddRange(Enumerable.Repeat(CharClass, Length));
			}
			CharClassList.AddRange(Enumerable.Repeat(range.CharClass, range.Length));
			End = range.End;
		}
	}
}
