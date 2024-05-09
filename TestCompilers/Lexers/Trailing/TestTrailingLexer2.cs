using System.Text.RegularExpressions;
using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于测试后面固定长度向前看的词法分析器。
/// </summary>
[LexerSymbol("a+/b", Kind = TestKind.A)]
[LexerSymbol(".", RegexOptions.Singleline, Kind = TestKind.B)]
internal partial class TestTrailingLexer2 : LexerController<TestKind>
{
}

