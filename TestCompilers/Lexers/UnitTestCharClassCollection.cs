using System;
using System.Collections.Generic;
using System.Globalization;
using Cyjb;
using Cyjb.Collections;
using Cyjb.Compilers.Lexers;
using Cyjb.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

/// <summary>
/// <see cref="CharClassCollection"/> 类的单元测试。
/// </summary>
[TestClass]
public class UnitTestCharClassCollection
{
	/// <summary>
	/// <see cref="CharClassCollection"/> 的类型。
	/// </summary>
	private static readonly Type CharClassCollectionType = Type.GetType("Cyjb.Compilers.Lexers.CharClassCollection, Cyjb.Compilers")!;

	/// <summary>
	/// 对 <see cref="CharClassCollection.GetCharClassMap"/> 方法进行测试。
	/// </summary>
	[TestMethod]
	public void TestGetCharClassMap()
	{
		PrivateObject charClassCollection = new(CharClassCollectionType);
		charClassCollection.Invoke("GetCharClassSet", 'A');
		charClassCollection.Invoke("GetCharClassSet", 'B');
		charClassCollection.Invoke("GetCharClassSet", 'C');
		CharSet set = new();
		set.Add('\u0268', '\u027A');
		set.Add('\u0286');
		charClassCollection.Invoke("GetCharClassSet", set);
		set = new CharSet();
		set.Add('\u0280', '\u028A');
		set.Remove('\u0286');
		charClassCollection.Invoke("GetCharClassSet", set);
		charClassCollection.Invoke("GetCharClassSet", new CharSet("\u0281\u0284\u0288"));
		charClassCollection.Invoke("GetCharClassSet", new CharSet("\u0283\u0287"));
		charClassCollection.Invoke("GetCharClassSet", new CharSet("\u028A"));
		CharSet upperSet = new();
		CharSet lowerSet = new();
		for (int i = 0; i < char.MaxValue; i++)
		{
			char ch = (char)i;
			switch (char.GetUnicodeCategory(ch))
			{
				case UnicodeCategory.UppercaseLetter:
					upperSet.Add(ch);
					break;
				case UnicodeCategory.LowercaseLetter:
					lowerSet.Add(ch);
					break;
			}
		}
		upperSet.Remove('\0', '\x7F');
		upperSet.Remove('\u03AA', '\u03AD');
		lowerSet.Remove('\0', '\x7F');
		lowerSet.Remove('\u03AA', '\u03AD');
		charClassCollection.Invoke("GetCharClassSet", upperSet);
		charClassCollection.Invoke("GetCharClassSet", lowerSet);

		// 合并后才会最小化 CharClass 集合。
		charClassCollection.Invoke("MergeCharClass", new Dictionary<CharClass, CharClass>());

		CharClassMap map = (CharClassMap)charClassCollection.Invoke("GetCharClassMap")!;
		int[] indexes = new int[] { 0x268027A, 0x280028A, 0x3AA03AD };
		int[] charClasses = new int[128 + 3 + 0xB];
		charClasses.Fill(-1, 0, 128);
		charClasses['A'] = 0;
		charClasses['B'] = 1;
		charClasses['C'] = 2;
		charClasses[128] = 3;
		charClasses[129] = -131;
		charClasses[130] = -1;
		charClasses[131] = 4;
		charClasses[132] = 5;
		charClasses[133] = 4;
		charClasses[134] = 6;
		charClasses[135] = 5;
		charClasses[136] = 4;
		charClasses[137] = 3;
		charClasses[138] = 6;
		charClasses[139] = 5;
		charClasses[140] = 4;
		charClasses[141] = 7;
		CollectionAssert.AreEqual(indexes, map.Indexes);
		CollectionAssert.AreEqual(charClasses, map.CharClasses);
		Assert.IsNotNull(map.Categories);
		Assert.AreEqual(2, map.Categories.Count);
		Assert.AreEqual(8, map.Categories[UnicodeCategory.UppercaseLetter]);
		Assert.AreEqual(9, map.Categories[UnicodeCategory.LowercaseLetter]);
	}
}
