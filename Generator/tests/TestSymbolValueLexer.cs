using Cyjb.Compilers.Lexers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

/// <summary>
/// 测试词法单元的值。
/// </summary>
public partial class TestSymbolValueLexer : LexerController<Calc>
{
	[LexerSymbol("[0-9]+", Kind = Calc.Id, Value = 101)]
	public void DigitAction()
	{
		Assert.AreEqual(101, Value);
		Accept((int)Value! + 10);
	}
}

