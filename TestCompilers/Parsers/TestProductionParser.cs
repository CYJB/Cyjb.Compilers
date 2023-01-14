using System.Collections;
using System.Linq;
using Cyjb.Compilers.Parsers;

namespace TestCompilers.Parsers;

/// <summary>
/// 用于单元测试的产生式语法分析。
/// </summary>
internal partial class TestProductionParser : ParserController<ProductionKind>
{
	[ParserProduction(ProductionKind.AltExp, ProductionKind.AltExp, ProductionKind.Or, ProductionKind.Exp)]
	private object? OrAction()
	{
		return $"({this[0].Value})|({this[2].Value})";
	}

	[ParserProduction(ProductionKind.Exp, ProductionKind.Repeat, SymbolOptions.OneOrMore)]
	private object? ExpressionAction()
	{
		return string.Join(" ", ((IList)this[0].Value!).Cast<string>());
	}

	[ParserProduction(ProductionKind.Item, ProductionKind.Id)]
	private object? CopyTextAction()
	{
		return this[0].Text;
	}

	[ParserProduction(ProductionKind.AltExp, ProductionKind.Exp)]
	[ParserProduction(ProductionKind.Repeat, ProductionKind.Item)]
	private object? CopyValueAction()
	{
		return this[0].Value;
	}

	[ParserProduction(ProductionKind.Item, ProductionKind.LBrace, ProductionKind.AltExp, ProductionKind.RBrace)]
	private object? BraceAction()
	{
		return $"({this[1].Value})";
	}

	[ParserProduction(ProductionKind.Repeat, ProductionKind.Item, ProductionKind.Plus)]
	[ParserProduction(ProductionKind.Repeat, ProductionKind.Item, ProductionKind.Star)]
	[ParserProduction(ProductionKind.Repeat, ProductionKind.Item, ProductionKind.Question)]
	private object? RepeatAction()
	{
		return $"{this[0].Value}{this[1].Text}";
	}
}

