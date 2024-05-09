using System.Text.RegularExpressions;
using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于测试向前看行尾的词法分析器。
/// </summary>
[LexerSymbol("a$", Kind = TestKind.A)]
[LexerSymbol(".", RegexOptions.Singleline, Kind = TestKind.B)]
internal partial class TestTrailingLexerEOL : LexerController<TestKind>
{
}

