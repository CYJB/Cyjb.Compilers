using Cyjb.Collections;
using Cyjb.Compilers.Lexers;
using Cyjb.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Compile.Lexers;

/// <summary>
/// <see cref="CharClassCollection"/> 类的单元测试。
/// </summary>
[TestClass]
public class UnitTestCharClassCollection
{
	/// <summary>
	/// 对 <see cref="CharClassCollection.GetCharClassMap"/> 方法进行测试。
	/// </summary>
	[TestMethod]
	public void TestGetCharClassMap()
	{
		PrivateObject charClassCollection = new("Cyjb.Compilers.Compile", "Cyjb.Compilers.Lexers.CharClassCollection");
		charClassCollection.Invoke("GetCharClassSet", 'A');
		charClassCollection.Invoke("GetCharClassSet", 'B');
		charClassCollection.Invoke("GetCharClassSet", 'C');
		CharSet set = new();
		set.Add('\u0268', '\u027A');
		set.Add('\u0286');
		charClassCollection.Invoke("GetCharClassSet", set);
		charClassCollection.Invoke("GetCharClassSet", new CharSet("\u0280\u0282\u0285\u0289"));
		charClassCollection.Invoke("GetCharClassSet", new CharSet("\u0281\u0284\u0288"));
		charClassCollection.Invoke("GetCharClassSet", new CharSet("\u0283\u0287"));
		charClassCollection.Invoke("GetCharClassSet", new CharSet("\u028A"));

		CharClassMap map = (CharClassMap)charClassCollection.Invoke("GetCharClassMap")!;
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
		CollectionAssert.AreEqual(indexes, map.Indexes);
		CollectionAssert.AreEqual(charClasses, map.CharClasses);
	}
}
