using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Cyjb.Collections;

namespace Cyjb.Compilers.RegularExpressions;

/// <summary>
/// 表示正则表达式的字符类。
/// </summary>
[Serializable]
public sealed class RegexCharClass : IEquatable<RegexCharClass>
{

	#region 常量定义

	/// <summary>
	/// 预定义的 Unicode 类别映射。
	/// </summary>
	private static readonly Dictionary<string, string> definedCategories = new(37)
	{
		{ "Lu", "\x00" },
		{ "Ll", "\x01" },
		{ "Lt", "\x02" },
		{ "Lm", "\x03" },
		{ "Lo", "\x04" },
		{ "L", "\x00\x01\x02\x03\x04" },
		{ "Mn", "\x05" },
		{ "Mc", "\x06" },
		{ "Me", "\x07" },
		{ "M", "\x05\x06\x07" },
		{ "Nd", "\x08" },
		{ "Nl", "\x09" },
		{ "No", "\x0A" },
		{ "N", "\x08\x09\x0A" },
		{ "Zs", "\x0B" },
		{ "Zl", "\x0C" },
		{ "Zp", "\x0D" },
		{ "Z", "\x0B\x0C\x0D" },
		{ "Cc", "\x0E" },
		{ "Cf", "\x0F" },
		{ "Cs", "\x10" },
		{ "Co", "\x11" },
		{ "Cn", "\x1D" },
		{ "C", "\x0E\x0F\x10\x11\x1D" },
		{ "Pc", "\x12" },
		{ "Pd", "\x13" },
		{ "Ps", "\x14" },
		{ "Pe", "\x15" },
		{ "Pi", "\x16" },
		{ "Pf", "\x17" },
		{ "Po", "\x18" },
		{ "P", "\x12\x13\x14\x15\x16\x17\x18" },
		{ "Sm", "\x19" },
		{ "Sc", "\x1A" },
		{ "Sk", "\x1B" },
		{ "So", "\x1C" },
		{ "S", "\x19\x1A\x1B\x1C" },
	};
	/// <summary>
	/// 预定义的 Unicode 块映射。
	/// </summary>
	private static readonly Dictionary<string, string> definedBlocks = new(108)
	{
		{ "IsAlphabeticPresentationForms", "\uFB00\uFB4F" },
		{ "IsArabic", "\u0600\u06FF" },
		{ "IsArabicPresentationForms-A", "\uFB50\uFDFF" },
		{ "IsArabicPresentationForms-B", "\uFE70\uFEFF" },
		{ "IsArmenian", "\u0530\u058F" },
		{ "IsArrows", "\u2190\u21FF" },
		{ "IsBasicLatin", "\u0000\u007F" },
		{ "IsBengali", "\u0980\u09FF" },
		{ "IsBlockElements", "\u2580\u259F" },
		{ "IsBopomofo", "\u3100\u312F" },
		{ "IsBopomofoExtended", "\u31A0\u31BF" },
		{ "IsBoxDrawing", "\u2500\u257F" },
		{ "IsBraillePatterns", "\u2800\u28FF" },
		{ "IsBuhid", "\u1740\u175F" },
		{ "IsCherokee", "\u13A0\u13FF" },
		{ "IsCJKCompatibility", "\u3300\u33FF" },
		{ "IsCJKCompatibilityForms", "\uFE30\uFE4F" },
		{ "IsCJKCompatibilityIdeographs", "\uF900\uFAFF" },
		{ "IsCJKRadicalsSupplement", "\u2E80\u2EFF" },
		{ "IsCJKSymbolsandPunctuation", "\u3000\u303F" },
		{ "IsCJKUnifiedIdeographs", "\u4E00\u9FFF" },
		{ "IsCJKUnifiedIdeographsExtensionA", "\u3400\u4DBF" },
		{ "IsCombiningDiacriticalMarks", "\u0300\u036F" },
		{ "IsCombiningDiacriticalMarksforSymbols", "\u20D0\u20FF" },
		{ "IsCombiningHalfMarks", "\uFE20\uFE2F" },
		{ "IsCombiningMarksforSymbols", "\u20D0\u20FF" },
		{ "IsControlPictures", "\u2400\u243F" },
		{ "IsCurrencySymbols", "\u20A0\u20CF" },
		{ "IsCyrillic", "\u0400\u04FF" },
		{ "IsCyrillicSupplement", "\u0500\u052F" },
		{ "IsDevanagari", "\u0900\u097F" },
		{ "IsDingbats", "\u2700\u27BF" },
		{ "IsEnclosedAlphanumerics", "\u2460\u24FF" },
		{ "IsEnclosedCJKLettersandMonths", "\u3200\u32FF" },
		{ "IsEthiopic", "\u1200\u137F" },
		{ "IsGeneralPunctuation", "\u2000\u206F" },
		{ "IsGeometricShapes", "\u25A0\u25FF" },
		{ "IsGeorgian", "\u10A0\u10FF" },
		{ "IsGreek", "\u0370\u03FF" },
		{ "IsGreekandCoptic", "\u0370\u03FF" },
		{ "IsGreekExtended", "\u1F00\u1FFF" },
		{ "IsGujarati", "\u0A80\u0AFF" },
		{ "IsGurmukhi", "\u0A00\u0A7F" },
		{ "IsHalfwidthandFullwidthForms", "\uFF00\uFFEF" },
		{ "IsHangulCompatibilityJamo", "\u3130\u318F" },
		{ "IsHangulJamo", "\u1100\u11FF" },
		{ "IsHangulSyllables", "\uAC00\uD7AF" },
		{ "IsHanunoo", "\u1720\u173F" },
		{ "IsHebrew", "\u0590\u05FF" },
		{ "IsHighPrivateUseSurrogates", "\uDB80\uDBFF" },
		{ "IsHighSurrogates", "\uD800\uDB7F" },
		{ "IsHiragana", "\u3040\u309F" },
		{ "IsIdeographicDescriptionCharacters", "\u2FF0\u2FFF" },
		{ "IsIPAExtensions", "\u0250\u02AF" },
		{ "IsKanbun", "\u3190\u319F" },
		{ "IsKangxiRadicals", "\u2F00\u2FDF" },
		{ "IsKannada", "\u0C80\u0CFF" },
		{ "IsKatakana", "\u30A0\u30FF" },
		{ "IsKatakanaPhoneticExtensions", "\u31F0\u31FF" },
		{ "IsKhmer", "\u1780\u17FF" },
		{ "IsKhmerSymbols", "\u19E0\u19FF" },
		{ "IsLao", "\u0E80\u0EFF" },
		{ "IsLatin-1Supplement", "\u0080\u00FF" },
		{ "IsLatinExtended-A", "\u0100\u017F" },
		{ "IsLatinExtended-B", "\u0180\u024F" },
		{ "IsLatinExtendedAdditional", "\u1E00\u1EFF" },
		{ "IsLetterlikeSymbols", "\u2100\u214F" },
		{ "IsLimbu", "\u1900\u194F" },
		{ "IsLowSurrogates", "\uDC00\uDFFF" },
		{ "IsMalayalam", "\u0D00\u0D7F" },
		{ "IsMathematicalOperators", "\u2200\u22FF" },
		{ "IsMiscellaneousMathematicalSymbols-A", "\u27C0\u27EF" },
		{ "IsMiscellaneousMathematicalSymbols-B", "\u2980\u29FF" },
		{ "IsMiscellaneousSymbols", "\u2600\u26FF" },
		{ "IsMiscellaneousSymbolsandArrows", "\u2B00\u2BFF" },
		{ "IsMiscellaneousTechnical", "\u2300\u23FF" },
		{ "IsMongolian", "\u1800\u18AF" },
		{ "IsMyanmar", "\u1000\u109F" },
		{ "IsNumberForms", "\u2150\u218F" },
		{ "IsOgham", "\u1680\u169F" },
		{ "IsOpticalCharacterRecognition", "\u2440\u245F" },
		{ "IsOriya", "\u0B00\u0B7F" },
		{ "IsPhoneticExtensions", "\u1D00\u1D7F" },
		{ "IsPrivateUse", "\uE000\uF8FF" },
		{ "IsPrivateUseArea", "\uE000\uF8FF" },
		{ "IsRunic", "\u16A0\u16FF" },
		{ "IsSinhala", "\u0D80\u0DFF" },
		{ "IsSmallFormVariants", "\uFE50\uFE6F" },
		{ "IsSpacingModifierLetters", "\u02B0\u02FF" },
		{ "IsSpecials", "\uFFF0\uFFFF" },
		{ "IsSuperscriptsandSubscripts", "\u2070\u209F" },
		{ "IsSupplementalArrows-A", "\u27F0\u27FF" },
		{ "IsSupplementalArrows-B", "\u2900\u297F" },
		{ "IsSupplementalMathematicalOperators", "\u2A00\u2AFF" },
		{ "IsSyriac", "\u0700\u074F" },
		{ "IsTagalog", "\u1700\u171F" },
		{ "IsTagbanwa", "\u1760\u177F" },
		{ "IsTaiLe", "\u1950\u197F" },
		{ "IsTamil", "\u0B80\u0BFF" },
		{ "IsTelugu", "\u0C00\u0C7F" },
		{ "IsThaana", "\u0780\u07BF" },
		{ "IsThai", "\u0E00\u0E7F" },
		{ "IsTibetan", "\u0F00\u0FFF" },
		{ "IsUnifiedCanadianAboriginalSyllabics", "\u1400\u167F" },
		{ "IsVariationSelectors", "\uFE00\uFE0F" },
		{ "IsYiRadicals", "\uA490\uA4CF" },
		{ "IsYiSyllables", "\uA000\uA48F" },
		{ "IsYijingHexagramSymbols", "\u4DC0\u4DFF" },
	};
	/// <summary>
	/// 所有 Unicode 通用类别的名称。
	/// </summary>
	private static readonly string[] CategoryNames = new[] {
		"Lu", "Ll", "Lt", "Lm", "Lo", "Mn", "Mc", "Me", "Nd", "Nl", "No", "Zs", "Zl", "Zp", "Cc",
		"Cf", "Cs", "Co", "Pc", "Pd", "Ps", "Pe", "Pi", "Pf", "Po", "Sm", "Sc", "Sk", "So", "Cn",
	};
	/// <summary>
	/// 所有 Unicode 类别。
	/// </summary>
	private static readonly UnicodeCategory[] AllCategories = Enum.GetValues<UnicodeCategory>();

	#endregion // 常量定义

	/// <summary>
	/// 从正则表达式的字符类模式获取 <see cref="RegexCharClass"/> 对象。
	/// </summary>
	/// <param name="pattern">字符类模式。</param>
	/// <param name="option">正则表达式的选项。</param>
	/// <returns><see cref="RegexCharClass"/> 对象。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="pattern"/> 为 <c>null</c>。</exception>
	/// <remarks>支持的正则表达式选项有 <see cref="RegexOptions.IgnoreCase"/>、
	/// <see cref="RegexOptions.Singleline"/>、<see cref="RegexOptions.IgnorePatternWhitespace"/>、
	/// <see cref="RegexOptions.ECMAScript"/> 和 <see cref="RegexOptions.CultureInvariant"/>。</remarks>
	public static RegexCharClass Parse(string pattern, RegexOptions option = RegexOptions.None)
	{
		ArgumentNullException.ThrowIfNull(pattern);
		return RegexParser.ParseCharClass(pattern, option, false);
	}

	/// <summary>
	/// 当前字符类是否表示否定。
	/// </summary>
	private bool negate = false;
	/// <summary>
	/// 字符类中包含的字符范围。
	/// </summary>
	private CharSet ranges = new();
	/// <summary>
	/// 字符类中包含的 Unicode 类别。
	/// </summary>
	private SortedSet<UnicodeCategory> categories = new();
	/// <summary>
	/// 被排除的字符类。
	/// </summary>
	private RegexCharClass? subtractor;

	/// <summary>
	/// 初始化 <see cref="RegexCharClass"/> 类的新实例。
	/// </summary>
	internal RegexCharClass() { }

	/// <summary>
	/// 返回当前字符类是否包含指定字符。
	/// </summary>
	/// <param name="ch">要判断的字符。</param>
	/// <returns>如果字符属于当前字符类，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
	public bool Contains(char ch)
	{
		bool result = ranges.Contains(ch) || CategoryContains(ch);
		if (negate)
		{
			result = !result;
		}
		if (result && subtractor != null)
		{
			result = !subtractor.Contains(ch);
		}
		return result;
	}

	/// <summary>
	/// 返回当前 Unicode 类别是否包含指定字符。
	/// </summary>
	/// <param name="ch">要判断的字符。</param>
	/// <returns>如果字符属于当前 Unicode 类别，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
	private bool CategoryContains(char ch)
	{
		return categories.Contains(char.GetUnicodeCategory(ch));
	}

	/// <summary>
	/// 返回当前字符类对应的字符集合。
	/// </summary>
	/// <returns>当前字符类对应的字符集合。</returns>
	public CharSet GetCharSet()
	{
		CharSet set = new();
		set.UnionWith(ranges);
		foreach (UnicodeCategory category in categories)
		{
			set.UnionWith(UnicodeCategoryCharSet.GetCharSet(category));
		}
		if (negate)
		{
			set.Negated();
		}
		if (subtractor != null)
		{
			set.ExceptWith(subtractor.GetCharSet());
		}
		return set;
	}

	/// <summary>
	/// 设置当前字符类表示否定。
	/// </summary>
	internal void SetNegated()
	{
		negate = true;
	}

	/// <summary>
	/// 添加被排除的字符类。
	/// </summary>
	/// <value>被排除的字符类。</value>
	internal void AddSubtraction(RegexCharClass subtractor)
	{
		this.subtractor = subtractor;
	}

	/// <summary>
	/// 化简当前字符类。
	/// </summary>
	internal void Simplify()
	{
		if (categories.Count > 0)
		{
			if (categories.Count == AllCategories.Length)
			{
				// 包含全部字符类
				categories.Clear();
				ranges.Add('\0', char.MaxValue);
			}
			else
			{
				// 简化字符范围，移除可以连续被 Unicode 类别包含的范围。
				// 未被完全包含的范围就不做简化了，避免出现过于细碎的范围
				CharSet newRange = new();
				foreach (var (start, end) in ranges.Ranges())
				{
					char min;
					for (min = start; min < end; min++)
					{
						if (!CategoryContains(min))
						{
							break;
						}
					}
					if (min < end)
					{
						char max;
						for (max = end; max >= min; max--)
						{
							if (!CategoryContains(max))
							{
								break;
							}
						}
						if (min < max)
						{
							newRange.Add(min, max);
						}
					}
					else if (!CategoryContains(min))
					{
						newRange.Add(min);
					}
				}
				ranges = newRange;
			}
		}
		if (subtractor != null)
		{
			// 尝试与排除的字符类合并
			if (categories.Count == 0 && subtractor.categories.Count == 0)
			{
				// 均不包含 Unicode 类别时，可以直接计算字符范围
				if (negate == subtractor.negate)
				{
					if (negate)
					{
						subtractor.ranges.ExceptWith(ranges);
						ranges = subtractor.ranges;
						negate = false;
					}
					else
					{
						ranges.ExceptWith(subtractor.ranges);
					}
				}
				else
				{
					ranges.UnionWith(subtractor.ranges);
				}
				subtractor = null;
			}
			else if (ranges.Count == 0 && subtractor.ranges.Count == 0)
			{
				// 均不包含字符范围时，可以直接计算 Unicode 类别。
				if (negate == subtractor.negate)
				{
					if (negate)
					{
						subtractor.categories.ExceptWith(categories);
						categories = subtractor.categories;
						negate = false;
					}
					else
					{
						categories.ExceptWith(subtractor.categories);
					}
				}
				else
				{
					categories.UnionWith(subtractor.categories);
				}
				subtractor = null;
			}
		}
	}

	#region 添加字符

	/// <summary>
	/// 添加指定字符。
	/// </summary>
	/// <param name="ch">要添加的字符。</param>
	internal void AddChar(char ch)
	{
		ranges.Add(ch);
	}

	/// <summary>
	/// 添加指定字符。
	/// </summary>
	/// <param name="ch">要添加的字符。</param>
	/// <param name="caseInsensitive">是否不区分大小写。</param>
	/// <param name="culture">大小写转换使用的区域信息。</param>
	internal void AddChar(char ch, bool caseInsensitive, CultureInfo? culture)
	{
		if (caseInsensitive)
		{
			ranges.AddIgnoreCase(ch, culture);
		}
		else
		{
			ranges.Add(ch);
		}
	}

	/// <summary>
	/// 添加特定范围的字符。
	/// </summary>
	/// <param name="first">要添加的范围的起始字符（包含）。</param>
	/// <param name="last">要添加的范围的结束字符（包含）。</param>
	internal void AddRange(char first, char last)
	{
		ranges.Add(first, last);
	}

	/// <summary>
	/// 添加特定范围的字符。
	/// </summary>
	/// <param name="first">要添加的范围的起始字符（包含）。</param>
	/// <param name="last">要添加的范围的结束字符（包含）。</param>
	/// <param name="caseInsensitive">是否不区分大小写。</param>
	/// <param name="culture">大小写转换使用的区域信息。</param>
	internal void AddRange(char first, char last, bool caseInsensitive, CultureInfo? culture)
	{
		if (caseInsensitive)
		{
			ranges.AddIgnoreCase(first, last, culture);
		}
		else
		{
			ranges.Add(first, last);
		}
	}

	#endregion // 添加字符

	#region 添加类别

	/// <summary>
	/// 添加空白字符。
	/// </summary>
	/// <param name="ecma">是否使用 ECMAScript 行为。</param>
	/// <param name="negate">是否对空白字符取否定。</param>
	/// <returns>当前字符类。</returns>
	internal void AddSpace(bool ecma, bool negate)
	{
		if (ecma)
		{
			if (negate)
			{
				ranges.Add('\x00', '\x08');
				ranges.Add('\x0E', '\x1F');
				ranges.Add('\x21', char.MaxValue);
			}
			else
			{
				ranges.Add('\x09', '\x0D');
				ranges.Add('\x20');
			}
		}
		else
		{
			// see https://docs.microsoft.com/zh-cn/dotnet/api/system.char.iswhitespace?view=net-6.0#remarks
			if (negate)
			{
				// 逆向，需要先排除所有 Unicode 类别再恢复部分被额外排除掉的字符范围
				ranges.Add('\x00', '\x08');
				ranges.Add('\x0E', '\x1F');
				ranges.Add('\x7F');
				ranges.Add('\x80', '\x84');
				ranges.Add('\x86', '\x9F');
				AddCategory("\x0B\x0C\x0D\x0E", true);
			}
			else
			{
				ranges.Add('\x09', '\x0D');
				ranges.Add('\x85');
				AddCategory("\x0B\x0C\x0D", false);
			}
		}
	}

	/// <summary>
	/// 添加数字字符。
	/// </summary>
	/// <param name="ecma">是否使用 ECMAScript 行为。</param>
	/// <param name="negate">是否对数字字符取否定。</param>
	internal void AddDigit(bool ecma, bool negate)
	{
		if (ecma)
		{
			if (negate)
			{
				ranges.Add('\x00', '\x2F');
				ranges.Add('\x3A', char.MaxValue);
			}
			else
			{
				ranges.Add('0', '9');
			}
		}
		else
		{
			AddCategory("\x08", negate);
		}
	}

	/// <summary>
	/// 添加单词字符。
	/// </summary>
	/// <param name="ecma">是否使用 ECMAScript 行为。</param>
	/// <param name="negate">是否对单词字符取否定。</param>
	internal void AddWord(bool ecma, bool negate)
	{
		if (ecma)
		{
			if (negate)
			{
				ranges.Add('\x00', '\x2F');
				ranges.Add('\x3A', '\x40');
				ranges.Add('\x5B', '\x5E');
				ranges.Add('\x60');
				ranges.Add('\x7B', '\u012F');
				ranges.Add('\u0131', char.MaxValue);
			}
			else
			{
				ranges.Add('0', '9');
				ranges.Add('A', 'Z');
				ranges.Add('_');
				ranges.Add('a', 'z');
				ranges.Add('\u0130');
			}
		}
		else
		{
			AddCategory("\x00\x01\x02\x03\x04\x05\x08\x12", negate);
		}
	}

	/// <summary>
	/// 根据名称添加通用 Unicode 类别。
	/// </summary>
	/// <param name="categoryName">通用 Unicode 类别的名称。</param>
	/// <param name="negate">是否对通用 Unicode 类别取否定。</param>
	/// <param name="caseInsensitive">是否不区分字母大小写。</param>
	/// <param name="culture">大小写转换使用的区域信息。</param>
	internal void AddCategoryFromName(string categoryName, bool negate, bool caseInsensitive, CultureInfo? culture)
	{
		if (definedCategories.TryGetValue(categoryName, out var value))
		{
			// 忽略大小写时，转换部分字符类别。
			if (caseInsensitive && (categoryName == "Ll" || categoryName == "Lu" || categoryName == "Lt"))
			{
				value = "\x00\x01\x02";
			}
			AddCategory(value, negate);
		}
		else if (definedBlocks.TryGetValue(categoryName, out value))
		{
			if (caseInsensitive)
			{
				if (negate)
				{
					ranges.AddIgnoreCase('\x00', (char)(value[0] - 1), culture);
					ranges.AddIgnoreCase((char)(value[1] + 1), char.MaxValue, culture);
				}
				else
				{
					ranges.AddIgnoreCase(value[0], value[1], culture);
				}
			}
			else
			{
				if (negate)
				{
					ranges.Add('\x00', (char)(value[0] - 1));
					ranges.Add((char)(value[1] + 1), char.MaxValue);
				}
				else
				{
					ranges.Add(value[0], value[1]);
				}
			}
		}
		else
		{
			throw new ArgumentException(Resources.UnrecognizedUnicodeProperty(categoryName));
		}
	}

	/// <summary>
	/// 添加指定的 Unicode 类别。
	/// </summary>
	/// <param name="category">要添加的 Unicode 类别。</param>
	/// <param name="negate">是否是需要排除的类别。</param>
	private void AddCategory(string category, bool negate)
	{
		if (negate)
		{
			// 是要排除的 Unicode 类别。
			HashSet<UnicodeCategory> set = new(AllCategories);
			for (int i = 0; i < category.Length; i++)
			{
				set.Remove((UnicodeCategory)category[i]);
			}
			categories.UnionWith(set);
		}
		else
		{
			// 普通 Unicode 类别，每个类别单独添加。
			for (int i = 0; i < category.Length; i++)
			{
				categories.Add((UnicodeCategory)category[i]);
			}
		}
	}

	#endregion // 添加类别

	#region IEquatable<RegexCharClass> 成员

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public bool Equals(RegexCharClass? other)
	{
		if (other == null)
		{
			return false;
		}
		return ranges.SetEquals(other.ranges) && negate == other.negate && categories.SetEquals(other.categories) && subtractor == other.subtractor;
	}

	/// <summary>
	/// 返回当前对象是否等于另一对象。
	/// </summary>
	/// <param name="obj">要与当前对象进行比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="obj"/>，则为 true；否则为 false。</returns>
	public override bool Equals(object? obj)
	{
		if (obj is RegexCharClass other)
		{
			return Equals(other);
		}
		return false;
	}

	/// <summary>
	/// 返回当前对象的哈希值。
	/// </summary>
	/// <returns>当前对象的哈希值。</returns>
	public override int GetHashCode()
	{
		HashCode hashCode = new();
		hashCode.Add(ranges);
		hashCode.Add(negate);
		hashCode.Add(categories, SetEqualityComparer<UnicodeCategory>.Default);
		hashCode.Add(subtractor);
		return hashCode.ToHashCode();
	}

	/// <summary>
	/// 返回指定的 <see cref="RegexCharClass"/> 是否相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator ==(RegexCharClass? left, RegexCharClass? right)
	{
		if (ReferenceEquals(left, right))
		{
			return true;
		}
		if (left is null)
		{
			return false;
		}
		return left.Equals(right);
	}

	/// <summary>
	/// 返回指定的 <see cref="RegexCharClass"/> 是否不相等。
	/// </summary>
	/// <param name="left">要比较的第一个对象。</param>
	/// <param name="right">要比较的第二个对象。</param>
	/// <returns>如果 <paramref name="left"/> 等于 <paramref name="right"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool operator !=(RegexCharClass? left, RegexCharClass? right)
	{
		if (ReferenceEquals(left, right))
		{
			return false;
		}
		if (left is null)
		{
			return true;
		}
		return !left.Equals(right);
	}

	#endregion // IEquatable<RegexCharClass> 成员

	#region 字符类描述

	/// <summary>
	/// 返回当前字符类的可读描述。
	/// </summary>
	/// <param name="builder">保存字符类的可读描述的可变字符串。</param>
	internal void ToString(StringBuilder builder)
	{
		builder.Append('[');
		if (negate)
		{
			builder.Append('^');
		}
		foreach (var (start, end) in ranges.Ranges())
		{
			builder.Append(CharDescription(start));
			if (start != end)
			{
				// 字符范围。
				if (start + 1 != end)
				{
					builder.Append('-');
				}
				builder.Append(CharDescription(end));
			}
		}
		// 输出时选择更短的 Unicode 类别
		if (categories.Count > AllCategories.Length / 2)
		{
			SortedSet<UnicodeCategory> set = new(categories);
			set.SymmetricExceptWith(AllCategories);
			builder.Append(@"\P{");
			foreach (UnicodeCategory category in set)
			{
				builder.Append(CategoryNames[(int)category]);
				builder.Append(',');
			}
			builder[^1] = '}';
		}
		else
		{
			foreach (UnicodeCategory category in categories)
			{
				builder.Append(@"\p{");
				builder.Append(CategoryNames[(int)category]);
				builder.Append('}');
			}
		}
		// 减去范围。
		if (subtractor != null)
		{
			builder.Append('-');
			subtractor.ToString(builder);
		}
		builder.Append(']');
	}

	/// <summary>
	/// 返回字符的可读描述。
	/// </summary>
	/// <param name="ch">要获取可读描述的字符。</param>
	/// <returns>字符的可读描述。</returns>
	private static string CharDescription(char ch)
	{
		// Regex 转义。
		return ch switch
		{
			']' => @"\]",
			'-' => @"\-",
			'\x1B' => @"\e",
			_ => ch.UnicodeEscape(),
		};
	}

	/// <summary>
	/// 返回当前对象的字符串形式。
	/// </summary>
	/// <returns>当前对象的字符串形式。</returns>
	public override string ToString()
	{
		StringBuilder builder = new();
		ToString(builder);
		return builder.ToString();
	}

	#endregion

}
