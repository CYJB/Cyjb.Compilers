using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.CodeAnalysis;

/// <summary>
/// 标签语句的构造器。
/// </summary>
internal class LabeledStatementBuilder : StatementBuilder
{
	/// <summary>
	/// 标签的标识符。
	/// </summary>
	private readonly string identifier;
	/// <summary>
	/// 标签的语句。
	/// </summary>
	private readonly StatementBuilder statement;

	/// <summary>
	/// 使用标签标识符和语句初始化 <see cref="LabeledStatementBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="identifier">标签的标识符。</param>
	/// <param name="statement">标签的语句。</param>
	public LabeledStatementBuilder(string identifier, StatementBuilder statement)
	{
		this.identifier = identifier;
		this.statement = statement;
	}

	/// <summary>
	/// 构造标签语句。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>标签语句。</returns>
	public override LabeledStatementSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.LabeledStatement(
			SyntaxFactory.Identifier(identifier).WithLeadingTrivia(format.DecDepth().Indentation),
			SyntaxFactory.Token(SyntaxKind.ColonToken).WithTrailingTrivia(format.EndOfLine),
			statement.GetSyntax(format)
		);
	}
}
