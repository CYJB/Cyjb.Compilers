using System.Text.RegularExpressions;
using Cyjb.Compilers.Lexers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

public partial class UnitTestLexer
{
	/// <summary>
	/// 测试向前看。
	/// </summary>
	[TestMethod]
	public void TestTrailing()
	{
		// 前面固定长度
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"ab/c").Kind(TestKind.A);
		lexer.DefineSymbol(@".", RegexOptions.Singleline).Kind(TestKind.B);
		var factory = lexer.GetFactory();

		var tokenizer = factory.CreateTokenizer();
		tokenizer.Load("ab");
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(2), tokenizer.Read());

		tokenizer.Load("abc");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "ab", new TextSpan(0, 2)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "c", new TextSpan(2, 3)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(3), tokenizer.Read());

		// 后面固定长度
		lexer = new();
		lexer.DefineSymbol(@"a+/b").Kind(TestKind.A);
		lexer.DefineSymbol(@".", RegexOptions.Singleline).Kind(TestKind.B);
		factory = lexer.GetFactory();

		tokenizer = factory.CreateTokenizer();
		tokenizer.Load("ab");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(2), tokenizer.Read());

		tokenizer.Load("ac");
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "c", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(2), tokenizer.Read());

		// 都不固定长度
		lexer = new();
		lexer.DefineSymbol(@"a+/b*c").Kind(TestKind.A);
		lexer.DefineSymbol(@".", RegexOptions.Singleline).Kind(TestKind.B);
		factory = lexer.GetFactory();

		tokenizer = factory.CreateTokenizer();
		tokenizer.Load("aabbc");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "aa", new TextSpan(0, 2)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(2, 3)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(3, 4)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "c", new TextSpan(4, 5)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(5), tokenizer.Read());

		tokenizer.Load("aabb");
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "a", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(2, 3)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(3, 4)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(4), tokenizer.Read());
	}

	/// <summary>
	/// 测试向前看行尾。
	/// </summary>
	[TestMethod]
	public void TestTrailingEOL()
	{
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"a$").Kind(TestKind.A);
		lexer.DefineSymbol(@".", RegexOptions.Singleline).Kind(TestKind.B);
		var factory = lexer.GetFactory();

		var tokenizer = factory.CreateTokenizer();
		tokenizer.Load("ab");
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(2), tokenizer.Read());

		// 匹配 \n。
		tokenizer.Load("a\n");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "\n", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(2), tokenizer.Read());

		// 匹配 \r\n
		tokenizer.Load("a\r\n");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "\r", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "\n", new TextSpan(2, 3)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(3), tokenizer.Read());

		// 匹配 EOF
		tokenizer.Load("a");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(1), tokenizer.Read());
	}
}
