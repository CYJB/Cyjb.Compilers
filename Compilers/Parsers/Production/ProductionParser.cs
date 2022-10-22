namespace Cyjb.Compilers.Parsers.Production;

/// <summary>
/// 产生式语法分析器。
/// </summary>
internal partial class ProductionParser : ParserController<ProductionKind>
{
	[ParserProduction(ProductionKind.Expression, ProductionKind.Expression, ProductionKind.Repeat)]
	private object? ExpressionAction()
	{
		ProductionNode node = (this[0].Value as ProductionNode)!;
		if (node.RepeatKind != ProductionKind.Id)
		{
			node = new ProductionNode(node);
		}
		node.Symbols.Add(this[1].Value!);
		return node;
	}

	[ParserProduction(ProductionKind.Expression, ProductionKind.Repeat)]
	[ParserProduction(ProductionKind.Repeat, ProductionKind.Item)]
	[ParserProduction(ProductionKind.Item, ProductionKind.Id)]
	private object? CopyAction()
	{
		object? value = this[0].Value;
		if (value is ProductionNode)
		{
			return value;
		}
		else
		{
			return new ProductionNode(value!);
		}
	}

	[ParserProduction(ProductionKind.Item, ProductionKind.LBrace, ProductionKind.Expression, ProductionKind.RBrace)]
	private object? BraceAction()
	{
		return new ProductionNode(this[1].Value!);
	}

	[ParserProduction(ProductionKind.Repeat, ProductionKind.Item, ProductionKind.Plus)]
	[ParserProduction(ProductionKind.Repeat, ProductionKind.Item, ProductionKind.Star)]
	[ParserProduction(ProductionKind.Repeat, ProductionKind.Item, ProductionKind.Question)]
	private object? RepeatAction()
	{
		ProductionNode node = (this[0].Value as ProductionNode)!;
		node.RepeatKind = this[1].Kind;
		return node;
	}
}

