using System.Collections.Generic;
using Cyjb;
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

		// 合并后才会最小化 CharClass 集合。
		charClassCollection.Invoke("MergeCharClass", new Dictionary<CharClass, CharClass>());

		CharClassMap map = (CharClassMap)charClassCollection.Invoke("GetCharClassMap")!;
		int[] indexes = new int[] { 0x268027A, 0x280028A };
		int[] charClasses = new int[128 + 2 + 0xB];
		charClasses.Fill(-1, 0, 128);
		charClasses['A'] = 0;
		charClasses['B'] = 1;
		charClasses['C'] = 2;
		charClasses[128] = 3;
		charClasses[129] = -130;
		charClasses[130] = 4;
		charClasses[131] = 5;
		charClasses[132] = 4;
		charClasses[133] = 6;
		charClasses[134] = 5;
		charClasses[135] = 4;
		charClasses[136] = 3;
		charClasses[137] = 6;
		charClasses[138] = 5;
		charClasses[139] = 4;
		charClasses[140] = 7;
		CollectionAssert.AreEqual(indexes, map.Indexes);
		CollectionAssert.AreEqual(charClasses, map.CharClasses);
	}
}
