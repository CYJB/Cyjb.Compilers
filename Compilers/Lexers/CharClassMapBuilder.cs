using System.Globalization;
using Cyjb.Collections;
using Cyjb.Globalization;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// <see cref="CharClassMap"/> 的构造器。
/// </summary>
internal class CharClassMapBuilder
{
	/// <summary>
	/// 简化字符集合字符串表示用到的 Unicode 类别。
	/// </summary>
	private class UnicodeCategoryInfo
	{
		/// <summary>
		/// 使用指定的 Unicode 类别和相应字符集合初始化 <see cref="UnicodeCategoryInfo"/> 类的新实例。
		/// </summary>
		/// <param name="chars">Unicode 类别对应的字符集合。</param>
		/// <param name="categories">Unicode 类别。</param>
		public UnicodeCategoryInfo(CharSet chars, params UnicodeCategory[] categories)
		{
			Chars = chars;
			Categories = categories;
		}

		/// <summary>
		/// Unicode 类别对应的字符集合。
		/// </summary>
		public CharSet Chars { get; }
		/// <summary>
		/// Unicode 类别。
		/// </summary>
		public UnicodeCategory[] Categories { get; }
	}

	/// <summary>
	/// ASCII 范围的长度。
	/// </summary>
	private const int AsciiLength = 0x80;
	/// <summary>
	/// 需要合并的字符范围长度。
	/// </summary>
	private const int MergedRangeLength = 5;
	/// <summary>
	/// Unicode 类别信息。
	/// </summary>
	private static readonly UnicodeCategoryInfo[] UnicodeCategoryInfos = BuildUnicodeCategories();

	/// <summary>
	/// 构造 Unicode 类别信息。
	/// </summary>
	/// <returns>Unicode 类别信息。</returns>
	private static UnicodeCategoryInfo[] BuildUnicodeCategories()
	{
		UnicodeCategory[] categories = Enum.GetValues<UnicodeCategory>();
		UnicodeCategoryInfo[] infos = new UnicodeCategoryInfo[categories.Length + 6];
		int i = 0;
		for (; i < categories.Length; i++)
		{
			UnicodeCategory category = categories[i];
			// 只需要 ASCII 之外的部分。
			CharSet set = new(category.GetChars());
			set.Remove('\0', '\x7F');
			infos[i] = new UnicodeCategoryInfo(set, category);
		}
		// 定制的特殊 Unicode 类别
		CharSet lull = infos[(int)UnicodeCategory.UppercaseLetter].Chars +
			infos[(int)UnicodeCategory.LowercaseLetter].Chars;
		// UppercaseLetter & LowercaseLetter
		infos[i++] = new UnicodeCategoryInfo(lull,
			UnicodeCategory.UppercaseLetter, UnicodeCategory.LowercaseLetter
			);
		// UppercaseLetter & LowercaseLetter & TitlecaseLetter
		infos[i++] = new UnicodeCategoryInfo(lull + infos[(int)UnicodeCategory.TitlecaseLetter].Chars,
			UnicodeCategory.UppercaseLetter, UnicodeCategory.LowercaseLetter, UnicodeCategory.TitlecaseLetter
			);
		CharSet mn = infos[(int)UnicodeCategory.NonSpacingMark].Chars;
		CharSet mnmc = mn + infos[(int)UnicodeCategory.SpacingCombiningMark].Chars;
		// NonSpacingMark & SpacingCombiningMark
		infos[i++] = new UnicodeCategoryInfo(mnmc,
			UnicodeCategory.NonSpacingMark, UnicodeCategory.SpacingCombiningMark
			);
		// NonSpacingMark & SpacingCombiningMark & EnclosingMark
		infos[i++] = new UnicodeCategoryInfo(mnmc + infos[(int)UnicodeCategory.EnclosingMark].Chars,
			UnicodeCategory.NonSpacingMark, UnicodeCategory.SpacingCombiningMark, UnicodeCategory.EnclosingMark
			);
		// NonSpacingMark & EnclosingMark
		infos[i++] = new UnicodeCategoryInfo(mn + infos[(int)UnicodeCategory.EnclosingMark].Chars,
			UnicodeCategory.NonSpacingMark, UnicodeCategory.EnclosingMark
			);
		// OpenPunctuation & ClosePunctuation
		infos[i++] = new UnicodeCategoryInfo(infos[(int)UnicodeCategory.OpenPunctuation].Chars +
			infos[(int)UnicodeCategory.ClosePunctuation].Chars,
			UnicodeCategory.OpenPunctuation, UnicodeCategory.ClosePunctuation
			);
		// 按字符个数从小到大排序。
		Array.Sort(infos, (left, right) => left.Chars.Count - right.Chars.Count);
		return infos;
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
		List<uint> resultRange = new();
		for (int i = 0; i < ranges.Count; i++)
		{
			CharClassRange range = ranges[i];
			resultRange.Add(((uint)range.Start << 0x10) | range.End);
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
		CharSet fullChars = new();
		foreach (CharClassItem item in items)
		{
			CharSet set = item.Chars;
			fullChars.UnionWith(set);
			rangeCount.Add(set.RangeCount());
		}
		CharSet negateChars = new();
		// 检查每个 Unicode 类别，找到最适合的简化。
		List<UnicodeCategoryInfo> infos = new(UnicodeCategoryInfos);
		List<UnicodeCategoryInfo> nextInfos = new();
		bool changed = true;
		while (changed)
		{
			changed = false;
			foreach (UnicodeCategoryInfo info in infos)
			{
				CharSet catChars = info.Chars;
				CharSet usedCatChars = catChars & fullChars;
				if (usedCatChars.Count == 0)
				{
					continue;
				}
				int maxCharCount = -1;
				int index = -1;
				CharSet curNegateChars = new();
				for (int i = 0; i < cnt; i++)
				{
					CharSet curChars = items[i].Chars;
					// 允许最多尝试 10% 的排除字符。
					if (!curChars.Overlaps(catChars) || 1.1 * curChars.Count < catChars.Count)
					{
						continue;
					}
					// 确保应用 Unicode 类别后能够减少范围和字符个数。
					CharSet appliedChars = new(curChars);
					appliedChars.UnionWith(catChars);
					appliedChars.ExceptWith(usedCatChars);
					if (appliedChars.RangeCount() > rangeCount[i] || appliedChars.Count > curChars.Count)
					{
						continue;
					}
					// 找到简化字符个数最多的字符类。
					CharSet coveredChars = curChars & catChars;
					if (coveredChars.Count > maxCharCount)
					{
						index = i;
						maxCharCount = coveredChars.Count;
						curNegateChars = appliedChars;
					}
				}
				if (index == -1)
				{
					// 没有找到可以化简的字符类，需要在后面重复检测，避免遗漏。
					nextInfos.Add(info);
					continue;
				}
				// 简化字符类。
				int charClassIndex = items[index].Index;
				foreach (UnicodeCategory category in info.Categories)
				{
					categories[category] = charClassIndex;
				}
				CharSet targetChars = items[index].Chars;
				targetChars.ExceptWith(catChars);
				if (targetChars.Count == 0)
				{
					items.RemoveAt(index);
					rangeCount.RemoveAt(index);
					cnt--;
				}
				else
				{
					rangeCount[index] = targetChars.Ranges().Count();
				}
				negateChars.UnionWith(curNegateChars);
				changed = true;
			}
			var temp = infos;
			infos = nextInfos;
			nextInfos = temp;
			nextInfos.Clear();
		}
		negateChars.ExceptWith(fullChars);
		if (negateChars.Count > 0)
		{
			// 添加需要被额外指定为 -1（不存在）的字符
			items.Add(new CharClassItem(-1, negateChars));
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
