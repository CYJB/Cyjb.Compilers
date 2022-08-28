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
	public ExpressionBuilder ContextsValue(LexerData<int> data)
	{
		TypeBuilder contextType = $"ContextData<{kindType}>";
		var builder = SyntaxBuilder.ObjectCreationExpression().InitializerWrap(1);
		// 按照索引顺序生成上下文
		foreach (var pair in data.Contexts.OrderBy(pair => pair.Value.Index))
		{
			var contextBuilder = SyntaxBuilder.ObjectCreationExpression()
				.Type(contextType)
				.Argument(SyntaxBuilder.LiteralExpression(pair.Value.Index))
				.Argument(GetContextKey(pair.Value.Label));
			if (pair.Value.EofAction != null)
			{
				contextBuilder.Argument(SyntaxBuilder.LambdaExpression()
					.Parameter("c", controllerType)
					.Body(SyntaxBuilder.IdentifierName("c").Access(actionMap[pair.Value.EofAction]).Invoke())
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
			return SyntaxBuilder.IdentifierName("ContextData").Access("Initial");
		}
		else
		{
			return SyntaxBuilder.LiteralExpression(key);
		}
	}
}
