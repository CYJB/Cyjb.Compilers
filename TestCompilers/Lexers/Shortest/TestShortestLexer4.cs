using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于测试向前看+可拒绝最短匹配的词法分析器。
/// </summary>
[LexerRejectable]
internal partial class TestShortestLexer4 : LexerController<TestKind>
{
	[LexerSymbol("a.+/b", Kind = TestKind.A, UseShortest = true)]
	public void TestAction()
	{
		if (Text.Length < 4)
		{
			Reject();
		}
		else
		{
			Accept();
		}
	}
}

