using Cyjb.Compilers.Lexers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

/// <summary>
/// <see cref="LexerRunner{T}"/> 类的单元测试。
/// </summary>
[TestClass]
public partial class UnitTestLexerRunner
{
	/// <summary>
	/// 对计算器词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestDefineCalc()
	{
		LexerRunner<Calc> runner = TestCalcRunnerLexer.Factory.CreateRunner();
		LexerRunnerContext context = new();
		runner.SharedContext = context;

		runner.Parse("1 + 20 * 3 / 4*(5+6)");
		Assert.AreEqual("1+20*3/4*(5+6)", context.Result);
	}
}
