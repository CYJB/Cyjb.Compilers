using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace Cyjb.Compilers.CodeAnalysis;

/// <summary>
/// 表示 if 语句的构造器。
/// </summary>
internal class IfStatementBuilder : StatementBuilder
{
	/// <summary>
	/// if 语句的条件。
	/// </summary>
	private readonly ExpressionBuilder condition;
	/// <summary>
	/// if 条件的语句。
	/// </summary>
	private readonly BlockBuilder statement = new();
	/// <summary>
	/// else 子句。
	/// </summary>
	private StatementBuilder? elseClause;

	/// <summary>
	/// 使用指定的条件初始化 <see cref="IfStatementBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="condition">if 语句的条件。</param>
	public IfStatementBuilder(ExpressionBuilder condition)
	{
		this.condition = condition;
	}

	/// <summary>
	/// 添加 if 的语句。
	/// </summary>
	/// <param name="statement">要添加的语句。</param>
	/// <returns>当前 if 语句。</returns>
	public IfStatementBuilder Statement(StatementBuilder statement)
	{
		this.statement.Add(statement);
		return this;
	}

	/// <summary>
	/// 添加 else 子句。
	/// </summary>
	/// <param name="elseClause">要添加的 else 子句。</param>
	/// <returns>当前 if 语句。</returns>
	public IfStatementBuilder Else(StatementBuilder elseClause)
	{
		this.elseClause = elseClause;
		return this;
	}

	/// <summary>
	/// 构造 if 语句。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>if 语句。</returns>
	public override IfStatementSyntax GetSyntax(SyntaxFormat format)
	{
		ElseClauseSyntax? elseClause = null;
		if (this.elseClause != null)
		{
			elseClause = SyntaxFactory.ElseClause(
				SyntaxFactory.Token(SyntaxKind.ElseKeyword)
					.WithLeadingTrivia(format.Indentation)
					.WithTrailingTrivia(format.EndOfLine),
				this.elseClause.GetSyntax(format));
		}
		return SyntaxFactory.IfStatement(
			SyntaxFactory.Token(SyntaxKind.IfKeyword)
				.WithLeadingTrivia(format.Indentation)
				.WithTrailingTrivia(SyntaxFactory.Space),
			SyntaxFactory.Token(SyntaxKind.OpenParenToken),
			condition.GetSyntax(format),
			SyntaxFactory.Token(SyntaxKind.CloseParenToken)
				.WithTrailingTrivia(format.EndOfLine),
			statement.GetSyntax(format),
			elseClause
		);
	}
}
