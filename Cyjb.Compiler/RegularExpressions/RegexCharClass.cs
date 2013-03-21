using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示正则表达式的字符类。
	/// </summary>
	[Serializable]
	public sealed class RegexCharClass
	{

		#region RegexCharClass 类方法调用

		/// <summary>
		/// RegexCharClass 类的类型。
		/// </summary>
		private static readonly Type RccType = typeof(System.Text.RegularExpressions.Regex).Assembly
			.GetType("System.Text.RegularExpressions.RegexCharClass");
		/// <summary>
		/// RegexCharClass 类的 _definedCategories 字段。
		/// </summary>
		private static readonly Lazy<Func<Dictionary<string, string>>> RccGetDefinedCategories =
			RccType.CreateDelegateLazy<Func<Dictionary<string, string>>>("_definedCategories");
		/// <summary>
		/// RegexCharClass 类的 Word 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccGetWord =
			RccType.CreateDelegateLazy<Func<string>>("Word");
		/// <summary>
		/// RegexCharClass 类的 NotWord 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccGetNotWord =
			RccType.CreateDelegateLazy<Func<string>>("NotWord");
		/// <summary>
		/// RegexCharClass 类的 SpaceClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccSpaceClass =
			RccType.CreateDelegateLazy<Func<string>>("SpaceClass");
		/// <summary>
		/// RegexCharClass 类的 NotSpaceClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccNotSpaceClass =
			RccType.CreateDelegateLazy<Func<string>>("NotSpaceClass");
		/// <summary>
		/// RegexCharClass 类的 WordClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccWordClass =
			RccType.CreateDelegateLazy<Func<string>>("WordClass");
		/// <summary>
		/// RegexCharClass 类的 NotWordClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccNotWordClass =
			RccType.CreateDelegateLazy<Func<string>>("NotWordClass");
		/// <summary>
		/// RegexCharClass 类的 DigitClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccDigitClass =
			RccType.CreateDelegateLazy<Func<string>>("DigitClass");
		/// <summary>
		/// RegexCharClass 类的 NotDigitClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccNotDigitClass =
			RccType.CreateDelegateLazy<Func<string>>("NotDigitClass");
		/// <summary>
		/// RegexCharClass 类的 ECMASpaceClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccECMASpaceClass =
			RccType.CreateDelegateLazy<Func<string>>("ECMASpaceClass");
		/// <summary>
		/// RegexCharClass 类的 NotECMASpaceClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccNotECMASpaceClass =
			RccType.CreateDelegateLazy<Func<string>>("NotECMASpaceClass");
		/// <summary>
		/// RegexCharClass 类的 ECMAWordClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccECMAWordClass =
			RccType.CreateDelegateLazy<Func<string>>("ECMAWordClass");
		/// <summary>
		/// RegexCharClass 类的 NotECMAWordClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccNotECMAWordClass =
			RccType.CreateDelegateLazy<Func<string>>("NotECMAWordClass");
		/// <summary>
		/// RegexCharClass 类的 ECMADigitClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccECMADigitClass =
			RccType.CreateDelegateLazy<Func<string>>("ECMADigitClass");
		/// <summary>
		/// RegexCharClass 类的 NotECMADigitClass 字段。
		/// </summary>
		private static readonly Lazy<Func<string>> RccNotECMADigitClass =
			RccType.CreateDelegateLazy<Func<string>>("NotECMADigitClass");
		/// <summary>
		/// RegexCharClass 类的 CharInClass 方法。
		/// </summary>
		private static readonly Lazy<Func<char, string, bool>> RccCharInClass =
			RccType.CreateDelegateLazy<Func<char, string, bool>>("CharInClass");
		/// <summary>
		/// RegexCharClass 类的 IsECMAWordChar 方法。
		/// </summary>
		private static readonly Lazy<Func<char, bool>> RccIsECMAWordChar =
			RccType.CreateDelegateLazy<Func<char, bool>>("IsECMAWordChar");
		/// <summary>
		/// RegexCharClass 类的 IsEmpty 方法。
		/// </summary>
		private static readonly Lazy<Func<string, bool>> RccIsEmpty =
			RccType.CreateDelegateLazy<Func<string, bool>>("IsEmpty");
		/// <summary>
		/// RegexCharClass 类的 IsMergeable 方法。
		/// </summary>
		private static readonly Lazy<Func<string, bool>> RccIsMergeable =
			RccType.CreateDelegateLazy<Func<string, bool>>("IsMergeable");
		/// <summary>
		/// RegexCharClass 类的 IsNegated 方法。
		/// </summary>
		private static readonly Lazy<Func<string, bool>> RccIsNegated =
			RccType.CreateDelegateLazy<Func<string, bool>>("IsNegated");
		/// <summary>
		/// RegexCharClass 类的 IsSingleton 方法。
		/// </summary>
		private static readonly Lazy<Func<string, bool>> RccIsSingleton =
			RccType.CreateDelegateLazy<Func<string, bool>>("IsSingleton");
		/// <summary>
		/// RegexCharClass 类的 IsSingletonInverse 方法。
		/// </summary>
		private static readonly Lazy<Func<string, bool>> RccIsSingletonInverse =
			RccType.CreateDelegateLazy<Func<string, bool>>("IsSingletonInverse");
		/// <summary>
		/// RegexCharClass 类的 IsSubtraction 方法。
		/// </summary>
		private static readonly Lazy<Func<string, bool>> RccIsSubtraction =
			RccType.CreateDelegateLazy<Func<string, bool>>("IsSubtraction");
		/// <summary>
		/// RegexCharClass 类的 IsWordChar 方法。
		/// </summary>
		private static readonly Lazy<Func<char, bool>> RccIsWordChar =
			RccType.CreateDelegateLazy<Func<char, bool>>("IsWordChar");
		/// <summary>
		/// RegexCharClass 类的 SingletonChar 方法。
		/// </summary>
		private static readonly Lazy<Func<string, char>> RccSingletonChar =
			RccType.CreateDelegateLazy<Func<string, char>>("SingletonChar");
		/// <summary>
		/// RegexCharClass 类的 Parse 方法。
		/// </summary>
		private static readonly Lazy<Func<string, object>> RccParse =
			RccType.CreateDelegateLazy<Func<string, object>>("Parse");
		/// <summary>
		/// RegexCharClass 类的构造函数。
		/// </summary>
		private static readonly Lazy<Func<object>> RccConstructor =
			RccType.CreateDelegateLazy<Func<object>>(".ctor");
		/// <summary>
		/// RegexCharClass 实例的获取 _rangelist 字段。
		/// </summary>
		private static readonly Lazy<Func<object, object>> RccGetRangelist =
			RccType.CreateDelegateLazy<Func<object, object>>("_rangelist");
		/// <summary>
		/// RegexCharClass 实例的获取 _categories 字段。
		/// </summary>
		private static readonly Lazy<Func<object, StringBuilder>> RccGetCategories =
			RccType.CreateDelegateLazy<Func<object, StringBuilder>>("_categories");
		/// <summary>
		/// RegexCharClass 实例的获取 _canonical 字段。
		/// </summary>
		private static readonly Lazy<Func<object, bool>> RccGetCanonical =
			RccType.CreateDelegateLazy<Func<object, bool>>("_canonical");
		/// <summary>
		/// RegexCharClass 实例的设置 _canonical 字段。
		/// </summary>
		private static readonly Lazy<Action<object, bool>> RccSetCanonical =
			RccType.CreateDelegateLazy<Action<object, bool>>("_canonical");
		/// <summary>
		/// RegexCharClass 实例的 RangeCount 方法。
		/// </summary>
		private static readonly Lazy<Func<object, int>> RccRangeCount =
			RccType.CreateDelegateLazy<Func<object, int>>("RangeCount");
		/// <summary>
		/// RegexCharClass 实例的 RangeCount 方法。
		/// </summary>
		private static readonly Lazy<Func<object, int, object>> RccGetRangeAt =
			RccType.CreateDelegateLazy<Func<object, int, object>>("GetRangeAt");
		/// <summary>
		/// RegexCharClass 实例的 AddCategoryFromName 方法。
		/// </summary>
		private static readonly Lazy<Action<object, string, bool, bool, string>> RccAddCategoryFromName =
			RccType.CreateDelegateLazy<Action<object, string, bool, bool, string>>("AddCategoryFromName");
		/// <summary>
		/// RegexCharClass 实例的 AddChar 方法。
		/// </summary>
		private static readonly Lazy<Action<object, char>> RccAddChar =
			RccType.CreateDelegateLazy<Action<object, char>>("AddChar");
		/// <summary>
		/// RegexCharClass 实例的 AddDigit 方法。
		/// </summary>
		private static readonly Lazy<Action<object, bool, bool, string>> RccAddDigit =
			RccType.CreateDelegateLazy<Action<object, bool, bool, string>>("AddDigit");
		/// <summary>
		/// RegexCharClass 实例的 AddLowercase 方法。
		/// </summary>
		private static readonly Lazy<Action<object, CultureInfo>> RccAddLowercase =
			RccType.CreateDelegateLazy<Action<object, CultureInfo>>("AddLowercase");
		/// <summary>
		/// RegexCharClass 实例的 AddRange 方法。
		/// </summary>
		private static readonly Lazy<Action<object, char, char>> RccAddRange =
			RccType.CreateDelegateLazy<Action<object, char, char>>("AddRange");
		/// <summary>
		/// RegexCharClass 实例的 AddSpace 方法。
		/// </summary>
		private static readonly Lazy<Action<object, bool, bool>> RccAddSpace =
			RccType.CreateDelegateLazy<Action<object, bool, bool>>("AddSpace");
		/// <summary>
		/// RegexCharClass 实例的 AddSubtraction 方法。
		/// </summary>
		private static readonly Lazy<Action<object, object>> RccAddSubtraction =
			RccType.CreateDelegateLazy<Action<object, object>>("AddSubtraction");
		/// <summary>
		/// RegexCharClass 实例的 AddWord 方法。
		/// </summary>
		private static readonly Lazy<Action<object, bool, bool>> RccAddWord =
			RccType.CreateDelegateLazy<Action<object, bool, bool>>("AddWord");
		/// <summary>
		/// RegexCharClass 实例的 RccToStringClass 方法。
		/// </summary>
		private static readonly Lazy<Func<object, string>> RccToStringClass =
			RccType.CreateDelegateLazy<Func<object, string>>("ToStringClass");
		/// <summary>
		/// RegexCharClass 实例获取 CanMerge 属性值。
		/// </summary>
		private static readonly Lazy<Func<object, bool>> RccGetCanMerge =
			RccType.CreateDelegateLazy<Func<object, bool>>("CanMerge");
		/// <summary>
		/// RegexCharClass 实例获取 Negate 属性值。
		/// </summary>
		private static readonly Lazy<Func<object, bool>> RccGetNegate =
			RccType.CreateDelegateLazy<Func<object, bool>>("_negate");
		/// <summary>
		/// RegexCharClass 实例设置 Negate 属性值。
		/// </summary>
		private static readonly Lazy<Action<object, bool>> RccSetNegate =
			RccType.CreateDelegateLazy<Action<object, bool>>("Negate");

		#endregion

		#region RegexCharClass.SingleRange 类方法调用

		/// <summary>
		/// RegexCharClass.SingleRange 类的类型。
		/// </summary>
		private static readonly Type RccSingleRangeType =
			RccType.GetNestedType("SingleRange", BindingFlags.NonPublic);
		/// <summary>
		/// List&lt;RegexCharClass.SingleRange&gt; 类的类型。
		/// </summary>
		private static readonly Type RccSingleRangeListType = typeof(List<>).MakeGenericType(RccSingleRangeType);
		/// <summary>
		/// List&lt;RegexCharClass.SingleRange&gt; 类的 Add 方法。
		/// </summary>
		private static readonly Lazy<Action<object, object>> RccSRListAdd =
			RccSingleRangeListType.CreateDelegateLazy<Action<object, object>>("Add");
		/// <summary>
		/// RegexCharClass.SingleRange 类的构造函数。
		/// </summary>
		private static readonly Lazy<Func<char, char, object>> RccSRConstructor =
			RccSingleRangeType.CreateDelegateLazy<Func<char, char, object>>(".ctor");
		/// <summary>
		/// RegexCharClass.SingleRange 类的 _first 字段。
		/// </summary>
		private static readonly Lazy<Func<object, char>> RccSRGetFirst =
			RccSingleRangeType.CreateDelegateLazy<Func<object, char>>("_first");
		/// <summary>
		/// RegexCharClass.SingleRange 类的 _last 字段。
		/// </summary>
		private static readonly Lazy<Func<object, char>> RccSRGetLast =
			RccSingleRangeType.CreateDelegateLazy<Func<object, char>>("_last");

		#endregion // RegexCharClass.SingleRange 类方法调用

		#region 静态函数

		/// <summary>
		/// 获取与任何空白匹配的字符类。
		/// </summary>
		public static string SpaceClass
		{
			get { return RccSpaceClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何空白匹配的字符类。
		/// </summary>
		public static string NotSpaceClass
		{
			get { return RccNotSpaceClass.Value(); }
		}
		/// <summary>
		/// 获取与任何单词字符匹配的字符类。
		/// </summary>
		public static string WordClass
		{
			get { return RccWordClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何单词字符匹配的字符类。
		/// </summary>
		public static string NotWordClass
		{
			get { return RccNotWordClass.Value(); }
		}
		/// <summary>
		/// 获取与任何十进制数字匹配的字符类。
		/// </summary>
		public static string DigitClass
		{
			get { return RccDigitClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何十进制数字匹配的字符类。
		/// </summary>
		public static string NotDigitClass
		{
			get { return RccNotDigitClass.Value(); }
		}
		/// <summary>
		/// 获取与任何符合 ECMAScript 行为的空白匹配的字符类。
		/// </summary>
		public static string EcmaSpaceClass
		{
			get { return RccECMASpaceClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何符合 ECMAScript 行为的空白匹配的字符类。
		/// </summary>
		public static string NotEcmaSpaceClass
		{
			get { return RccNotECMASpaceClass.Value(); }
		}
		/// <summary>
		/// 获取与任何符合 ECMAScript 行为的单词字符匹配的字符类。
		/// </summary>
		public static string EcmaWordClass
		{
			get { return RccECMAWordClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何符合 ECMAScript 行为的单词字符匹配的字符类。
		/// </summary>
		public static string NotEcmaWordClass
		{
			get { return RccNotECMAWordClass.Value(); }
		}
		/// <summary>
		/// 获取与任何符合 ECMAScript 行为的十进制数字匹配的字符类。
		/// </summary>
		public static string EcmaDigitClass
		{
			get { return RccECMADigitClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何符合 ECMAScript 行为的十进制数字匹配的字符类。
		/// </summary>
		public static string NotEcmaDigitClass
		{
			get { return RccNotECMADigitClass.Value(); }
		}
		/// <summary>
		/// 返回特定字符是否在字符类内。
		/// </summary>
		/// <param name="ch">要判断的字符。</param>
		/// <param name="set">要判断的字符类。</param>
		/// <returns>如果字符属于字符类，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
		public static bool CharInClass(char ch, string set)
		{
			return RccCharInClass.Value(ch, set);
		}
		/// <summary>
		/// 返回特定字符是否是符合 ECMAScript 行为的单词字符。
		/// </summary>
		/// <param name="ch">要判断的字符。</param>
		/// <returns>如果字符是符合 ECMAScript 行为的单词字符，则返回 <c>true</c>；
		/// 否则返回 <c>false</c>。</returns>
		public static bool IsEcmaWordChar(char ch)
		{
			return RccIsECMAWordChar.Value(ch);
		}
		/// <summary>
		/// 返回给定的字符类是否是空字符类。
		/// </summary>
		/// <param name="charClass">要判断的字符类。</param>
		/// <returns>如果字符类是空的，则返回 <c>true</c>。否则返回 <c>false</c>。</returns>
		public static bool IsEmpty(string charClass)
		{
			return RccIsEmpty.Value(charClass);
		}
		/// <summary>
		/// 返回给定的字符类是否是可合并的。
		/// </summary>
		/// <param name="charClass">要判断的字符类。</param>
		/// <returns>如果字符类是可合并的，则返回 <c>true</c>。否则返回 <c>false</c>。</returns>
		public static bool IsMergeable(string charClass)
		{
			return RccIsMergeable.Value(charClass);
		}
		/// <summary>
		/// 返回给定的字符类是否是否定的字符类。
		/// </summary>
		/// <param name="charClass">要判断的字符类。</param>
		/// <returns>如果字符类是否定的，则返回 <c>true</c>。否则返回 <c>false</c>。</returns>
		public static bool IsNegated(string charClass)
		{
			return RccIsNegated.Value(charClass);
		}
		/// <summary>
		/// 返回给定的字符类是否只包含单个字符。
		/// </summary>
		/// <param name="charClass">要判断的字符类。</param>
		/// <returns>如果字符类只包含单个字符，则返回 <c>true</c>。否则返回 <c>false</c>。</returns>
		public static bool IsSingleton(string charClass)
		{
			return RccIsSingleton.Value(charClass);
		}
		/// <summary>
		/// 返回给定的字符类是否只包含单个字符的否定。
		/// </summary>
		/// <param name="charClass">要判断的字符类。</param>
		/// <returns>如果字符类只包含单个字符的否定，则返回 <c>true</c>。否则返回 <c>false</c>。</returns>
		public static bool IsSingletonInverse(string charClass)
		{
			return RccIsSingletonInverse.Value(charClass);
		}
		/// <summary>
		/// 返回给定的字符类是否包含差集。
		/// </summary>
		/// <param name="charClass">要判断的字符类。</param>
		/// <returns>如果字符类包含差集，则返回 <c>true</c>。否则返回 <c>false</c>。</returns>
		public static bool IsSubtraction(string charClass)
		{
			return RccIsSubtraction.Value(charClass);
		}
		/// <summary>
		/// 返回特定字符是否是单词字符。
		/// </summary>
		/// <param name="ch">要判断的字符。</param>
		/// <returns>如果字符是单词字符，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
		public static bool IsWordChar(char ch)
		{
			return RccIsWordChar.Value(ch);
		}
		/// <summary>
		/// 返回只含有单个字符的字符类对应的字符。
		/// </summary>
		/// <param name="set">要获取字符的字符类。</param>
		/// <returns>字符类对应的字符。</returns>
		public static char SingletonChar(string set)
		{
			return RccSingletonChar.Value(set);
		}
		/// <summary>
		/// 从字符串形式的字符范围获取 <see cref="RegexCharClass"/> 对象。
		/// </summary>
		/// <param name="charClass">字符串形式的字符范围。</param>
		/// <returns><see cref="RegexCharClass"/> 对象。</returns>
		public static RegexCharClass Parse(string charClass)
		{
			return new RegexCharClass(charClass);
		}
		/// <summary>
		/// 从正则表达式的字符类模式获取 <see cref="RegexCharClass"/> 对象。
		/// </summary>
		/// <param name="pattern">字符类模式。</param>
		/// <returns><see cref="RegexCharClass"/> 对象。</returns>
		public static RegexCharClass ParsePattern(string pattern)
		{
			return null;
			// return RegexParser.ParseCharClass(pattern, RegexOptions.None);
		}
		/// <summary>
		/// 从正则表达式的字符类模式获取 <see cref="RegexCharClass"/> 对象。
		/// </summary>
		/// <param name="pattern">字符类模式。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <returns><see cref="RegexCharClass"/> 对象。</returns>
		//public static RegexCharClass ParsePattern(string pattern, RegexOptions option)
		//{
		//	// return RegexParser.ParseCharClass(pattern, option);
		//}

		#region 字符类描述

		/// <summary>
		/// 表示字符类长度的索引。
		/// </summary>
		private const int SetLength = 1;
		/// <summary>
		/// 表示字符类通用 Unicode 类别的长度的索引。
		/// </summary>
		private const int CategoryLength = 2;
		/// <summary>
		/// 表示字符类的起始索引。
		/// </summary>
		private const int SetStart = 3;
		/// <summary>
		/// 返回字符类的可读描述。
		/// </summary>
		/// <param name="charClass">要获取可读描述的字符类。</param>
		/// <returns>字符类的可读描述。</returns>
		public static string CharClassDescription(string charClass)
		{
			int setLen = charClass[SetLength];
			int categoryLen = charClass[CategoryLength];
			int endPos = SetStart + setLen + categoryLen;
			StringBuilder builder = new StringBuilder("[");
			int index = SetStart;
			char ch1, ch2;
			if (IsNegated(charClass))
			{
				builder.Append('^');
			}
			while (index < SetStart + charClass[SetLength])
			{
				ch1 = charClass[index];
				if (index + 1 < charClass.Length)
				{
					ch2 = (char)(charClass[index + 1] - 1);
				}
				else
				{
					ch2 = char.MaxValue;
				}
				builder.Append(CharDescription(ch1));
				if (ch2 != ch1)
				{
					// 字符范围。
					if (ch1 + 1 != ch2)
					{
						builder.Append('-');
					}
					builder.Append(CharDescription(ch2));
				}
				index += 2;
			}
			while (index < SetStart + charClass[SetLength] + charClass[CategoryLength])
			{
				ch1 = charClass[index];
				if (ch1 == 0)
				{
					bool found = false;
					int lastindex = charClass.IndexOf('\0', index + 1);
					string group = charClass.Substring(index, lastindex - index + 1);
					foreach (KeyValuePair<string, string> pair in RccGetDefinedCategories.Value())
					{
						if (group.Equals(pair.Value))
						{
							if ((short)charClass[index + 1] > 0)
							{
								builder.Append("\\p{");
							}
							else
							{
								builder.Append("\\P{");
							}
							builder.Append(pair.Key);
							builder.Append("}");
							found = true;
							break;
						}
					}
					if (!found)
					{
						if (group.Equals(RccGetWord.Value()))
						{
							builder.Append("\\w");
						}
						else
						{
							if (group.Equals(RccGetNotWord.Value()))
							{
								builder.Append("\\W");
							}
						}
					}
					index = lastindex;
				}
				else
				{
					builder.Append(CategoryDescription(ch1));
				}
				index++;
			}
			if (charClass.Length > endPos)
			{
				// 减去范围。
				builder.Append('-');
				builder.Append(CharClassDescription(charClass.Substring(endPos)));
			}
			builder.Append(']');
			return builder.ToString();
		}
		/// <summary>
		/// 返回字符的可读描述。
		/// </summary>
		/// <param name="ch">要获取可读描述的字符。</param>
		/// <returns>字符的可读描述。</returns>
		private static string CharDescription(char ch)
		{
			// Regex 转义。
			if (ch == ']')
			{
				return "\\]";
			}
			else if (ch == '-')
			{
				return "\\-";
			}
			return ch.ToPrintableString();
		}
		/// <summary>
		/// 所有 Unicode 通用类别的名称。
		/// </summary>
		private static readonly string[] Categories = new string[] {"Lu", "Ll", "Lt", "Lm", "Lo",
			"Mn", "Mc", "Me", "Nd", "Nl", "No", "Zs", "Zl", "Zp", "Cc", "Cf", "Cs", "Co",
			"Pc", "Pd", "Ps", "Pe", "Pi", "Pf", "Po", "Sm", "Sc", "Sk", "So", "Cn" };
		/// <summary>
		/// 返回类别字符的可读描述。
		/// </summary>
		/// <param name="ch">要获取可读描述的类别字符。</param>
		/// <returns>类别字符的可读描述。</returns>
		private static String CategoryDescription(char ch)
		{
			if (ch == 100)
			{
				// SpaceConst = 100。
				return "\\s";
			}
			else if ((short)ch == -100)
			{
				// NotSpaceConst = -100。
				return "\\S";
			}
			else if ((short)ch < 0)
			{
				return String.Format(CultureInfo.InvariantCulture, "\\P{{{0}}}", Categories[-((short)ch) - 1]);
			}
			else
			{
				return String.Format(CultureInfo.InvariantCulture, "\\p{{{0}}}", Categories[ch - 1]);
			}
		}

		#endregion

		#endregion

		/// <summary>
		/// 包装的 RegexCharClass 对象。
		/// </summary>
		private readonly object charClass;
		/// <summary>
		/// 初始化 <see cref="RegexCharClass"/> 类的新实例。
		/// </summary>
		public RegexCharClass()
		{
			charClass = RccConstructor.Value();
		}
		/// <summary>
		/// 使用给定的字符类初始化 <see cref="RegexCharClass"/> 类的新实例。
		/// </summary>
		/// <param name="charClass">初始化使用的字符类。</param>
		public RegexCharClass(string charClass)
		{
			this.charClass = RccParse.Value(charClass);
		}
		/// <summary>
		/// 返回当前对象的字符串形式。
		/// </summary>
		/// <returns>当前对象的字符串形式。</returns>
		public override string ToString()
		{
			return CharClassDescription(this.ToStringClass());
		}

		#region 方法和属性

		/// <summary>
		/// 根据名称添加通用 Unicode 类别。
		/// </summary>
		/// <param name="categoryName">通用 Unicode 类别的名称。</param>
		/// <param name="invert">是否对通用 Unicode 类别取否定。</param>
		/// <param name="caseInsensitive">是否不区分字母大小写。</param>
		/// <param name="pattern">字符类的模式串。</param>
		public void AddCategoryFromName(string categoryName, bool invert, bool caseInsensitive, string pattern)
		{
			RccAddCategoryFromName.Value(charClass, categoryName, invert, caseInsensitive, pattern);
		}
		/// <summary>
		/// 添加指定字符。
		/// </summary>
		/// <param name="ch">要添加的字符。</param>
		public void AddChar(char ch)
		{
			RccAddChar.Value(charClass, ch);
		}
		/// <summary>
		/// 添加指定字符类。
		/// </summary>
		/// <param name="charClass">要添加的字符类。</param>
		public void AddCharClass(RegexCharClass charClass)
		{
			// 这个方法的原版实现会直接将 cc 中的字符范围添加到当前类中，
			// 而不是复制副本，有时会导致出错。
			if (!(charClass.CanMerge && this.CanMerge))
			{
				CompilerExceptionHelper.RegexCharClassCannotMerge("charClass");
			}
			int ccRangeCount = RccRangeCount.Value(charClass.charClass);
			if (!RccGetCanonical.Value(charClass.charClass))
			{
				// 如果要合并的字符类并不规范，则自己也不规范。
				RccSetCanonical.Value(charClass, false);
			}
			else if (RccGetCanonical.Value(charClass) && RccRangeCount.Value(charClass) > 0 && ccRangeCount > 0 &&
				RccSRGetFirst.Value(RccGetRangeAt.Value(charClass.charClass, 0)) <=
					RccSRGetLast.Value(RccGetRangeAt.Value(this, RccRangeCount.Value(charClass) - 1)))
			{
				RccSetCanonical.Value(charClass, false);
			}
			object list = RccGetRangelist.Value(charClass);
			for (int i = 0; i < ccRangeCount; i += 1)
			{
				object range = RccGetRangeAt.Value(charClass.charClass, i);
				// 这里创建一个新的字符范围。
				RccSRListAdd.Value(list, RccSRConstructor.Value(RccSRGetFirst.Value(range), RccSRGetLast.Value(range)));
			}
			RccGetCategories.Value(charClass).Append(RccGetCategories.Value(charClass.charClass).ToString());
		}
		/// <summary>
		/// 添加数字字符。
		/// </summary>
		/// <param name="ecma">是否使用 ECMAScript 行为。</param>
		/// <param name="negate">是否对数字字符取否定。</param>
		/// <param name="pattern">字符类的模式串。</param>
		public void AddDigit(bool ecma, bool negate, string pattern)
		{
			RccAddDigit.Value(charClass, ecma, negate, pattern);
		}
		/// <summary>
		/// 添加小写字母。
		/// </summary>
		/// <param name="culture">小写字母的区域信息。</param>
		public void AddLowercase(CultureInfo culture)
		{
			RccAddLowercase.Value(charClass, culture);
		}
		/// <summary>
		/// 添加特定范围的字符。
		/// </summary>
		/// <param name="first">要添加的范围的起始字符（包含）。</param>
		/// <param name="last">要添加的范围的结束字符（包含）。</param>
		public void AddRange(char first, char last)
		{
			RccAddRange.Value(charClass, first, last);
		}
		/// <summary>
		/// 添加空白字符。
		/// </summary>
		/// <param name="ecma">是否使用 ECMAScript 行为。</param>
		/// <param name="negate">是否对空白字符取否定。</param>
		public void AddSpace(bool ecma, bool negate)
		{
			RccAddSpace.Value(charClass, ecma, negate);
		}
		/// <summary>
		/// 添加要排除的字符类。
		/// </summary>
		/// <param name="subCharClass">要排除的字符类。</param>
		public void AddSubtraction(RegexCharClass subCharClass)
		{
			RccAddSubtraction.Value(charClass, subCharClass.charClass);
		}
		/// <summary>
		/// 添加单词字符。
		/// </summary>
		/// <param name="ecma">是否使用 ECMAScript 行为。</param>
		/// <param name="negate">是否对单词字符取否定。</param>
		public void AddWord(bool ecma, bool negate)
		{
			RccAddWord.Value(charClass, ecma, negate);
		}
		/// <summary>
		/// 返回字符类的字符串表示。
		/// </summary>
		/// <returns>字符串表示的字符类。</returns>
		public string ToStringClass()
		{
			return RccToStringClass.Value(charClass);
		}
		/// <summary>
		/// 获取当前字符类是否可以与其他字符类合并。
		/// </summary>
		public bool CanMerge
		{
			get { return RccGetCanMerge.Value(charClass); }
		}
		/// <summary>
		/// 获取或设置当前字符类是否是否定。
		/// </summary>
		public bool Negate
		{
			get { return RccGetNegate.Value(charClass); }
			set { RccSetNegate.Value(charClass, value); }
		}

		#endregion

	}
}
