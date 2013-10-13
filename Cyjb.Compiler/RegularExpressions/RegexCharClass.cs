using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
		private static readonly Lazy<Func<object, IList>> RccGetRangelist =
			RccType.CreateDelegateLazy<Func<object, IList>>("_rangelist");
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
		/// RegexCharClass 实例的 GetRangeAt 方法。
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
		/// RegexCharClass 实例的 AddLowercaseRange 方法。
		/// </summary>
		private static readonly Lazy<Action<object, char, char, CultureInfo>> RccAddLowercaseRange =
			RccType.CreateDelegateLazy<Action<object, char, char, CultureInfo>>("AddLowercaseRange");
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
		/// <value>任何空白匹配的字符类。</value>
		public static string SpaceClass
		{
			get { return RccSpaceClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何空白匹配的字符类。
		/// </summary>
		/// <value>不与任何空白匹配的字符类。</value>
		public static string NotSpaceClass
		{
			get { return RccNotSpaceClass.Value(); }
		}
		/// <summary>
		/// 获取与任何单词字符匹配的字符类。
		/// </summary>
		/// <value>与任何单词字符匹配的字符类。</value>
		public static string WordClass
		{
			get { return RccWordClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何单词字符匹配的字符类。
		/// </summary>
		/// <value>不与任何单词字符匹配的字符类。</value>
		public static string NotWordClass
		{
			get { return RccNotWordClass.Value(); }
		}
		/// <summary>
		/// 获取与任何十进制数字匹配的字符类。
		/// </summary>
		/// <value>与任何十进制数字匹配的字符类。</value>
		public static string DigitClass
		{
			get { return RccDigitClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何十进制数字匹配的字符类。
		/// </summary>
		/// <value>不与任何十进制数字匹配的字符类。</value>
		public static string NotDigitClass
		{
			get { return RccNotDigitClass.Value(); }
		}
		/// <summary>
		/// 获取与任何符合 ECMAScript 行为的空白匹配的字符类。
		/// </summary>
		/// <value>与任何符合 ECMAScript 行为的空白匹配的字符类。</value>
		public static string EcmaSpaceClass
		{
			get { return RccECMASpaceClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何符合 ECMAScript 行为的空白匹配的字符类。
		/// </summary>
		/// <value>不与任何符合 ECMAScript 行为的空白匹配的字符类。</value>
		public static string NotEcmaSpaceClass
		{
			get { return RccNotECMASpaceClass.Value(); }
		}
		/// <summary>
		/// 获取与任何符合 ECMAScript 行为的单词字符匹配的字符类。
		/// </summary>
		/// <value>与任何符合 ECMAScript 行为的单词字符匹配的字符类。</value>
		public static string EcmaWordClass
		{
			get { return RccECMAWordClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何符合 ECMAScript 行为的单词字符匹配的字符类。
		/// </summary>
		/// <value>不与任何符合 ECMAScript 行为的单词字符匹配的字符类。</value>
		public static string NotEcmaWordClass
		{
			get { return RccNotECMAWordClass.Value(); }
		}
		/// <summary>
		/// 获取与任何符合 ECMAScript 行为的十进制数字匹配的字符类。
		/// </summary>
		/// <value>与任何符合 ECMAScript 行为的十进制数字匹配的字符类。</value>
		public static string EcmaDigitClass
		{
			get { return RccECMADigitClass.Value(); }
		}
		/// <summary>
		/// 获取不与任何符合 ECMAScript 行为的十进制数字匹配的字符类。
		/// </summary>
		/// <value>不与任何符合 ECMAScript 行为的十进制数字匹配的字符类。</value>
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
		/// <param name="charClass">要获取字符的字符类。</param>
		/// <returns>字符类对应的字符。</returns>
		public static char SingletonChar(string charClass)
		{
			return RccSingletonChar.Value(charClass);
		}
		/// <summary>
		/// 返回字符类中时候包含 Unicode 字符分类的定义。
		/// </summary>
		/// <param name="charClass">要判断是否包含 Unicode 字符分类的字符类。</param>
		/// <returns>如果包含，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public static bool ContainsCategory(string charClass)
		{
			return charClass[CategoryLengthIndex] > 0;
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
		/// <overloads>
		/// <summary>
		/// 从正则表达式的字符类模式获取 <see cref="RegexCharClass"/> 对象。
		/// </summary>
		/// </overloads>
		public static RegexCharClass ParsePattern(string pattern)
		{
			return RegexParser.ParseCharClass(pattern, RegexOptions.None, false);
		}
		/// <summary>
		/// 从正则表达式的字符类模式获取 <see cref="RegexCharClass"/> 对象。
		/// </summary>
		/// <param name="pattern">字符类模式。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <returns><see cref="RegexCharClass"/> 对象。</returns>
		public static RegexCharClass ParsePattern(string pattern, RegexOptions option)
		{
			return RegexParser.ParseCharClass(pattern, option, false);
		}
		/// <summary>
		/// 返回正则表达式的字符类中定义的字符范围集合。
		/// </summary>
		/// <param name="charClass">要获取字符范围的字符类。</param>
		/// <returns>字符类中定义的字符范围集合。</returns>
		internal static string GetCharClassRanges(string charClass)
		{
			Debug.Assert(!IsSubtraction(charClass));
			return charClass.Substring(SetStart, charClass[SetLengthIndex]);
		}

		#region 字符类描述

		/// <summary>
		/// 表示字符类长度的索引。
		/// </summary>
		private const int SetLengthIndex = 1;
		/// <summary>
		/// 表示字符类通用 Unicode 类别的长度的索引。
		/// </summary>
		private const int CategoryLengthIndex = 2;
		/// <summary>
		/// 表示字符类的起始索引。
		/// </summary>
		private const int SetStart = 3;
		/// <summary>
		/// 返回字符类的可读描述。
		/// </summary>
		/// <param name="charClass">要获取可读描述的字符类。</param>
		/// <returns>字符类的可读描述。</returns>
		public static string GetDescription(string charClass)
		{
			StringBuilder builder = new StringBuilder();
			GetDescription(charClass, builder);
			return builder.ToString();
		}
		/// <summary>
		/// 返回字符类的可读描述。
		/// </summary>
		/// <param name="charClass">要获取可读描述的字符类。</param>
		/// <param name="builder">保存字符类的可读描述的可变字符串。</param>
		private static void GetDescription(string charClass, StringBuilder builder)
		{
			int setLen = charClass[SetLengthIndex];
			int categoryLen = charClass[CategoryLengthIndex];
			builder.Append('[');
			if (IsNegated(charClass))
			{
				builder.Append('^');
			}
			int index = SetStart;
			int endPos = SetStart + setLen;
			char ch1, ch2;
			while (index < endPos)
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
			endPos += categoryLen;
			while (index < endPos)
			{
				ch1 = charClass[index];
				if (ch1 == '\0')
				{
					bool found = false;
					int endIndex = charClass.IndexOf('\0', index + 1);
					string group = charClass.Substring(index, endIndex - index + 1);
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
					index = endIndex;
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
				GetDescription(charClass.Substring(endPos), builder);
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
			short sch = (short)ch;
			if (sch == 100)
			{
				// SpaceConst = 100。
				return "\\s";
			}
			else if (sch == -100)
			{
				// NotSpaceConst = -100。
				return "\\S";
			}
			else if (sch < 0)
			{
				return string.Format(CultureInfo.InvariantCulture, "\\P{{{0}}}", Categories[-sch - 1]);
			}
			else
			{
				return string.Format(CultureInfo.InvariantCulture, "\\p{{{0}}}", Categories[ch - 1]);
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
		/// <overloads>
		/// <summary>
		/// 初始化 <see cref="RegexCharClass"/> 类的新实例。
		/// </summary>
		/// </overloads>
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
			return GetDescription(this.ToStringClass());
		}

		#region 方法和属性

		/// <summary>
		/// 根据名称添加通用 Unicode 类别。
		/// </summary>
		/// <param name="categoryName">通用 Unicode 类别的名称。</param>
		/// <param name="invert">是否对通用 Unicode 类别取否定。</param>
		/// <param name="caseInsensitive">是否不区分字母大小写。</param>
		public void AddCategoryFromName(string categoryName, bool invert, bool caseInsensitive)
		{
			RccAddCategoryFromName.Value(charClass, categoryName, invert, caseInsensitive, categoryName);
		}
		/// <summary>
		/// 根据名称添加通用 Unicode 类别。
		/// </summary>
		/// <param name="categoryName">通用 Unicode 类别的名称。</param>
		/// <param name="invert">是否对通用 Unicode 类别取否定。</param>
		/// <param name="caseInsensitive">是否不区分字母大小写。</param>
		/// <param name="pattern">字符类的模式串。</param>
		internal void AddCategoryFromName(string categoryName, bool invert, bool caseInsensitive, string pattern)
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
		/// <param name="cc">要添加的字符类。</param>
		public void AddCharClass(RegexCharClass cc)
		{
			// 这个方法的原版实现会直接将 cc 中的字符范围添加到当前类中，
			// 而不是复制副本，有时会导致出错。
			if (!(cc.CanMerge && this.CanMerge))
			{
				CompilerExceptionHelper.RegexCharClassCannotMerge("charClass");
			}
			int ccRangeCount = RccRangeCount.Value(cc.charClass);
			int thisRangeCount = RccRangeCount.Value(this.charClass);
			if (!RccGetCanonical.Value(cc.charClass))
			{
				// 如果要合并的字符类并不规范，则自己也不规范。
				RccSetCanonical.Value(this.charClass, false);
			}
			else if (RccGetCanonical.Value(this.charClass) && thisRangeCount > 0 && ccRangeCount > 0 &&
				RccSRGetFirst.Value(RccGetRangeAt.Value(cc.charClass, 0)) <=
					RccSRGetLast.Value(RccGetRangeAt.Value(this, thisRangeCount - 1)))
			{
				// Range 不能恰好接起来，那么也会导致不规范。
				RccSetCanonical.Value(this.charClass, false);
			}
			IList list = RccGetRangelist.Value(this.charClass);
			for (int i = 0; i < ccRangeCount; i += 1)
			{
				object range = RccGetRangeAt.Value(cc.charClass, i);
				// 这里创建一个新的字符范围。
				list.Add(RccSRConstructor.Value(RccSRGetFirst.Value(range), RccSRGetLast.Value(range)));
			}
			RccGetCategories.Value(this.charClass).Append(RccGetCategories.Value(cc.charClass).ToString());
		}
		/// <summary>
		/// 添加数字字符。
		/// </summary>
		/// <param name="ecma">是否使用 ECMAScript 行为。</param>
		/// <param name="negate">是否对数字字符取否定。</param>
		public void AddDigit(bool ecma, bool negate)
		{
			RccAddDigit.Value(charClass, ecma, negate, null);
		}
		/// <summary>
		/// 添加数字字符。
		/// </summary>
		/// <param name="ecma">是否使用 ECMAScript 行为。</param>
		/// <param name="negate">是否对数字字符取否定。</param>
		/// <param name="pattern">字符类的模式串。</param>
		internal void AddDigit(bool ecma, bool negate, string pattern)
		{
			RccAddDigit.Value(charClass, ecma, negate, pattern);
		}
		/// <summary>
		/// 将当前字符类所对应的小写字符添加到字符类中。
		/// </summary>
		/// <param name="culture">获取小写字符使用的区域信息。</param>
		public void AddLowercase(CultureInfo culture)
		{
			RccSetCanonical.Value(charClass, false);
			int count = RccRangeCount.Value(this.charClass);
			for (int i = 0; i < count; i++)
			{
				object range = RccGetRangeAt.Value(this.charClass, i);
				char first = RccSRGetFirst.Value(range);
				char last = RccSRGetLast.Value(range);
				if (first == last)
				{
					char temp = char.ToLower(first, culture);
					if (temp != first)
					{
						this.AddChar(temp);
					}
				}
				else
				{
					RccAddLowercaseRange.Value(this.charClass, first, last, culture);
				}
			}
		}
		/// <summary>
		/// 将当前字符类所对应的大写字符添加到字符类中。
		/// </summary>
		/// <param name="culture">获取大写字符使用的区域信息。</param>
		public void AddUppercase(CultureInfo culture)
		{
			RccSetCanonical.Value(charClass, false);
			int count = RccRangeCount.Value(this.charClass);
			for (int i = 0; i < count; i++)
			{
				object range = RccGetRangeAt.Value(this.charClass, i);
				char first = RccSRGetFirst.Value(range);
				char last = RccSRGetLast.Value(range);
				if (first == last)
				{
					char temp = char.ToUpper(first, culture);
					if (temp != first)
					{
						this.AddChar(temp);
					}
				}
				else
				{
					AddUppercaseRange(first, last);
				}
			}
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
		/// <value>如果当前字符类可以与其他字符类合并，则为 <c>true</c>；否则为 <c>false</c>。</value>
		public bool CanMerge
		{
			get { return RccGetCanMerge.Value(charClass); }
		}
		/// <summary>
		/// 获取或设置当前字符类是否是否定。
		/// </summary>
		/// <value>如果当前字符类表示否定，则为 <c>true</c>；否则为 <c>false</c>。</value>
		public bool Negate
		{
			get { return RccGetNegate.Value(charClass); }
			set { RccSetNegate.Value(charClass, value); }
		}
		/// <summary>
		/// 添加指定字符范围对应的大写字符。
		/// </summary>
		/// <param name="chMin">字符范围的起始。</param>
		/// <param name="chMax">字符范围的结束。</param>
		private void AddUppercaseRange(char chMin, char chMax)
		{
			int i, iMax, iMid;
			// 二分查找匹配的映射关系。
			for (i = 0, iMax = ucTable.Length; i < iMax; )
			{
				iMid = (i + iMax) / 2;
				if (ucTable[iMid].ChMax < chMin) { i = iMid + 1; }
				else { iMax = iMid; }
			}
			if (i >= ucTable.Length) { return; }
			char chMinT, chMaxT;
			UpperCaseMapping uc;
			// 将小写字符映射为大写字符。
			for (; i < ucTable.Length && (uc = ucTable[i]).ChMin <= chMax; i++)
			{
				if ((chMinT = uc.ChMin) < chMin) { chMinT = chMin; }
				if ((chMaxT = uc.ChMax) > chMax) { chMaxT = chMax; }
				switch (uc.Operator)
				{
					case UppercaseSet:
						chMinT = (char)uc.Data;
						chMaxT = (char)uc.Data;
						break;
					case UppercaseAdd:
						chMinT += (char)uc.Data;
						chMaxT += (char)uc.Data;
						break;
					case UppercaseBor:
						chMinT |= (char)1;
						chMaxT |= (char)1;
						break;
					case UppercaseBad:
						chMinT += (char)(chMinT & 1);
						chMaxT += (char)(chMaxT & 1);
						break;
				}
				if (chMinT < chMin || chMaxT > chMax)
				{
					AddRange(chMinT, chMaxT);
				}
			}
		}

		#endregion

		#region 小写字符到相应的大写字符的映射

		/// <summary>
		/// 小写字符到相应的大写字符的映射关系。
		/// </summary>
		private struct UpperCaseMapping
		{
			/// <summary>
			/// 初始化 <see cref="UpperCaseMapping"/> 结构。
			/// </summary>
			/// <param name="chMin">小写字符的最小值。</param>
			/// <param name="chMax">小写字符的最大值。</param>
			/// <param name="op">到相应大写字符的映射操作。</param>
			/// <param name="data">映射中使用的数据。</param>
			internal UpperCaseMapping(char chMin, char chMax, int op, int data)
			{
				this.ChMin = chMin;
				this.ChMax = chMax;
				this.Operator = op;
				this.Data = data;
			}
			/// <summary>
			/// 小写字符的最小值。
			/// </summary>
			internal char ChMin;
			/// <summary>
			/// 小写字符的最大值。
			/// </summary>
			internal char ChMax;
			/// <summary>
			/// 到相应大写字符的映射操作。
			/// </summary>
			internal int Operator;
			/// <summary>
			/// 映射中使用的数据。
			/// </summary>
			internal int Data;
		}
		/// <summary>
		/// 直接设置相应的大写字符：U(ch) = constant。
		/// </summary>
		private const int UppercaseSet = 0;
		/// <summary>
		/// 加上一个参数：U(ch) = ch + offset。
		/// </summary>
		private const int UppercaseAdd = 1;
		/// <summary>
		/// 按位或 1：U(ch) = ch | 1。
		/// </summary>
		private const int UppercaseBor = 2;
		/// <summary>
		/// 按位与 1：U(ch) = ch + (ch &amp; 1)。
		/// </summary>
		private const int UppercaseBad = 3;
		/// <summary>
		/// 小写字符到相应的大写字符的映射关系。
		/// </summary>
		private static readonly UpperCaseMapping[] ucTable = new UpperCaseMapping[] 
		{
			new UpperCaseMapping('\u0061', '\u007A', UppercaseAdd, -32),
			new UpperCaseMapping('\u00E0', '\u00FE', UppercaseAdd, -32),
			new UpperCaseMapping('\u0101', '\u012F', UppercaseBad, 0),
			new UpperCaseMapping('\u0069', '\u0069', UppercaseSet, 0x0130),
			new UpperCaseMapping('\u0133', '\u0137', UppercaseBad, 0),
			new UpperCaseMapping('\u013A', '\u0148', UppercaseBor, 0),
			new UpperCaseMapping('\u014B', '\u0177', UppercaseBad, 0),
			new UpperCaseMapping('\u00FF', '\u00FF', UppercaseSet, 0x0178),
			new UpperCaseMapping('\u017A', '\u017E', UppercaseBor, 0),
			new UpperCaseMapping('\u0253', '\u0253', UppercaseSet, 0x0181),
			new UpperCaseMapping('\u0183', '\u0185', UppercaseBad, 0),
			new UpperCaseMapping('\u0254', '\u0254', UppercaseSet, 0x0186),
			new UpperCaseMapping('\u0188', '\u0188', UppercaseSet, 0x0187),
			new UpperCaseMapping('\u0256', '\u0257', UppercaseAdd, -205),
			new UpperCaseMapping('\u018C', '\u018C', UppercaseSet, 0x018B),
			new UpperCaseMapping('\u01DD', '\u01DD', UppercaseSet, 0x018E),
			new UpperCaseMapping('\u0259', '\u0259', UppercaseSet, 0x018F),
			new UpperCaseMapping('\u025B', '\u025B', UppercaseSet, 0x0190),
			new UpperCaseMapping('\u0192', '\u0192', UppercaseSet, 0x0191),
			new UpperCaseMapping('\u0260', '\u0260', UppercaseSet, 0x0193),
			new UpperCaseMapping('\u0263', '\u0263', UppercaseSet, 0x0194),
			new UpperCaseMapping('\u0269', '\u0269', UppercaseSet, 0x0196),
			new UpperCaseMapping('\u0268', '\u0268', UppercaseSet, 0x0197),
			new UpperCaseMapping('\u0199', '\u0199', UppercaseSet, 0x0198),
			new UpperCaseMapping('\u026F', '\u026F', UppercaseSet, 0x019C),
			new UpperCaseMapping('\u0272', '\u0272', UppercaseSet, 0x019D),
			new UpperCaseMapping('\u0275', '\u0275', UppercaseSet, 0x019F),
			new UpperCaseMapping('\u01A1', '\u01A5', UppercaseBad, 0),
			new UpperCaseMapping('\u01A8', '\u01A8', UppercaseSet, 0x01A7),
			new UpperCaseMapping('\u0283', '\u0283', UppercaseSet, 0x01A9),
			new UpperCaseMapping('\u01AD', '\u01AD', UppercaseSet, 0x01AC),
			new UpperCaseMapping('\u0288', '\u0288', UppercaseSet, 0x01AE),
			new UpperCaseMapping('\u01B0', '\u01B0', UppercaseSet, 0x01AF),
			new UpperCaseMapping('\u028A', '\u028B', UppercaseAdd, -217),
			new UpperCaseMapping('\u01B4', '\u01B6', UppercaseBor, 0),
			new UpperCaseMapping('\u0292', '\u0292', UppercaseSet, 0x01B7),
			new UpperCaseMapping('\u01B9', '\u01B9', UppercaseSet, 0x01B8),
			new UpperCaseMapping('\u01BD', '\u01BD', UppercaseSet, 0x01BC),
			new UpperCaseMapping('\u01C6', '\u01C6', UppercaseSet, 0x01C4),
			new UpperCaseMapping('\u01C9', '\u01C9', UppercaseSet, 0x01C7),
			new UpperCaseMapping('\u01CC', '\u01CC', UppercaseSet, 0x01CA),
			new UpperCaseMapping('\u01CE', '\u01DC', UppercaseBor, 0),
			new UpperCaseMapping('\u01DF', '\u01EF', UppercaseBad, 0),
			new UpperCaseMapping('\u01F3', '\u01F3', UppercaseSet, 0x01F1),
			new UpperCaseMapping('\u01F5', '\u01F5', UppercaseSet, 0x01F4),
			new UpperCaseMapping('\u01FB', '\u0217', UppercaseBad, 0),
			new UpperCaseMapping('\u03AC', '\u03AC', UppercaseSet, 0x0386),
			new UpperCaseMapping('\u03AD', '\u03AF', UppercaseAdd, -37),
			new UpperCaseMapping('\u03CC', '\u03CC', UppercaseSet, 0x038C),
			new UpperCaseMapping('\u03CD', '\u03CE', UppercaseAdd, -63),
			new UpperCaseMapping('\u03B1', '\u03CB', UppercaseAdd, -32),
			new UpperCaseMapping('\u03E3', '\u03EF', UppercaseBad, 0),
			new UpperCaseMapping('\u0451', '\u045F', UppercaseAdd, -80),
			new UpperCaseMapping('\u0430', '\u044F', UppercaseAdd, -32),
			new UpperCaseMapping('\u0461', '\u0481', UppercaseBad, 0),
			new UpperCaseMapping('\u0491', '\u04BF', UppercaseBad, 0),
			new UpperCaseMapping('\u04C2', '\u04C4', UppercaseBor, 0),
			new UpperCaseMapping('\u04C8', '\u04C8', UppercaseSet, 0x04C7),
			new UpperCaseMapping('\u04CC', '\u04CC', UppercaseSet, 0x04CB),
			new UpperCaseMapping('\u04D1', '\u04EB', UppercaseBad, 0),
			new UpperCaseMapping('\u04EF', '\u04F5', UppercaseBad, 0),
			new UpperCaseMapping('\u04F9', '\u04F9', UppercaseSet, 0x04F8),
			new UpperCaseMapping('\u0561', '\u0586', UppercaseAdd, -48),
			new UpperCaseMapping('\u10D0', '\u10F5', UppercaseAdd, -48),
			new UpperCaseMapping('\u1E01', '\u1EF9', UppercaseBad, 0),
			new UpperCaseMapping('\u1F00', '\u1F07', UppercaseAdd, 8),
			new UpperCaseMapping('\u1F10', '\u1F17', UppercaseAdd, 8),
			new UpperCaseMapping('\u1F20', '\u1F27', UppercaseAdd, 8),
			new UpperCaseMapping('\u1F30', '\u1F37', UppercaseAdd, 8),
			new UpperCaseMapping('\u1F40', '\u1F45', UppercaseAdd, 8),
			new UpperCaseMapping('\u1F51', '\u1F51', UppercaseSet, 0x1F59),
			new UpperCaseMapping('\u1F53', '\u1F53', UppercaseSet, 0x1F5B),
			new UpperCaseMapping('\u1F55', '\u1F55', UppercaseSet, 0x1F5D),
			new UpperCaseMapping('\u1F57', '\u1F57', UppercaseSet, 0x1F5F),
			new UpperCaseMapping('\u1F60', '\u1F67', UppercaseAdd, 8),
			new UpperCaseMapping('\u1F80', '\u1F87', UppercaseAdd, 8),
			new UpperCaseMapping('\u1F90', '\u1F97', UppercaseAdd, 8),
			new UpperCaseMapping('\u1FA0', '\u1FA7', UppercaseAdd, 8),
			new UpperCaseMapping('\u1FB0', '\u1FB1', UppercaseAdd, 8),
			new UpperCaseMapping('\u1F70', '\u1F71', UppercaseAdd, 74),
			new UpperCaseMapping('\u1FB3', '\u1FB3', UppercaseSet, 0x1FBC),
			new UpperCaseMapping('\u1F72', '\u1F75', UppercaseAdd, 86),
			new UpperCaseMapping('\u1FC3', '\u1FC3', UppercaseSet, 0x1FCC),
			new UpperCaseMapping('\u1FD0', '\u1FD1', UppercaseAdd, 8),
			new UpperCaseMapping('\u1F76', '\u1F77', UppercaseAdd, 100),
			new UpperCaseMapping('\u1FE0', '\u1FE1', UppercaseAdd, 8),
			new UpperCaseMapping('\u1F7A', '\u1F7B', UppercaseAdd, 112),
			new UpperCaseMapping('\u1FE5', '\u1FE5', UppercaseSet, 0x1FEC),
			new UpperCaseMapping('\u1F78', '\u1F79', UppercaseAdd, 128),
			new UpperCaseMapping('\u1F7C', '\u1F7D', UppercaseAdd, 126),
			new UpperCaseMapping('\u1FF3', '\u1FF3', UppercaseSet, 0x1FFC),
			new UpperCaseMapping('\u2170', '\u217F', UppercaseAdd, -16),
			new UpperCaseMapping('\u24D0', '\u24EA', UppercaseAdd, -26),
			new UpperCaseMapping('\uFF41', '\uFF5A', UppercaseAdd, -32)
		};

		#endregion // 小写字符到相应的大写字符的映射

	}
}
