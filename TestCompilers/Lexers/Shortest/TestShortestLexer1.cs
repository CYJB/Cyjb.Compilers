using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于测试简单最短匹配的词法分析器。
/// </summary>
[LexerSymbol("ab+", Kind = TestKind.A, UseShortest = true)]
internal partial class TestShortestLexer1 : LexerController<TestKind>
{
}

