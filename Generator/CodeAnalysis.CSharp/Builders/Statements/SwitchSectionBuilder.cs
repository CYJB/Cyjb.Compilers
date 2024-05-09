using Cyjb.Compilers.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// switch 段的构造器。
/// </summary>
internal sealed class SwitchSectionBuilder
{
	/// <summary>
	/// case 的值，为空时表示 default。
	/// </summary>
	private readonly List<Variant<ExpressionBuilder, PatternBuilder>> cases = new();
	/// <summary>
	/// 语句列表。
	/// </summary>
	private readonly List<StatementBuilder> statements = new();

	/// <summary>
	/// 添加 case 值。
	/// </summary>
	/// <param name="value">case 的值。</param>
	/// <returns>当前 switch 段。</returns>
	public SwitchSectionBuilder Case(ExpressionBuilder value)
	{
		cases.Add(value);
		return this;
	}

	/// <summary>
	/// 添加 case 模式。
	/// </summary>
	/// <param name="pattern">case 的模式。</param>
	/// <returns>当前 switch 段。</returns>
	public SwitchSectionBuilder Case(PatternBuilder pattern)
	{
		cases.Add(pattern);
		return this;
	}

	/// <summary>
	/// 添加语句。
	/// </summary>
	/// <param name="statement">要添加的语句。</param>
	/// <returns>当前 switch 段。</returns>
	public SwitchSectionBuilder Statement(StatementBuilder statement)
	{
		statements.Add(statement);
		return this;
	}

	/// <summary>
	/// 构造 switch 段。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>switch 段。</returns>
	public SwitchSectionSyntax GetSyntax(SyntaxFormat format)
	{
		SyntaxFormat statementFormat = format.IncDepth();
		SyntaxList<SwitchLabelSyntax> labels;
		if (cases.Count == 0)
		{
			// 默认段。
			labels = SyntaxFactory.SingletonList<SwitchLabelSyntax>(
				SyntaxFactory.DefaultSwitchLabel()
					.WithLeadingTrivia(format.Indentation)
					.WithTrailingTrivia(format.EndOfLine)
			);
		}
		else
		{
			// case 段
			labels = SyntaxFactory.List(cases.Select(value =>
			{
				SwitchLabelSyntax syntax;
				if (value.TryGetValue(out ExpressionBuilder? expression))
				{
					syntax = SyntaxFactory.CaseSwitchLabel(expression.GetSyntax(format)
						.WithLeadingTrivia(SyntaxFactory.Space));
				}
				else
				{
					PatternSyntax pattern = ((PatternBuilder)value).GetSyntax(format)
						.WithLeadingTrivia(SyntaxFactory.Space);
					syntax = SyntaxFactory.CasePatternSwitchLabel(
						 pattern,
						 SyntaxFactory.Token(SyntaxKind.ColonToken)
					 );
				}
				return syntax
					.WithLeadingTrivia(format.Indentation)
					.WithTrailingTrivia(format.EndOfLine);

			}));
		}
		return SyntaxFactory.SwitchSection(labels, SyntaxFactory.List(statements
			.Select(statement => statement.GetSyntax(statementFormat)))
		);
	}
}

