using System.Globalization;
using Cyjb.Collections;

namespace Cyjb.Compilers.RegularExpressions;

/// <summary>
/// Unicode 类别对应的字符集合。
/// </summary>
internal class UnicodeCategoryCharSet
{
	/// <summary>
	/// Unicode 类别对应的字符集合。
	/// </summary>
	private static readonly Dictionary<UnicodeCategory, CharSet> map = GetCharSetMap();

	/// <summary>
	/// 计算 Unicode 类别对应的字符集合。
	/// </summary>
	/// <returns>Unicode 类别对应的字符集合字典。</returns>
	private static Dictionary<UnicodeCategory, CharSet> GetCharSetMap()
	{
		UnicodeCategory[] categories = Enum.GetValues<UnicodeCategory>();
		Dictionary<UnicodeCategory, CharSet> map = new(categories.Length);
		foreach (UnicodeCategory category in categories)
		{
			map[category] = new CharSet();
		}
		for (char ch = '\0'; ch < char.MaxValue; ch++)
		{
			map[char.GetUnicodeCategory(ch)].Add(ch);
		}
		map[char.GetUnicodeCategory(char.MaxValue)].Add(char.MaxValue);
		return map;
	}

	/// <summary>
	/// 返回指定 Unicode 类别对应的字符集合。
	/// </summary>
	/// <param name="category">Unicode 类别。</param>
	/// <returns>Unicode 类别对应的字符集合。</returns>
	public static CharSet GetCharSet(UnicodeCategory category)
	{
		return map[category];
	}
}
