using Cyjb.Compilers.Lexers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

public partial class UnitTestLexer
{
	/// <summary>
	/// 测试最短匹配。
	/// </summary>
	[TestMethod]
	public void TestShortest()
	{
		// 简单匹配。
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"ab+").Kind(TestKind.A).UseShortest();
		var factory = lexer.GetFactory();

		var tokenizer = factory.CreateTokenizer("abbbb");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "ab", new TextSpan(0, 2)), tokenizer.Read());

		// 向前看。
		lexer = new();
		lexer.DefineSymbol(@"a.+/b").Kind(TestKind.A).UseShortest();
		factory = lexer.GetFactory();

		tokenizer = factory.CreateTokenizer("abbbb");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "ab", new TextSpan(0, 2)), tokenizer.Read());

		// 可拒绝。
		lexer = new();
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
		factory = lexer.GetFactory(true);

		tokenizer = factory.CreateTokenizer("abbbb");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "abbb", new TextSpan(0, 4)), tokenizer.Read());

		// 向前看+可拒绝。
		lexer = new();
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
		factory = lexer.GetFactory(true);

		tokenizer = factory.CreateTokenizer("abbbb");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "abbb", new TextSpan(0, 4)), tokenizer.Read());
	}
}
