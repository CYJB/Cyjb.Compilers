using System;
using Cyjb;
using Cyjb.Compilers.Parsers;

namespace TestCompilers.Parsers;

/// <summary>
/// 用于单元测试的计算器控制器。
/// </summary>
[ParserLeftAssociate(Calc.Add, Calc.Sub)]
[ParserLeftAssociate(Calc.Mul, Calc.Div)]
[ParserRightAssociate(Calc.Pow)]
[ParserNonAssociate(Calc.Id)]
public partial class TestCalcParser : ParserController<Calc>
{
	[ParserProduction(Calc.E, Calc.Id)]
	private object? IdAction()
	{
		return this[0].Value;
	}

	[ParserProduction(Calc.E, Calc.E, Calc.Add, Calc.E)]
	[ParserProduction(Calc.E, Calc.E, Calc.Sub, Calc.E)]
	[ParserProduction(Calc.E, Calc.E, Calc.Mul, Calc.E)]
	[ParserProduction(Calc.E, Calc.E, Calc.Div, Calc.E)]
	[ParserProduction(Calc.E, Calc.E, Calc.Pow, Calc.E)]
	private object? BinaryAction()
	{
		double left = (double)this[0].Value!;
		double right = (double)this[2].Value!;
		return this[1].Kind switch
		{
			Calc.Add => left + right,
			Calc.Sub => left - right,
			Calc.Mul => left * right,
			Calc.Div => left / right,
			Calc.Pow => Math.Pow(left, right),
			_ => throw CommonExceptions.Unreachable(),
		};
	}

	[ParserProduction(Calc.E, Calc.LBrace, Calc.E, Calc.RBrace)]
	private object? BraceAction()
	{
		return this[1].Value;
	}
}
