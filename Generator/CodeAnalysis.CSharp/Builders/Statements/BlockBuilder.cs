using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 语句块的构造器。
/// </summary>
internal sealed class BlockBuilder : StatementBuilder
{
	/// <summary>
	/// 包含的语句列表。
	/// </summary>
	private readonly List<StatementBuilder> statements = new();

	/// <summary>
	/// 添加指定的语句。
	/// </summary>
	/// <param name="statement">语句。</param>
	/// <returns>当前语句块构造器。</returns>
	public BlockBuilder Add(StatementBuilder statement)
	{
		statements.Add(statement);
		return this;
	}

	/// <summary>
	/// 构造语句块语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>语句块语法节点。</returns>
	public override BlockSyntax GetSyntax(SyntaxFormat format)
	{
		SyntaxFormat stmFormat = format.IncDepth();
		return SyntaxFactory.Block(
			SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
				.WithLeadingTrivia(GetLeadingTrivia(format))
				.WithTrailingTrivia(format.EndOfLine),
			SyntaxFactory.List(statements.Select(stm => stm.GetSyntax(stmFormat))),
			SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
				.WithLeadingTrivia(format.Indentation)
				.WithTrailingTrivia(format.EndOfLine)
		);
	}
}
