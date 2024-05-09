using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.CodeAnalysis;

/// <summary>
/// 表示 goto 语句的构造器。
/// </summary>
internal class GotoStatementBuilder : StatementBuilder
{
	/// <summary>
	/// goto 语句的类型。
	/// </summary>
	private readonly SyntaxKind kind;
	/// <summary>
	/// goto 语句的表达式。
	/// </summary>
	private readonly ExpressionBuilder expression;

	/// <summary>
	/// 使用指定的标签名初始化 <see cref="GotoStatementBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="identifier">要跳转的标签名。</param>
	public GotoStatementBuilder(string identifier)
	{
		kind = SyntaxKind.GotoStatement;
		expression = SyntaxFactory.IdentifierName(identifier);
	}

	/// <summary>
	/// 构造 goto 语句。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>goto 语句。</returns>
	public override GotoStatementSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.GotoStatement(kind,
			SyntaxFactory.Token(SyntaxKind.GotoKeyword)
				.WithLeadingTrivia(format.Indentation)
				.WithTrailingTrivia(SyntaxFactory.Space),
			SyntaxFactory.Token(SyntaxKind.None),
			expression.GetSyntax(format),
			SyntaxFactory.Token(SyntaxKind.SemicolonToken).WithTrailingTrivia(format.EndOfLine)
		);
	}
}
