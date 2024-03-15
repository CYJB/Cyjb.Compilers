using System.Collections.Generic;
using Cyjb.Compilers.Lexers;
using Cyjb.Compilers.Parsers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Parsers;

public partial class UnitTestParser
{
	enum TestKind { A, B, C, Ta, Tb, Tc, Td, Te }
	private ILexerFactory<TestKind>? recoverLexerFactory;
	private IParserFactory<TestKind>? recoverParserFactory;

	/// <summary>
	/// 对错误恢复进行测试。
	/// </summary>
	[TestMethod]
	public void TestRecover()
	{
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"a").Kind(TestKind.Ta);
		lexer.DefineSymbol(@"b").Kind(TestKind.Tb);
		lexer.DefineSymbol(@"c").Kind(TestKind.Tc);
		lexer.DefineSymbol(@"d").Kind(TestKind.Td);
		lexer.DefineSymbol(@"e").Kind(TestKind.Te);
		recoverLexerFactory = lexer.GetFactory();

		Parser<TestKind> parser = new();
		parser.DefineProduction(TestKind.A, TestKind.B, TestKind.Ta, TestKind.Tb)
			.Action(c => $"A[{c[0].Value}{c[1].Text}{c[2].Text}]");
		parser.DefineProduction(TestKind.B, TestKind.C, TestKind.Tc, TestKind.Td)
			.Action(c => $"B[{c[0].Value}{c[1].Text}{c[2].Text}]");
		parser.DefineProduction(TestKind.C, TestKind.Te)
			.Action(c => $"C[{c[0].Text}]");
		recoverParserFactory = parser.GetFactory();

		// 测试无错误。
		{
			TestRecover("A[B[C[e]cd]ab]", "ecdab");
		}
		// 测试需要删除一个字符
		{
			Token<TestKind> token = new(TestKind.Te, "e", new TextSpan(1, 2));
			TestRecover("A[B[C[e]cd]ab]", "eecdab",
				new UnexpectedTokenError<TestKind>(token, new HashSet<TestKind>() { TestKind.Tc }));
		}
		{
			Token<TestKind> token = new(TestKind.Td, "d", new TextSpan(1, 2));
			TestRecover("A[B[C[e]cd]ab]", "edcdab",
				new UnexpectedTokenError<TestKind>(token, new HashSet<TestKind>() { TestKind.Tc }));
		}
		{
			Token<TestKind> token = new(TestKind.Td, "d", new TextSpan(0, 1));
			TestRecover("A[B[C[e]cd]ab]", "decdab",
				new UnexpectedTokenError<TestKind>(token, new HashSet<TestKind>() { TestKind.Te }));
		}
		{
			Token<TestKind> token = new(TestKind.Tc, "c", new TextSpan(2, 3));
			TestRecover("A[B[C[e]cd]ab]", "eccdab",
				new UnexpectedTokenError<TestKind>(token, new HashSet<TestKind>() { TestKind.Td }));
		}
		{
			Token<TestKind> token = new(TestKind.Ta, "a", new TextSpan(4, 5));
			TestRecover("A[B[C[e]cd]ab]", "ecdaab",
				new UnexpectedTokenError<TestKind>(token, new HashSet<TestKind>() { TestKind.Tb }));
		}
		{
			Token<TestKind> token = new(TestKind.Tb, "b", new TextSpan(5, 6));
			TestRecover("A[B[C[e]cd]ab]", "ecdabb",
				new UnexpectedTokenError<TestKind>(token, new HashSet<TestKind>() { Token<TestKind>.EndOfFile }));
		}
		{
			Token<TestKind> token1 = new(TestKind.Td, "d", new TextSpan(1, 2));
			Token<TestKind> token2 = new(TestKind.Te, "e", new TextSpan(4, 5));
			TestRecover("A[B[C[e]cd]ab]", "edcdeab",
				new UnexpectedTokenError<TestKind>(token1, new HashSet<TestKind>() { TestKind.Tc }),
				new UnexpectedTokenError<TestKind>(token2, new HashSet<TestKind>() { TestKind.Ta }));
		}
		// 测试需要插入一个字符
		{
			TestRecover("A[B[C[]cd]ab]", "cdab",
				new MissingTokenError<TestKind>(new Token<TestKind>(TestKind.Te, "", new TextSpan(0, 0))));
		}
		{
			TestRecover("A[B[C[e]d]ab]", "edab",
				new MissingTokenError<TestKind>(new Token<TestKind>(TestKind.Tc, "", new TextSpan(1, 1))));
		}
		{
			TestRecover("A[B[C[e]c]ab]", "ecab",
				new MissingTokenError<TestKind>(new Token<TestKind>(TestKind.Td, "", new TextSpan(2, 2))));
		}
		{
			TestRecover("A[B[C[e]cd]b]", "ecdb",
				new MissingTokenError<TestKind>(new Token<TestKind>(TestKind.Ta, "", new TextSpan(3, 3))));
		}
		{
			TestRecover("A[B[C[e]cd]a]", "ecda",
				new MissingTokenError<TestKind>(new Token<TestKind>(TestKind.Tb, "", new TextSpan(4, 4))));
		}
		// 测试恐慌模式
		{
			Token<TestKind> token = new(TestKind.Te, "e", new TextSpan(1, 2));
			TestRecover("A[B[C[e]cd]ab]", "eeecdab",
				new UnexpectedTokenError<TestKind>(token, new HashSet<TestKind>() { TestKind.Tc }));
		}
		{
			Token<TestKind> token1 = new(TestKind.Te, "e", new TextSpan(1, 2));
			Token<TestKind> token2 = new(TestKind.Tc, "c", new TextSpan(4, 5));
			TestRecover("A[B[C[e]cd]ab]", "eeecccccdab",
				new UnexpectedTokenError<TestKind>(token1, new HashSet<TestKind>() { TestKind.Tc }),
				new UnexpectedTokenError<TestKind>(token2, new HashSet<TestKind>() { TestKind.Td }));
		}
	}

	private void TestRecover(string expected, string input, params TokenParseError[] errors)
	{
		LexerTokenizer<TestKind> tokenizer = recoverLexerFactory!.CreateTokenizer();
		tokenizer.Load(input);

		var tokenParser = recoverParserFactory!.CreateParser();
		tokenParser.Load(tokenizer);
		int index = 0;
		tokenParser.ParseError += (_, error) =>
		{
			if (index >= errors.Length)
			{
				Assert.Fail($"Unexpected error: {error}");
			}
			else
			{
				Assert.AreEqual(errors[index], error);
				index++;
			}
		};
		Assert.AreEqual(expected, tokenParser.Parse().Value);
		if (index != errors.Length)
		{
			Assert.Fail($"Untriggered error: {errors[index]}");
		}
	}
}
