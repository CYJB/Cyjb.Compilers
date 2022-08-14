using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// Lambda 表达式的构造器。
/// </summary>
internal sealed class LambdaExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 参数列表构造器。
	/// </summary>
	private readonly ParameterListBuilder parameters = new();
	/// <summary>
	/// Lambda 表达式的内容。
	/// </summary>
	private ExpressionBuilder? expressionBody;

	/// <summary>
	/// 添加 Lambda 表达式的参数。
	/// </summary>
	/// <param name="name">参数的名称。</param>
	/// <returns>当前 Labmda 表达式构造器。</returns>
	public LambdaExpressionBuilder Parameter(string name)
	{
		parameters.Add(name);
		return this;
	}

	/// <summary>
	/// 添加 Lambda 表达式的参数。
	/// </summary>
	/// <param name="name">参数的名称。</param>
	/// <param name="type">参数的类型。</param>
	/// <returns>当前 Labmda 表达式构造器。</returns>
	public LambdaExpressionBuilder Parameter(string name, TypeBuilder type)
	{
		parameters.Add(name, type);
		return this;
	}

	/// <summary>
	/// 设置 Lambda 的表达式体。
	/// </summary>
	/// <param name="expression">Lambda 的表达式体</param>
	/// <returns>当前 Labmda 表达式构造器。</returns>
	public LambdaExpressionBuilder Body(ExpressionBuilder expression)
	{
		expressionBody = expression;
		return this;
	}

	/// <summary>
	/// 构造 Lambda 表达式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>Lambda 表达式。</returns>
	public override LambdaExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		SyntaxToken arrowToken = SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken)
			.WithLeadingTrivia(SyntaxFactory.Space)
			.WithTrailingTrivia(SyntaxFactory.Space);
		if (parameters.IsSimple)
		{
			return SyntaxFactory.SimpleLambdaExpression(
				SyntaxFactory.TokenList(),
				parameters.GetSimpleSyntax(format),
				arrowToken,
				null,
				expressionBody?.GetSyntax(format)
			);
		}
		else
		{
			return SyntaxFactory.ParenthesizedLambdaExpression(
				SyntaxFactory.TokenList(),
				parameters.GetSyntax(format),
				arrowToken,
				null,
				expressionBody?.GetSyntax(format)
			);
		}
	}
}
