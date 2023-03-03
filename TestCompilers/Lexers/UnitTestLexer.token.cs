using Cyjb.Compilers.Lexers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

public partial class UnitTestLexer
{
	/// <summary>
	/// 测试文本是可变的。
	/// </summary>
	[TestMethod]
	public void TestMutableText()
	{
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"a").Kind(TestKind.A).Action(c =>
		{
			// 主动修改的 Text，不会跟着 Source.Index 发生变更。
			c.Text = "XXX";
			c.Source.Index++;
			Assert.AreEqual("XXX", c.Text);
			c.Accept();
		});
		lexer.DefineSymbol(@"b").Kind(TestKind.B).Action(c =>
		{
			// 修改 Source.Index 时，Text 会跟着发生变更。
			Assert.AreEqual("b", c.Text);
			c.Source.Index++;
			Assert.AreEqual("bc", c.Text);
			c.Accept();
		});
		var factory = lexer.GetFactory();

		var tokenizer = factory.CreateTokenizer("axbcd");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "XXX", new TextSpan(0, 2)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "bc", new TextSpan(2, 4)), tokenizer.Read());
	}
}
