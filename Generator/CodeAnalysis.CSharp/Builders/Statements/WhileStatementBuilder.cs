using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.CodeAnalysis;

/// <summary>
/// while 语句的构造器。
/// </summary>
internal class WhileStatementBuilder : StatementBuilder
{
	/// <summary>
	/// while 语句的条件表达式。
	/// </summary>
	private readonly ExpressionBuilder condition;
	/// <summary>
	/// 包含的语句块。
	/// </summary>
	private readonly BlockBuilder block = new();

	/// <summary>
	/// 初始化 <see cref="WhileStatementBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="condition">while 语句的条件表达式。</param>
	public WhileStatementBuilder(ExpressionBuilder condition)
	{
		this.condition = condition;
	}

	/// <summary>
	/// 添加指定的语句。
	/// </summary>
	/// <param name="statement">语句。</param>
	/// <returns>当前语句块构造器。</returns>
	public WhileStatementBuilder Statement(StatementBuilder statement)
	{
		block.Add(statement);
		return this;
	}

	/// <summary>
	/// 构造 while 语句。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>while 语句。</returns>
	public override WhileStatementSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.WhileStatement(
			SyntaxFactory.Token(SyntaxKind.WhileKeyword)
				.WithLeadingTrivia(GetLeadingTrivia(format))
				.WithTrailingTrivia(SyntaxFactory.Space),
			SyntaxFactory.Token(SyntaxKind.OpenParenToken),
			condition.GetSyntax(format),
			SyntaxFactory.Token(SyntaxKind.CloseParenToken)
				.WithTrailingTrivia(format.EndOfLine),
			block.GetSyntax(format)
		);
	}
}

