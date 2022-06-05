using System.Collections.Generic;
using System.Globalization;
using Cyjb;
using Cyjb.Compilers.Lexers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

/// <summary>
/// <see cref="CharClassMap"/> 类的单元测试。
/// </summary>
[TestClass]
public class UnitTestCharClassMap
{
	/// <summary>
	/// 对 <see cref="CharClassMap.GetCharClass"/> 方法进行测试。
	/// </summary>
	[TestMethod]
	public void TestGetCharClass()
	{
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
		Dictionary<UnicodeCategory, int> categories = new()
		{
			{ UnicodeCategory.UppercaseLetter, 8 },
			{ UnicodeCategory.LowercaseLetter, 9 },
		};

		CharClassMap map = new(indexes, charClasses, categories);
		Assert.AreEqual(-1, map.GetCharClass('\0'));
		Assert.AreEqual(0, map.GetCharClass('A'));
		Assert.AreEqual(1, map.GetCharClass('B'));
		Assert.AreEqual(2, map.GetCharClass('C'));
		Assert.AreEqual(-1, map.GetCharClass('D'));
		Assert.AreEqual(-1, map.GetCharClass('a'));
		Assert.AreEqual(-1, map.GetCharClass('b'));
		Assert.AreEqual(-1, map.GetCharClass('c'));
		Assert.AreEqual(-1, map.GetCharClass('d'));
		Assert.AreEqual(-1, map.GetCharClass('\u0080'));
		Assert.AreEqual(-1, map.GetCharClass('\u00BF'));
		Assert.AreEqual(8, map.GetCharClass('\u00C0'));
		Assert.AreEqual(8, map.GetCharClass('\u00DE'));
		Assert.AreEqual(9, map.GetCharClass('\u00DF'));
		Assert.AreEqual(9, map.GetCharClass('\u00FF'));
		Assert.AreEqual(9, map.GetCharClass('\u0267'));
		Assert.AreEqual(3, map.GetCharClass('\u0268'));
		Assert.AreEqual(3, map.GetCharClass('\u0269'));
		Assert.AreEqual(3, map.GetCharClass('\u0279'));
		Assert.AreEqual(3, map.GetCharClass('\u027A'));
		Assert.AreEqual(9, map.GetCharClass('\u027B'));
		Assert.AreEqual(9, map.GetCharClass('\u027F'));
		Assert.AreEqual(4, map.GetCharClass('\u0280'));
		Assert.AreEqual(5, map.GetCharClass('\u0281'));
		Assert.AreEqual(4, map.GetCharClass('\u0282'));
		Assert.AreEqual(6, map.GetCharClass('\u0283'));
		Assert.AreEqual(5, map.GetCharClass('\u0284'));
		Assert.AreEqual(4, map.GetCharClass('\u0285'));
		Assert.AreEqual(3, map.GetCharClass('\u0286'));
		Assert.AreEqual(6, map.GetCharClass('\u0287'));
		Assert.AreEqual(5, map.GetCharClass('\u0288'));
		Assert.AreEqual(4, map.GetCharClass('\u0289'));
		Assert.AreEqual(7, map.GetCharClass('\u028A'));
		Assert.AreEqual(9, map.GetCharClass('\u028B'));
		Assert.AreEqual(9, map.GetCharClass('\u02AF'));
		Assert.AreEqual(-1, map.GetCharClass('\u02B0'));
		Assert.AreEqual(8, map.GetCharClass('\u03A9'));
		Assert.AreEqual(-1, map.GetCharClass('\u03AA'));
		Assert.AreEqual(-1, map.GetCharClass('\u03AB'));
		Assert.AreEqual(-1, map.GetCharClass('\u03AC'));
		Assert.AreEqual(-1, map.GetCharClass('\u03AD'));
		Assert.AreEqual(9, map.GetCharClass('\u03AE'));
		Assert.AreEqual(-1, map.GetCharClass('\uFFFF'));
	}
}
