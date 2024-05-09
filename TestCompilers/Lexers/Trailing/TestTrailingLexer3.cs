using System.Text.RegularExpressions;
using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于测试非固定长度向前看的词法分析器。
/// </summary>
[LexerSymbol("a+/b*c", Kind = TestKind.A)]
[LexerSymbol(".", RegexOptions.Singleline, Kind = TestKind.B)]
internal partial class TestTrailingLexer3 : LexerController<TestKind>
{
}

