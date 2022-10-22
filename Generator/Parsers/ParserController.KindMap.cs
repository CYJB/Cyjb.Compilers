using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Parsers;

internal sealed partial class ParserController
{
	/// <summary>
	/// 返回指定符号类别字典的数据。
	/// </summary>
	/// <param name="map">符号类别字典。</param>
	/// <returns>符号类别字典的数据。</returns>
	private static ExpressionBuilder KindMap(Dictionary<SymbolKind, int> map)
	{
		var builder = SyntaxBuilder.CreateObject().InitializerWrap(1);
		// 按照索引顺序生成起始状态
		foreach (var (key, value) in map.OrderBy(pair => pair.Key.Index))
		{
			builder.Initializer(SyntaxBuilder.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression)
				.Add(key.Syntax)
				.Add(SyntaxBuilder.Literal(value)));
		}
		return builder;
	}
}

