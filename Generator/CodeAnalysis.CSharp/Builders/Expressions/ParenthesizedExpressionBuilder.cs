using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 括号表达式的构造器。
/// </summary>
internal class ParenthesizedExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 括号内的表达式。
	/// </summary>
	private readonly ExpressionBuilder expression;

	/// <summary>
	/// 使用括号内的表达式初始化 <see cref="TypeOfExpressionBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="expression">括号内的表达式。</param>
	public ParenthesizedExpressionBuilder(ExpressionBuilder expression)
	{
		this.expression = expression;
	}

	/// <summary>
	/// 构造括号表达式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>括号表达式。</returns>
	public override ParenthesizedExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.ParenthesizedExpression(expression.GetSyntax(format));
	}
}
