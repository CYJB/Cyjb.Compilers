using System.Collections.Generic;
using Cyjb.Compilers.Parsers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Parsers;

public partial class UnitTestParser
{
	/// <summary>
	/// 对语法分析选项进行测试。
	/// </summary>
	[TestMethod]
	public void TestOptionScanToEOF()
	{
		Parser<TestKind> parser = new();
		parser.DefineProduction(TestKind.A, TestKind.A, TestKind.Tb)
			.Action(c => $"{c[0].Value}(b)");
		parser.DefineProduction(TestKind.A, TestKind.Ta)
			.Action(c => "(a)");
		parser.DefineProduction(TestKind.A)
			.Action(c => "()");
		IParserFactory<TestKind> factory = parser.GetFactory();

		ITokenParser<TestKind> tokenParser = factory.CreateParser(GetTestOptionTokenizer());
		int errorCount = 0;
		tokenParser.ParseError += (parser, error) =>
		{
			Token<TestKind> token = new(TestKind.Ta, "a");
			HashSet<TestKind> expectedKind = new() { TestKind.Tb, Token<TestKind>.EndOfFile };
			TokenParseError expectedError = (new UnexpectedTokenError<TestKind>(token, expectedKind));
			Assert.AreEqual(expectedError, error);
			errorCount++;
			Assert.AreEqual(1, errorCount);
		};
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);
		Assert.AreEqual("(a)(b)(b)", tokenParser.Parse().Value);
		Assert.AreEqual(ParseStatus.Finished, tokenParser.Status);
	}

	/// <summary>
	/// 对语法分析选项进行测试。
	/// </summary>
	[TestMethod]
	public void TestOptionScanToMatch()
	{
		Parser<TestKind> parser = new();
		parser.DefineProduction(TestKind.A, TestKind.A, TestKind.Tb)
			.Action(c => $"{c[0].Value}(b)");
		parser.DefineProduction(TestKind.A, TestKind.Ta)
			.Action(c => "(a)");
		parser.DefineProduction(TestKind.A)
			.Action(c => "()");
		parser.AddStart(TestKind.A, ParseOptions.ScanToMatch);
		IParserFactory<TestKind> factory = parser.GetFactory();

		ITokenParser<TestKind> tokenParser = factory.CreateParser(GetTestOptionTokenizer());
		tokenParser.ParseError += (parser, error) =>
		{
			Assert.Fail("不应产生分析错误");
		};
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);
		Assert.AreEqual("(a)(b)", tokenParser.Parse().Value);
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);

		Assert.AreEqual("(a)(b)", tokenParser.Parse().Value);
		Assert.AreEqual(ParseStatus.Finished, tokenParser.Status);
	}

	/// <summary>
	/// 对语法分析选项进行测试。
	/// </summary>
	[TestMethod]
	public void TestOptionScanToFirstMatch()
	{
		Parser<TestKind> parser = new();
		parser.DefineProduction(TestKind.A, TestKind.A, TestKind.Tb)
			.Action(c => $"{c[0].Value}(b)");
		parser.DefineProduction(TestKind.A, TestKind.Ta)
			.Action(c => "(a)");
		parser.DefineProduction(TestKind.A)
			.Action(c => "()");
		parser.AddStart(TestKind.A, ParseOptions.ScanToFirstMatch);
		IParserFactory<TestKind> factory = parser.GetFactory();

		ITokenParser<TestKind> tokenParser = factory.CreateParser(GetTestOptionTokenizer());
		tokenParser.ParseError += (parser, error) =>
		{
			Assert.Fail("不应该出发 ParseError");
		};
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);
		Assert.AreEqual("(a)", tokenParser.Parse().Value);
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);

		Assert.AreEqual("()", tokenParser.Parse().Value);
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);

		Assert.AreEqual("()", tokenParser.Parse().Value);
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);
	}

	private static ITokenizer<TestKind> GetTestOptionTokenizer()
	{
		return new EnumerableTokenizer<TestKind>(new Token<TestKind>[]
		{
			new Token<TestKind>(TestKind.Ta, "a"),
			new Token<TestKind>(TestKind.Tb, "b"),
			new Token<TestKind>(TestKind.Ta, "a"),
			new Token<TestKind>(TestKind.Tb, "b"),
		});
	}
}
