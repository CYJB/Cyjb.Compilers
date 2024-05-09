using Cyjb.Compilers.Lexers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

/// <summary>
/// 带有最短匹配的词法分析单元测试。
/// </summary>
[TestClass]
public class UnitTestShortestLexer
{
	/// <summary>
	/// 测试简单最短匹配。
	/// </summary>
	[TestMethod]
	public void TestShortest1()
	{
		// 简单匹配。
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"ab+").Kind(TestKind.A).UseShortest();
		TestShotest1Factory(lexer.GetFactory());
	}
	[TestMethod]
	public void TestInDesignShortest1()
	{
		TestShotest1Factory(TestShortestLexer1.Factory);
	}
	/// <summary>
	/// 测试简单最短匹配词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestShotest1Factory(ILexerFactory<TestKind> factory)
	{
		var tokenizer = factory.CreateTokenizer();
		tokenizer.Load("abbbb");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "ab", new TextSpan(0, 2)), tokenizer.Read());
	}

	/// <summary>
	/// 测试向前看最短匹配。
	/// </summary>
	[TestMethod]
	public void TestShortest2()
	{
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"a.+/b").Kind(TestKind.A).UseShortest();
		TestShotest2Factory(lexer.GetFactory());
	}
	[TestMethod]
	public void TestInDesignShortest2()
	{
		TestShotest2Factory(TestShortestLexer2.Factory);
	}
	/// <summary>
	/// 测试向前看最短匹配词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestShotest2Factory(ILexerFactory<TestKind> factory)
	{
		var tokenizer = factory.CreateTokenizer();
		tokenizer.Load("abbbb");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "ab", new TextSpan(0, 2)), tokenizer.Read());
	}

	/// <summary>
	/// 测试可拒绝最短匹配。
	/// </summary>
	[TestMethod]
	public void TestShortest3()
	{
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"ab+").Kind(TestKind.A).UseShortest().Action(c =>
		{
			if (c.Text.Length < 4)
			{
				c.Reject();
			}
			else
			{
				c.Accept();
			}
		});
		TestShotest3Factory(lexer.GetFactory(true));
	}
	[TestMethod]
	public void TestInDesignShortest3()
	{
		TestShotest3Factory(TestShortestLexer3.Factory);
	}
	/// <summary>
	/// 测试可拒绝最短匹配词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestShotest3Factory(ILexerFactory<TestKind> factory)
	{
		var tokenizer = factory.CreateTokenizer();
		tokenizer.Load("abbbb");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "abbb", new TextSpan(0, 4)), tokenizer.Read());
	}

	/// <summary>
	/// 测试向前看+可拒绝最短匹配。
	/// </summary>
	[TestMethod]
	public void TestShortest4()
	{
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"a.+/b").Kind(TestKind.A).UseShortest().Action(c =>
		{
			if (c.Text.Length < 4)
			{
				c.Reject();
			}
			else
			{
				c.Accept();
			}
		});
		TestShotest4Factory(lexer.GetFactory(true));
	}
	[TestMethod]
	public void TestInDesignShortest4()
	{
		TestShotest4Factory(TestShortestLexer4.Factory);
	}
	/// <summary>
	/// 测试向前看+可拒绝最短匹配词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestShotest4Factory(ILexerFactory<TestKind> factory)
	{
		var tokenizer = factory.CreateTokenizer();
		tokenizer.Load("abbbb");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "abbb", new TextSpan(0, 4)), tokenizer.Read());
	}
}
