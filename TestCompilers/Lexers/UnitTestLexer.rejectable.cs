using System.Collections.Generic;
using Cyjb.Compilers.Lexers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

public partial class UnitTestLexer
{
	/// <summary>
	/// 测试 Reject。
	/// </summary>
	[TestMethod]
	public void TestRejectable()
	{
		Lexer<TestKind> lexer = new();
		int idx = 0;
		List<HashSet<TestKind>> tests = new()
		{
			new HashSet<TestKind>(){ TestKind.A, TestKind.B, TestKind.C },
			new HashSet<TestKind>(){ TestKind.B, TestKind.C },
			new HashSet<TestKind>(){ TestKind.C },
			new HashSet<TestKind>(),
		};
		lexer.DefineSymbol(@"abc{2,}").Kind(TestKind.A).Action((controller) =>
		{
			if (idx == 0 || idx == 1)
			{
				Assert.IsTrue(tests[idx].SetEquals(controller.Candidates));
				idx++;
				controller.Reject();
			}
			else
			{
				Assert.Fail("错误的进入条件");
			}
		});
		lexer.DefineSymbol(@"abcc").Kind(TestKind.B).Action((controller) =>
		{
			if (idx == 2)
			{
				Assert.IsTrue(tests[idx].SetEquals(controller.Candidates));
				idx++;
				controller.Reject();
			}
			else
			{
				Assert.Fail("错误的进入条件");
			}
		});
		lexer.DefineSymbol(@"abc").Kind(TestKind.C).Action((controller) =>
		{
			if (idx == 3)
			{
				Assert.IsTrue(tests[idx].SetEquals(controller.Candidates));
				idx++;
				controller.Accept();
			}
			else
			{
				Assert.Fail("错误的进入条件");
			}
		});
		var factory = lexer.GetFactory(true);

		var tokenizer = factory.CreateTokenizer("abccc");
		Assert.AreEqual(new Token<TestKind>(TestKind.C, "abc", new TextSpan(0, 3)), tokenizer.Read());
	}

	/// <summary>
	/// 测试 Reject 向前看。
	/// </summary>
	[TestMethod]
	public void TestRejectableTrainling()
	{
		Lexer<TestKind> lexer = new();
		int idx = 0;
		List<HashSet<TestKind>> tests = new()
		{
			new HashSet<TestKind>(){ TestKind.A, TestKind.B, TestKind.C },
			new HashSet<TestKind>(){ TestKind.B, TestKind.C },
			new HashSet<TestKind>(){ TestKind.C },
			new HashSet<TestKind>(),
		};
		lexer.DefineSymbol(@"abc+/c").Kind(TestKind.A).Action((controller) =>
		{
			if (idx == 0 || idx == 1)
			{
				Assert.IsTrue(tests[idx].SetEquals(controller.Candidates));
				idx++;
				controller.Reject();
			}
			else
			{
				Assert.Fail("错误的进入条件");
			}
		});
		lexer.DefineSymbol(@"abcc").Kind(TestKind.B).Action((controller) =>
		{
			if (idx == 2)
			{
				Assert.IsTrue(tests[idx].SetEquals(controller.Candidates));
				idx++;
				controller.Reject();
			}
			else
			{
				Assert.Fail("错误的进入条件");
			}
		});
		lexer.DefineSymbol(@"abc").Kind(TestKind.C).Action((controller) =>
		{
			if (idx == 3)
			{
				Assert.IsTrue(tests[idx].SetEquals(controller.Candidates));
				idx++;
				controller.Accept();
			}
			else
			{
				Assert.Fail("错误的进入条件");
			}
		});
		var factory = lexer.GetFactory(true);

		var tokenizer = factory.CreateTokenizer("abccc");
		Assert.AreEqual(new Token<TestKind>(TestKind.C, "abc", new TextSpan(0, 3)), tokenizer.Read());
	}
}
