using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.CodeAnalysis;

/// <summary>
/// 二元表达式的构造器。
/// </summary>
internal class BinaryExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 二元表达式的类型。
	/// </summary>
	private readonly SyntaxKind kind;
	/// <summary>
	/// 二元表达式左侧的表达式。
	/// </summary>
	private readonly ExpressionBuilder left;
	/// <summary>
	/// 二元表达式右侧的表达式。
	/// </summary>
	private readonly ExpressionBuilder right;

	/// <summary>
	/// 使用指定的表达式初始化 <see cref="BinaryExpressionBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="kind">表达式的类型。</param>
	/// <param name="left">左侧的表达式。</param>
	/// <param name="right">右侧的表达式。</param>
	public BinaryExpressionBuilder(SyntaxKind kind, ExpressionBuilder left, ExpressionBuilder right)
	{
		this.kind = kind;
		this.left = left;
		this.right = right;
	}

	/// <summary>
	/// 构造二元表达式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>二元表达式。</returns>
	public override BinaryExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.BinaryExpression(kind,
			left.GetSyntax(format).WithTrailingTrivia(SyntaxFactory.Space),
			right.GetSyntax(format).WithLeadingTrivia(SyntaxFactory.Space));
	}
}
