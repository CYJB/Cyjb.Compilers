using Cyjb.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Parsers;

internal sealed partial class ParserController
{
	/// <summary>
	/// 声明指定语法分析器数据的产生式数据。
	/// </summary>
	/// <param name="data">语法分析器数据</param>
	/// <returns>产生式数据声明语句。</returns>
	private LocalDeclarationStatementBuilder DeclareProductions(ParserData<SymbolKind> data)
	{
		TypeBuilder productionType = SyntaxBuilder.Name(typeof(ProductionData<>)).TypeArgument(KindType);
		var builder = SyntaxBuilder.CreateArray().InitializerWrap(1);
		foreach (ProductionData<SymbolKind> production in data.Productions)
		{
			var productionBuilder = SyntaxBuilder.CreateObject(productionType).ArgumentWrap(1);
			builder.Initializer(productionBuilder);
			productionBuilder.Argument(production.Head.Syntax);
			if (production.Action == null)
			{
				productionBuilder.Argument(SyntaxBuilder.Literal(null));
			}
			else if (production.Action == ProductionAction.Optional)
			{
				productionBuilder.Argument(SyntaxBuilder.Type<ProductionAction>().AccessMember("Optional"));
			}
			else if (production.Action == ProductionAction.More)
			{
				productionBuilder.Argument(SyntaxBuilder.Type<ProductionAction>().AccessMember("More"));
			}
			else
			{
				var action = SyntaxBuilder.Lambda()
					.Parameter("c", Name)
					.Body(SyntaxBuilder.Name("c").AccessMember(actionMap[production.Action]).Invoke());
				productionBuilder.Argument(action);
			}
			foreach (SymbolKind kind in production.Body)
			{
				productionBuilder.Argument(kind.Syntax);
			}
		}
		return SyntaxBuilder.DeclareLocal(productionType.Clone().Array(), "productions")
			.Comment("产生式数据").Value(builder);
	}
}

