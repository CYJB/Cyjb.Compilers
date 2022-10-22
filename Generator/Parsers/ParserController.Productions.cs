using Cyjb.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Parsers;

internal sealed partial class ParserController
{
	/// <summary>
	/// 返回指定语法分析器数据的产生式数据。
	/// </summary>
	/// <param name="data">语法分析器数据</param>
	/// <param name="actionMap">动作的映射。</param>
	/// <returns>产生式表达式。</returns>
	private ExpressionBuilder Productions(ParserData<SymbolKind> data, Dictionary<Delegate, string> actionMap)
	{
		TypeBuilder productionType = SyntaxBuilder.Type($"ProductionData<{KindType}>");
		var builder = SyntaxBuilder.CreateArray().InitializerWrap(1);
		foreach (ProductionData<SymbolKind> production in data.Productions)
		{
			var productionBuilder = SyntaxBuilder.CreateObject()
				.Type(productionType).ArgumentWrap(1);
			builder.Initializer(productionBuilder);
			productionBuilder.Argument(production.Head.Syntax);
			if (production.Action == null)
			{
				productionBuilder.Argument(SyntaxBuilder.Literal(null));
			}
			else
			{
				var action = SyntaxBuilder.Lambda()
					.Parameter("c", SyntaxBuilder.Name(Name))
					.Body(SyntaxBuilder.Name("c").AccessMember(actionMap[production.Action]).Invoke());
				productionBuilder.Argument(action);
			}
			foreach (SymbolKind kind in production.Body)
			{
				productionBuilder.Argument(kind.Syntax);
			}
		}
		return builder;
	}
}

