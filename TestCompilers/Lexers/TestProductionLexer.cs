using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于单元测试的计算器控制器。
/// </summary>
[LexerSymbol(@"\d+|\w[\w\d]*", Kind = ProductionKind.Id)]
[LexerSymbol(@"\(", Kind = ProductionKind.LBrace)]
[LexerSymbol(@"\)", Kind = ProductionKind.RBrace)]
[LexerSymbol(@"\+", Kind = ProductionKind.Plus)]
[LexerSymbol(@"\*", Kind = ProductionKind.Star)]
[LexerSymbol(@"\?", Kind = ProductionKind.Question)]
[LexerSymbol(@"\s")]
public partial class TestProductionLexer : LexerController<ProductionKind>
{ }

