using Cyjb.Compilers.Lexers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

public partial class UnitTestLexer
{
	/// <summary>
	/// 对取消词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestCancel()
	{
		LexerTokenizer<Calc> tokenizer = TestCalcLexer.Factory.CreateTokenizer();
		tokenizer.Load("1 + 20 * 3 / 4*(5+6)");

		Assert.AreEqual(ParseStatus.Ready, tokenizer.Status);
		Assert.AreEqual(new Token<Calc>(Calc.Id, "1", new TextSpan(0, 1), 1), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Add, "+", new TextSpan(2, 3)), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "20", new TextSpan(4, 6), 20), tokenizer.Read());
		tokenizer.Cancel();
		Assert.AreEqual(ParseStatus.Cancelled, tokenizer.Status);
		Assert.AreEqual(Token<Calc>.GetEndOfFile(6), tokenizer.Read());
		Assert.AreEqual(Token<Calc>.GetEndOfFile(6), tokenizer.Read());
	}
}
