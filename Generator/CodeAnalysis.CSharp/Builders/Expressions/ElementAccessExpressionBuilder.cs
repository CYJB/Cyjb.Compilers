using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 元素访问表达式的构造器。
/// </summary>
internal class ElementAccessExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 要访问元素的表达式。
	/// </summary>
	private readonly ExpressionBuilder expression;
	/// <summary>
	/// 要访问的元素。
	/// </summary>
	private readonly List<ArgumentBuilder> arguments = new();

	/// <summary>
	/// 使用指定的表达式初始化 <see cref="ElementAccessExpressionBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="expression">要访问元素的表达式。</param>
	public ElementAccessExpressionBuilder(ExpressionBuilder expression)
	{
		this.expression = expression;
	}

	/// <summary>
	/// 添加指定的元素参数。
	/// </summary>
	/// <param name="expression">参数表达式。</param>
	/// <returns>当前元素访问表达式的构造器。</returns>
	public ElementAccessExpressionBuilder Argument(ExpressionBuilder expression)
	{
		arguments.Add(new ArgumentBuilder(expression));
		return this;
	}

	/// <summary>
	/// 构造元素访问表达式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>元素访问表达式语法节点。</returns>
	public override ElementAccessExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.ElementAccessExpression(expression.GetSyntax(format),
			SyntaxFactory.BracketedArgumentList(SyntaxBuilder.SeparatedList(
				arguments.Select(arg => arg.GetSyntax(format)), format.IncDepth(), 0)));
	}
}
