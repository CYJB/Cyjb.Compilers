using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 实参的构造器。
/// </summary>
internal sealed class ArgumentBuilder
{
	/// <summary>
	/// 参数的表达式。
	/// </summary>
	private readonly ExpressionBuilder expression;
	/// <summary>
	/// 参数的名称。
	/// </summary>
	private string? name;
	/// <summary>
	/// 参数引用的类型。
	/// </summary>
	private SyntaxKind refKind = SyntaxKind.None;

	/// <summary>
	/// 使用指定的表达式初始化 <see cref="ArgumentBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="expression">参数的表达式。</param>
	public ArgumentBuilder(ExpressionBuilder expression)
	{
		this.expression = expression;
	}

	/// <summary>
	/// 设置参数的名称。
	/// </summary>
	/// <param name="name">参数的名称。</param>
	/// <returns>当前方法实参的构造器。</returns>
	public ArgumentBuilder Name(string name)
	{
		this.name = name;
		return this;
	}

	/// <summary>
	/// 设置参数的引用。
	/// </summary>
	/// <param name="kind">参数的引用。</param>
	/// <returns>当前方法实参的构造器。</returns>
	public ArgumentBuilder Ref(SyntaxKind kind)
	{
		refKind = kind;
		return this;
	}

	/// <summary>
	/// 构造实参语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>实参语法节点。</returns>
	public ArgumentSyntax GetSyntax(SyntaxFormat format)
	{
		ArgumentSyntax syntax = SyntaxFactory.Argument(expression.GetSyntax(format));
		if (name != null)
		{
			syntax = syntax.WithNameColon(SyntaxFactory.NameColon(name)
				.WithTrailingTrivia(SyntaxFactory.Space));
		}
		if (refKind != SyntaxKind.None)
		{
			syntax = syntax.WithRefKindKeyword(SyntaxFactory.Token(refKind)
				.WithTrailingTrivia(SyntaxFactory.Space));
		}
		return syntax;
	}
}
