using System;
using Cyjb.Compilers.Lexers;
using Cyjb.Compilers.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Compile.Lexers;

/// <summary>
/// <see cref="NFA"/> 类的单元测试。
/// </summary>
[TestClass]
public class UnitTestNFA
{
	/// <summary>
	/// 对 <see cref="NFA.BuildDFA"/> 方法进行测试。
	/// </summary>
	[TestMethod]
	public void TestBuildDFA()
	{
		NFA nfa = new();
		NFAState start = nfa.NewState();
		start.Add(nfa.BuildRegex(LexRegex.Parse("(a|b)*baa"), 4).Head);
		DFA dfa = nfa.BuildDFA(1);
		// dfa 参见 https://www.cnblogs.com/cyjb/archive/2013/05/02/LexerDfa.html DFA 最小化 (b) 部分
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
	}
}
