using Cyjb.Compilers.Lexers;

namespace Cyjb.Compilers.Parsers.Production;

/// <summary>
/// 产生式词法分析器。
/// </summary>
[LexerSymbol(@"\(", Kind = ProductionKind.LBrace)]
[LexerSymbol(@"\)", Kind = ProductionKind.RBrace)]
[LexerSymbol(@"\+", Kind = ProductionKind.Plus)]
[LexerSymbol(@"\*", Kind = ProductionKind.Star)]
[LexerSymbol(@"\?", Kind = ProductionKind.Question)]
[LexerSymbol(@"\s")]
internal partial class ProductionLexer : LexerController<ProductionKind>
{
	/// <summary>
	/// 符号定义。
	/// </summary>
	[LexerSymbol(@"\d+|\w[\w\d]*", Kind = ProductionKind.Id)]
	private void SymbolAction()
	{
		Accept(Enum.Parse((SharedContext as Type)!, Text));
	}
}

