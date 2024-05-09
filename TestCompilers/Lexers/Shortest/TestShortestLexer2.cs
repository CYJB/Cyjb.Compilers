using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于测试向前看最短匹配的词法分析器。
/// </summary>
[LexerSymbol("a.+/b", Kind = TestKind.A, UseShortest = true)]
internal partial class TestShortestLexer2 : LexerController<TestKind>
{
}

