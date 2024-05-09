using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace Cyjb.Compilers.CodeAnalysis;

/// <summary>
/// break 语句的构造器。
/// </summary>
internal class BreakStatementBuilder : StatementBuilder
{
	/// <summary>
	/// 构造 break 语句。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>break 语句。</returns>
	public override BreakStatementSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.BreakStatement()
			.WithLeadingTrivia(format.Indentation)
			.WithTrailingTrivia(format.EndOfLine);
	}
}
