using System;
using Cyjb;
using Cyjb.Compilers.Lexers;
using Cyjb.Compilers.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

/// <summary>
/// <see cref="Nfa"/> 类的单元测试。
/// </summary>
[TestClass]
public class UnitTestNFA
{
	/// <summary>
	/// 对 <see cref="Nfa.BuildDFA"/> 方法进行测试。
	/// </summary>
	[TestMethod]
	public void TestBuildDFA()
	{
		Nfa nfa = new();
		NfaState start = nfa.NewState();
		start.Add(nfa.BuildRegex(LexRegex.Parse("(a|b)*baa"), 4).Head);
		Dfa dfa = nfa.BuildDFA(1);
		// dfa 参见 https://www.cnblogs.com/cyjb/archive/2013/05/02/LexerDfa.html DFA 最小化 (b) 部分
		//     | 0 | 1 |
		// | 0 | 0 | 1 |
		// | 1 | 2 | 1 |
		// | 2 | 3 | 1 |
		// | 3 | 0 | 1 |
		Assert.AreEqual(2, dfa.CharClasses.Count);
		Assert.AreEqual(4, dfa.Count);
		CharClass ccA = dfa.CharClasses[0];
		CharClass ccB = dfa.CharClasses[1];
		Assert.AreEqual("[a]", ccA.ToString());
		Assert.AreEqual("[b]", ccB.ToString());
		Assert.AreEqual(dfa[0], dfa[0][ccA]);
		Assert.AreEqual(dfa[1], dfa[0][ccB]);
		Assert.AreEqual(dfa[2], dfa[1][ccA]);
		Assert.AreEqual(dfa[1], dfa[1][ccB]);
		Assert.AreEqual(dfa[3], dfa[2][ccA]);
		Assert.AreEqual(dfa[1], dfa[2][ccB]);
		Assert.AreEqual(dfa[0], dfa[3][ccA]);
		Assert.AreEqual(dfa[1], dfa[3][ccB]);
		CollectionAssert.AreEqual(new int[] { 4 }, dfa[3].Symbols);

		// 检查字符类。
		CharClassMap map = dfa.GetCharClassMap();
		int[] charClasses = new int[128];
		charClasses.Fill(-1, 0, 128);
		charClasses['a'] = 0;
		charClasses['b'] = 1;
		CollectionAssert.AreEqual(charClasses, map.CharClasses);
		Assert.AreEqual(0, map.Indexes.Length);
		Assert.IsNull(map.Categories);

		// 检查 DFA 数据。
		DfaData data = dfa.GetData();
		Assert.AreEqual(0, data.States[data.States[0]]);
		Assert.AreEqual(-1, data.States[data.States[0] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(0, data.States[data.States[0] + DfaStateData.SymbolsOffset]);

		Assert.AreEqual(2, data.States[data.States[1]]);
		Assert.AreEqual(-1, data.States[data.States[1] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(0, data.States[data.States[1] + DfaStateData.SymbolsOffset]);

		Assert.AreEqual(4, data.States[data.States[2]]);
		Assert.AreEqual(-1, data.States[data.States[2] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(0, data.States[data.States[2] + DfaStateData.SymbolsOffset]);

		Assert.AreEqual(6, data.States[data.States[3]]);
		Assert.AreEqual(-1, data.States[data.States[3] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(1, data.States[data.States[3] + DfaStateData.SymbolsOffset]);
		Assert.AreEqual(4, data.States[data.States[3] + DfaStateData.SymbolsOffset + 1]);

		CollectionAssert.AreEqual(new int[] { 0, 1, 2, 1, 3, 1, 0, 1 }, data.Next);
		CollectionAssert.AreEqual(new int[] { 0, 0, 1, 1, 2, 2, 3, 3 }, data.Check);
	}

	/// <summary>
	/// 对 <see cref="Nfa.BuildDFA"/> 方法进行测试。
	/// </summary>
	[TestMethod]
	public void TestBuildDFA2()
	{
		Nfa nfa = new();
		NfaState start = nfa.NewState();

		LexRegex quot = LexRegex.Symbol('"');
		LexRegex regularChar = LexRegex.Parse(@"[^""\\\n\r\u0085\u2028\u2029]|(\\.)");
		LexRegex regularLiteral = LexRegex.Concat(quot, regularChar.Star(), quot);
		LexRegex verbatimChar = LexRegex.Parse(@"[^""]|\""\""");
		LexRegex verbatimLiteral = LexRegex.Concat(LexRegex.Symbol('@'), quot, verbatimChar.Star(), quot);
		start.Add(nfa.BuildRegex(LexRegex.Alternate(regularLiteral, verbatimLiteral), 1).Head);
		Dfa dfa = nfa.BuildDFA(1);
		//     | 0 | 1 | 2 | 3 | 4 | 5 |
		// | 0 | 1 |   |   |   | 2 |   |
		// | 1 | 5 | 1 | 6 |   | 1 |   |
		// | 2 | 3 |   |   |   |   |   |
		// | 3 | 4 | 3 | 3 | 3 | 3 | 3 |
		// | 4 | 3 |   |   |   |   |   |
		// | 5 |   |   |   |   |   |   |
		// | 6 | 1 | 1 | 1 | 1 | 1 |   |
		Assert.AreEqual(6, dfa.CharClasses.Count);
		Assert.AreEqual(7, dfa.Count);

		// 检查 DFA 数据。
		DfaData data = dfa.GetData();
		Assert.AreEqual(0, data.States[data.States[0]]);
		Assert.AreEqual(-1, data.States[data.States[0] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(0, data.States[data.States[0] + DfaStateData.SymbolsOffset]);

		Assert.AreEqual(1, data.States[data.States[1]]);
		Assert.AreEqual(-1, data.States[data.States[1] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(0, data.States[data.States[1] + DfaStateData.SymbolsOffset]);

		Assert.AreEqual(6, data.States[data.States[2]]);
		Assert.AreEqual(-1, data.States[data.States[2] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(0, data.States[data.States[2] + DfaStateData.SymbolsOffset]);

		Assert.AreEqual(7, data.States[data.States[3]]);
		Assert.AreEqual(-1, data.States[data.States[3] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(0, data.States[data.States[3] + DfaStateData.SymbolsOffset]);

		Assert.AreEqual(13, data.States[data.States[4]]);
		Assert.AreEqual(-1, data.States[data.States[4] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(1, data.States[data.States[4] + DfaStateData.SymbolsOffset]);
		Assert.AreEqual(1, data.States[data.States[4] + DfaStateData.SymbolsOffset + 1]);

		Assert.AreEqual(int.MinValue, data.States[data.States[5]]);
		Assert.AreEqual(-1, data.States[data.States[5] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(1, data.States[data.States[5] + DfaStateData.SymbolsOffset]);
		Assert.AreEqual(1, data.States[data.States[5] + DfaStateData.SymbolsOffset + 1]);

		Assert.AreEqual(14, data.States[data.States[6]]);
		Assert.AreEqual(-1, data.States[data.States[6] + DfaStateData.DefaultStateOffset]);
		Assert.AreEqual(0, data.States[data.States[6] + DfaStateData.SymbolsOffset]);

		CollectionAssert.AreEqual(new int[] { 1, 5, 1, 6, 2, 1, 3, 4, 3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1 }, data.Next);
		CollectionAssert.AreEqual(new int[] { 0, 1, 1, 1, 0, 1, 2, 3, 3, 3, 3, 3, 3, 4, 6, 6, 6, 6, 6}, data.Check);
	}
}
