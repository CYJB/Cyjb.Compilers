using System.Linq;
using Cyjb.Compilers.Lexers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

public partial class UnitTestLexer
{
	/// <summary>
	/// 测试合并终结符。
	/// </summary>
	[TestMethod]
	public void TestMerge()
	{
		// 简单匹配。
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"a").Kind(TestKind.A);
		lexer.DefineSymbol(@"b").Kind(TestKind.A);
		lexer.DefineSymbol(@"c").Kind(TestKind.A);
		LexerData<TestKind> data = lexer.GetData();

		// 终结符会被合并成一个。
		Assert.AreEqual(1, data.Terminals.Length);
	}
}
