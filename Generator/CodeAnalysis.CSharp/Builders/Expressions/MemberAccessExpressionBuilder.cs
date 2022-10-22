using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 成员访问表达式的构造器。
/// </summary>
internal sealed class MemberAccessExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 对象表达式。
	/// </summary>
	private readonly ExpressionBuilder expression;
	/// <summary>
	/// 成员名称。
	/// </summary>
	private readonly string name;

	/// <summary>
	/// 使用指定的对象表达式和成员名称初始化 <see cref="NameBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="expression">对象表达式。</param>
	/// <param name="name">成员名称。</param>
	public MemberAccessExpressionBuilder(ExpressionBuilder expression, string name)
	{
		this.expression = expression;
		this.name = name;
	}

	/// <summary>
	/// 构造成员访问表达式。
	/// </summary>
	/// <param name="format">语法格式。</param>
	/// <returns>成员访问表达式。</returns>
	public override MemberAccessExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.MemberAccessExpression(
			SyntaxKind.SimpleMemberAccessExpression,
			expression.GetSyntax(format),
			(SimpleNameSyntax)SyntaxFactory.ParseTypeName(name));
	}
}
