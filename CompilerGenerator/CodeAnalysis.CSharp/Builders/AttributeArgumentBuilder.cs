using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 特性参数的构造器。
/// </summary>
internal sealed class AttributeArgumentBuilder
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
	/// 参数是否是特性成员。
	/// </summary>
	private bool isMember = false;

	/// <summary>
	/// 使用指定的表达式初始化 <see cref="ArgumentBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="expression">参数的表达式。</param>
	public AttributeArgumentBuilder(ExpressionBuilder expression)
	{
		this.expression = expression;
	}

	/// <summary>
	/// 设置参数的名称。
	/// </summary>
	/// <param name="name">参数的名称。</param>
	/// <param name="isMember">参数是否是特性成员。</param>
	/// <returns>当前特性参数的构造器。</returns>
	public AttributeArgumentBuilder Name(string name, bool isMember = false)
	{
		this.name = name;
		this.isMember = isMember;
		return this;
	}

	/// <summary>
	/// 构造特性参数语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>特性参数语法节点。</returns>
	public AttributeArgumentSyntax GetSyntax(SyntaxFormat format)
	{
		var syntax = SyntaxFactory.AttributeArgument(expression.GetSyntax(format));
		if (name != null)
		{
			if (isMember)
			{
				syntax = syntax.WithNameEquals(SyntaxFactory.NameEquals(name)
					.WithLeadingTrivia(SyntaxFactory.Space)
					.WithTrailingTrivia(SyntaxFactory.Space));
			}
			else
			{
				syntax = syntax.WithNameColon(SyntaxFactory.NameColon(name)
					.WithTrailingTrivia(SyntaxFactory.Space));
			}
		}
		return syntax;
	}
}
