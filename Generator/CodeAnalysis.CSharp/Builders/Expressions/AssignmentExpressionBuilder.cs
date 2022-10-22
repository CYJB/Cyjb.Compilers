using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 赋值表达式的构造器。
/// </summary>
internal class AssignmentExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 赋值语句的类型。
	/// </summary>
	private readonly SyntaxKind kind;
	/// <summary>
	/// 赋值语句左侧的表达式。
	/// </summary>
	private readonly ExpressionBuilder left;
	/// <summary>
	/// 赋值语句右侧的表达式。
	/// </summary>
	private readonly ExpressionBuilder right;

	/// <summary>
	/// 使用指定的表达式初始化 <see cref="TypeOfExpressionBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="kind">表达式的类型。</param>
	/// <param name="left">左侧的表达式。</param>
	/// <param name="right">右侧的表达式。</param>
	public AssignmentExpressionBuilder(SyntaxKind kind, ExpressionBuilder left, ExpressionBuilder right)
	{
		this.kind = kind;
		this.left = left;
		this.right = right;
	}

	/// <summary>
	/// 构造赋值表达式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>赋值表达式。</returns>
	public override AssignmentExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.AssignmentExpression(kind,
			left.GetSyntax(format).WithTrailingTrivia(SyntaxFactory.Space),
			right.GetSyntax(format).WithLeadingTrivia(SyntaxFactory.Space));
	}
}
