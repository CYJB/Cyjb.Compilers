using Cyjb.Compilers.Parsers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Parsers;

public partial class UnitTestParser
{
	/// <summary>
	/// 对语法分析多起始符号进行测试。
	/// </summary>
	[TestMethod]
	public void TestStart()
	{
		ITokenizer<TestKind> tokens = new EnumerableTokenizer<TestKind>(new Token<TestKind>[]
		{
			new(TestKind.Te, "e"),
			new(TestKind.Tc, "c"),
			new(TestKind.Td, "d"),
			new(TestKind.Ta, "a"),
			new(TestKind.Tb, "b"),
			new(TestKind.Te, "e"),
			new(TestKind.Te, "e"),
			new(TestKind.Tc, "c"),
			new(TestKind.Td, "d"),
		});

		Parser<TestKind> parser = new();
		parser.DefineProduction(TestKind.A, TestKind.B, TestKind.Ta, TestKind.Tb)
			.Action(c => $"A[{c[0].Value}{c[1].Text}{c[2].Text}]");
		parser.DefineProduction(TestKind.B, TestKind.C, TestKind.Tc, TestKind.Td)
			.Action(c => $"B[{c[0].Value}{c[1].Text}{c[2].Text}]");
		parser.DefineProduction(TestKind.C, TestKind.Te)
			.Action(c => $"C[{c[0].Text}]");
		parser.AddStart(TestKind.A, ParseOptions.ScanToMatch);
		parser.AddStart(TestKind.B, ParseOptions.ScanToMatch);
		parser.AddStart(TestKind.C, ParseOptions.ScanToMatch);
		IParserFactory<TestKind> factory = parser.GetFactory();

		var tokenParser = factory.CreateParser();
		tokenParser.Load(tokens);
		tokenParser.ParseError += (parser, error) =>
		{
			Assert.Fail("不应产生分析错误");
		};
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);
		Assert.AreEqual("A[B[C[e]cd]ab]", tokenParser.Parse().Value);
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);
		Assert.AreEqual("C[e]", tokenParser.Parse(TestKind.C).Value);
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);
		Assert.AreEqual("B[C[e]cd]", tokenParser.Parse(TestKind.B).Value);
		Assert.AreEqual(ParseStatus.Finished, tokenParser.Status);
	}
}
