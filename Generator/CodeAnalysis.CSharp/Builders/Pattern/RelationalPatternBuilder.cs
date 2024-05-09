using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.CodeAnalysis;

/// <summary>
/// 关系模式的构造器。
/// </summary>
internal class RelationalPatternBuilder : PatternBuilder
{
	/// <summary>
	/// 关系模式的类型。
	/// </summary>
	private readonly SyntaxKind kind;
	/// <summary>
	/// 关系模式的表达式。
	/// </summary>
	private readonly ExpressionBuilder expression;

	/// <summary>
	/// 使用指定的表达式初始化 <see cref="RelationalPatternBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="kind">表达式的类型。</param>
	/// <param name="expression">关系模式的表达式。</param>
	public RelationalPatternBuilder(SyntaxKind kind, ExpressionBuilder expression)
	{
		this.kind = kind;
		this.expression = expression;
	}

	/// <summary>
	/// 构造关系模式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>关系模式。</returns>
	public override RelationalPatternSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.RelationalPattern(
			SyntaxFactory.Token(kind).WithTrailingTrivia(SyntaxFactory.Space),
			expression.GetSyntax(format));
	}
}
