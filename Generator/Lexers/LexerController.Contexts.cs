using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Lexers;

internal sealed partial class LexerController
{
	/// <summary>
	/// 返回指定词法分析器数据的上下文数据。
	/// </summary>
	/// <param name="data">词法分析器数据</param>
	/// <returns>上下文数据。</returns>
	private ExpressionBuilder ContextsValue(LexerData<SymbolKind> data)
	{
		var builder = SyntaxBuilder.CreateObject().InitializerWrap(1);
		// 按照索引顺序生成上下文
		foreach (var pair in data.Contexts.OrderBy(pair => pair.Value.Index))
		{
			var contextBuilder = SyntaxBuilder.CreateObject<ContextData>()
				.Argument(SyntaxBuilder.Literal(pair.Value.Index))
				.Argument(GetContextKey(pair.Value.Label));
			if (pair.Value.EofAction != null)
			{
				contextBuilder.Argument(SyntaxBuilder.Lambda()
					.Parameter("c", SyntaxBuilder.Name(Name))
					.Body(SyntaxBuilder.Name("c").AccessMember(actionMap[pair.Value.EofAction]).Invoke())
				);
			}
			builder.Initializer(SyntaxBuilder.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression)
				.Add(GetContextKey(pair.Key))
				.Add(contextBuilder));
		}
		return builder;
	}

	/// <summary>
	/// 返回表示上下文的键的表达式。
	/// </summary>
	/// <param name="key">上下文的键。</param>
	/// <returns>表示上下文的键的表达式。</returns>
	private static ExpressionBuilder GetContextKey(string key)
	{
		if (key == ContextData.Initial)
		{
			return SyntaxBuilder.Type<ContextData>().AccessMember("Initial");
		}
		else
		{
			return SyntaxBuilder.Literal(key);
		}
	}
}
