using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 产生式的类型。
/// </summary>
public enum ProductionKind
{
	/// <summary>
	/// 词法单元类型。
	/// </summary>
	Id,
	/// <summary>
	/// 左小括号。
	/// </summary>
	LBrace,
	/// <summary>
	/// 右小括号。
	/// </summary>
	RBrace,
	/// <summary>
	/// 星号
	/// </summary>
	Star,
	/// <summary>
	/// 加号
	/// </summary>
	Plus,
	/// <summary>
	/// 问号
	/// </summary>
	Question,
}

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
public partial class TestProductionController : LexerController<ProductionKind>
{ }

