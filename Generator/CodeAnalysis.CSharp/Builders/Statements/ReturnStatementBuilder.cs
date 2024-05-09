using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 返回语句的构造器。
/// </summary>
internal sealed class ReturnStatementBuilder : StatementBuilder
{
	/// <summary>
	/// 要返回的表达式。
	/// </summary>
	private readonly ExpressionBuilder expression;

	/// <summary>
	/// 使用指定的表达式初始化 <see cref="ReturnStatementBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="expression">要返回的表达式。</param>
	public ReturnStatementBuilder(ExpressionBuilder expression)
	{
		this.expression = expression;
	}

	/// <summary>
	/// 构造返回语句。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>返回语句。</returns>
	public override ReturnStatementSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.ReturnStatement(
			SyntaxFactory.Token(SyntaxKind.ReturnKeyword)
				.WithLeadingTrivia(GetLeadingTrivia(format))
				.WithTrailingTrivia(SyntaxFactory.Space),
			expression.GetSyntax(format),
			SyntaxFactory.Token(SyntaxKind.SemicolonToken)
				.WithTrailingTrivia(format.EndOfLine)
		);
	}
}
