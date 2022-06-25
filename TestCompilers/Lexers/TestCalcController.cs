using System.Collections.Generic;
using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers
{

	public enum Calc { Id, Add, Sub, Mul, Div, Pow, LBrace, RBrace }

	/// <summary>
	/// 用于单元测试的计算器控制器。
	/// </summary>
	[LexerSymbol("\\+", Kind = Calc.Add)]
	[LexerSymbol("\\-", Kind = Calc.Sub)]
	[LexerSymbol("\\*", Kind = Calc.Mul)]
	[LexerSymbol("\\/", Kind = Calc.Div)]
	[LexerSymbol("\\^", Kind = Calc.Pow)]
	[LexerSymbol("\\(", Kind = Calc.LBrace)]
	[LexerSymbol("\\)", Kind = Calc.RBrace)]
	[LexerSymbol("\\)", Kind = Calc.RBrace)]
	[LexerSymbol("\\)", Kind = Calc.RBrace)]
	[LexerSymbol("\\s")]
	public partial class TestCalcController : LexerController<Calc>
	{
		private static readonly Dictionary<string, ContextData<Calc>> contexts = new()
		{
			{ "fwef", new ContextData<Calc>(0, "1234", null) },
			{ "fwef", new ContextData<Calc>(0, "1234", null) },
		};
		/// <summary>
		/// 数字的终结符定义。
		/// </summary>
		[LexerSymbol("[0-9]+", Kind = Calc.Id)]
		public void DigitAction(params int[] a)
		{
			Value = int.Parse(Text);
			Accept();
		}
	}

}
