using Cyjb.Compilers.Parsers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Parsers;

public partial class UnitTestParser
{
	/// <summary>
	/// 对取消语法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestCancel()
	{
		ITokenizer<TestKind> tokens = new EnumerableTokenizer<TestKind>(new Token<TestKind>[]
		{
			new(TestKind.Te, "e"),
			new(TestKind.Te, "e"),
			new(TestKind.Tc, "c"),
			new(TestKind.Td, "d"),
			new(TestKind.Ta, "a"),
			new(TestKind.Tb, "b"),
		});

		Parser<TestKind> parser = new();
		parser.DefineProduction(TestKind.A, TestKind.B, TestKind.Ta, TestKind.Tb)
			.Action(c => $"A[{c[0].Value}{c[1].Text}{c[2].Text}]");
		parser.DefineProduction(TestKind.B, TestKind.C, TestKind.Tc, TestKind.Td)
			.Action(c => $"B[{c[0].Value}{c[1].Text}{c[2].Text}]");
		parser.DefineProduction(TestKind.C, TestKind.Te)
			.Action(c => $"C[{c[0].Text}]");
		IParserFactory<TestKind> factory = parser.GetFactory();

		LRParser<TestKind> tokenParser = factory.CreateParser();
		tokenParser.Load(tokens);
		tokenParser.ParseError += (parser, error) =>
		{
			parser.Cancel();
		};
		Assert.AreEqual(ParseStatus.Ready, tokenParser.Status);
		Assert.AreEqual("A[B[C[e]]]", tokenParser.Parse().Value);
		Assert.AreEqual(ParseStatus.Cancelled, tokenParser.Status);
	}
}
