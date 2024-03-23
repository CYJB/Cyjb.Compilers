using System.Text;
using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于单元测试的计算器控制器。
/// </summary>
[LexerSymbol("\\s")]
internal partial class TestCalcRunnerLexer : LexerController<Calc>
{
	private readonly StringBuilder text = new();


	/// <summary>
	/// 数字的终结符定义。
	/// </summary>
	[LexerSymbol("[0-9]+", Kind = Calc.Id)]
	public void DigitAction()
	{
		text.Append(Text.AsSpan());
	}

	/// <summary>
	/// 操作符的动作。
	/// </summary>
	[LexerSymbol("\\+", Kind = Calc.Add)]
	[LexerSymbol("\\-", Kind = Calc.Sub)]
	[LexerSymbol("\\*", Kind = Calc.Mul)]
	[LexerSymbol("\\/", Kind = Calc.Div)]
	[LexerSymbol("\\^", Kind = Calc.Pow)]
	[LexerSymbol("\\(", Kind = Calc.LBrace)]
	[LexerSymbol("\\)", Kind = Calc.RBrace)]
	public void OperatorAction()
	{
		text.Append(Text.AsSpan());
	}

	/// <summary>
	/// 文件结束的动作。
	/// </summary>
	[LexerSymbol("<<EOF>>")]
	public void EofAction()
	{
		((LexerRunnerContext)SharedContext!).Result = text.ToString();
	}

	/// <summary>
	/// 已加载源读取器。
	/// </summary>
	protected override void SourceLoaded()
	{
		base.SourceLoaded();
		text.Clear();
	}
}

