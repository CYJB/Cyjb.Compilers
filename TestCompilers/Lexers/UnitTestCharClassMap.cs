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
		int[] indexes = new int[] { 0x268027A, 0x280028A };
		int[] charClasses = new int[128 + 2 + 0xB];
		charClasses['A'] = 1;
		charClasses['B'] = 2;
		charClasses['C'] = 3;
		charClasses[128] = 4;
		charClasses[129] = -130;
		charClasses[130] = 5;
		charClasses[131] = 6;
		charClasses[132] = 5;
		charClasses[133] = 7;
		charClasses[134] = 6;
		charClasses[135] = 5;
		charClasses[136] = 4;
		charClasses[137] = 7;
		charClasses[138] = 6;
		charClasses[139] = 5;
		charClasses[140] = 8;
		CharClassMap map = new(indexes, charClasses);
		Assert.AreEqual(0, map.GetCharClass('\0'));
		Assert.AreEqual(1, map.GetCharClass('A'));
		Assert.AreEqual(2, map.GetCharClass('B'));
		Assert.AreEqual(3, map.GetCharClass('C'));
		Assert.AreEqual(0, map.GetCharClass('D'));
		Assert.AreEqual(0, map.GetCharClass('\u0080'));
		Assert.AreEqual(0, map.GetCharClass('\u0267'));
		Assert.AreEqual(4, map.GetCharClass('\u0268'));
		Assert.AreEqual(4, map.GetCharClass('\u0269'));
		Assert.AreEqual(4, map.GetCharClass('\u0279'));
		Assert.AreEqual(4, map.GetCharClass('\u027A'));
		Assert.AreEqual(0, map.GetCharClass('\u027B'));
		Assert.AreEqual(0, map.GetCharClass('\u027F'));
		Assert.AreEqual(5, map.GetCharClass('\u0280'));
		Assert.AreEqual(6, map.GetCharClass('\u0281'));
		Assert.AreEqual(5, map.GetCharClass('\u0282'));
		Assert.AreEqual(7, map.GetCharClass('\u0283'));
		Assert.AreEqual(6, map.GetCharClass('\u0284'));
		Assert.AreEqual(5, map.GetCharClass('\u0285'));
		Assert.AreEqual(4, map.GetCharClass('\u0286'));
		Assert.AreEqual(7, map.GetCharClass('\u0287'));
		Assert.AreEqual(6, map.GetCharClass('\u0288'));
		Assert.AreEqual(5, map.GetCharClass('\u0289'));
		Assert.AreEqual(8, map.GetCharClass('\u028A'));
		Assert.AreEqual(0, map.GetCharClass('\u028B'));
		Assert.AreEqual(0, map.GetCharClass('\uFFFF'));
	}
}
