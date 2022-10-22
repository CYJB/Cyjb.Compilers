using Cyjb.Compilers.Parsers;

namespace TestCompilers.Parsers;

/// <summary>
/// 用于单元测试的产生式语法分析。
/// </summary>
internal partial class TestProductionParser : ParserController<ProductionKind>
{
	[ParserProduction(ProductionKind.Expression, ProductionKind.Expression, ProductionKind.Repeat)]
	private object? ExpressionAction()
	{
		return $"{this[0].Value} {this[1].Value}";
	}

	[ParserProduction(ProductionKind.Item, ProductionKind.Id)]
	private object? CopyTextAction()
	{
		return this[0].Text;
	}

	[ParserProduction(ProductionKind.Expression, ProductionKind.Repeat)]
	[ParserProduction(ProductionKind.Repeat, ProductionKind.Item)]
	private object? CopyValueAction()
	{
		return this[0].Value;
	}

	[ParserProduction(ProductionKind.Item, ProductionKind.LBrace, ProductionKind.Expression, ProductionKind.RBrace)]
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

