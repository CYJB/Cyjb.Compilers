using System.Text.RegularExpressions;
using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于测试前面固定长度向前看的词法分析器。
/// </summary>
[LexerSymbol("ab/c", Kind = TestKind.A)]
[LexerSymbol(".", RegexOptions.Singleline, Kind = TestKind.B)]
internal partial class TestTrailingLexer1 : LexerController<TestKind>
{
}

