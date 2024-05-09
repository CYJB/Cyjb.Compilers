using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于测试可拒绝最短匹配的词法分析器。
/// </summary>
[LexerRejectable]
internal partial class TestShortestLexer3 : LexerController<TestKind>
{
	[LexerSymbol("ab+", Kind = TestKind.A, UseShortest = true)]
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

