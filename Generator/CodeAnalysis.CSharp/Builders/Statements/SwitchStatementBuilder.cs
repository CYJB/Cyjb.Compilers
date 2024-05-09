using Cyjb.Compilers.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// switch 语句的构造器。
/// </summary>
internal sealed class SwitchStatementBuilder : StatementBuilder
{
	/// <summary>
	/// switch 语句的条件表达式。
	/// </summary>
	private readonly ExpressionBuilder expression;
	/// <summary>
	/// swtich 语句的段。
	/// </summary>
	private readonly List<SwitchSectionBuilder> sections = new();

	/// <summary>
	/// 使用指定的条件表达式初始化 <see cref="SwitchStatementBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="expression">switch 语句的条件表达式。</param>
	public SwitchStatementBuilder(ExpressionBuilder expression)
	{
		this.expression = expression;
	}

	/// <summary>
	/// 添加 switch 语句的段。
	/// </summary>
	/// <param name="section">switch 语句的段。</param>
	/// <returns>当前 switch 语句。</returns>
	public SwitchStatementBuilder Section(SwitchSectionBuilder section)
	{
		sections.Add(section);
		return this;
	}

	/// <summary>
	/// 添加 switch 语句的 case 段。
	/// </summary>
	/// <param name="value">case 的值。</param>
	/// <param name="statements">case 的语句。</param>
	/// <returns>当前 switch 语句。</returns>
	public SwitchStatementBuilder Case(ExpressionBuilder value, params StatementBuilder[] statements)
	{
		SwitchSectionBuilder section = new();
		section.Case(value);
		foreach (StatementBuilder statement in statements)
		{
			section.Statement(statement);
		}
		sections.Add(section);
		return this;
	}

	/// <summary>
	/// 添加 switch 语句的 case 模式段。
	/// </summary>
	/// <param name="pattern">case 的模式。</param>
	/// <param name="statements">case 的语句。</param>
	/// <returns>当前 switch 语句。</returns>
	public SwitchStatementBuilder Case(PatternBuilder pattern, params StatementBuilder[] statements)
	{
		SwitchSectionBuilder section = new();
		section.Case(pattern);
		foreach (StatementBuilder statement in statements)
		{
			section.Statement(statement);
		}
		sections.Add(section);
		return this;
	}

	/// <summary>
	/// 添加 switch 语句的 default 段。
	/// </summary>
	/// <param name="statements">case 的语句。</param>
	/// <returns>当前 switch 语句。</returns>
	public SwitchStatementBuilder Default(params StatementBuilder[] statements)
	{
		SwitchSectionBuilder section = new();
		foreach (StatementBuilder statement in statements)
		{
			section.Statement(statement);
		}
		sections.Add(section);
		return this;
	}

	/// <summary>
	/// 构造 switch 语句。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>switch 语句。</returns>
	public override SwitchStatementSyntax GetSyntax(SyntaxFormat format)
	{
		SyntaxFormat sectionFormat = format.IncDepth();
		return SyntaxFactory.SwitchStatement(
			SyntaxFactory.Token(SyntaxKind.SwitchKeyword)
				.WithLeadingTrivia(GetLeadingTrivia(format))
				.WithTrailingTrivia(SyntaxFactory.Space),
			SyntaxFactory.Token(SyntaxKind.OpenParenToken),
			expression.GetSyntax(format),
			SyntaxFactory.Token(SyntaxKind.CloseParenToken)
				.WithTrailingTrivia(format.EndOfLine),
			SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
				.WithLeadingTrivia(format.Indentation)
				.WithTrailingTrivia(format.EndOfLine),
			SyntaxFactory.List(sections.Select(section => section.GetSyntax(sectionFormat))),
			SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
				.WithLeadingTrivia(format.Indentation)
				.WithTrailingTrivia(format.EndOfLine)
		);
	}
}

