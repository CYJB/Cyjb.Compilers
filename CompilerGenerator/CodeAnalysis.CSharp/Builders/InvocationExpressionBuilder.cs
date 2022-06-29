using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 方法调用表达式的构造器。
/// </summary>
internal sealed class InvocationExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 要调用的方法。
	/// </summary>
	private readonly ExpressionBuilder expression;
	/// <summary>
	/// 方法的调用参数。
	/// </summary>
	private readonly ArgumentListBuilder arguments = new();

	/// <summary>
	/// 使用要调用的方法表达式初始化 <see cref="InvocationExpressionBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="expression">要调用的方法。</param>
	public InvocationExpressionBuilder(ExpressionBuilder expression)
	{
		this.expression = expression;
	}

	/// <summary>
	/// 添加方法的参数。
	/// </summary>
	/// <param name="argument">方法的参数。</param>
	/// <returns>当前方法调用表达式构造器。</returns>
	public InvocationExpressionBuilder Argument(ExpressionBuilder argument)
	{
		arguments.Add(argument);
		return this;
	}

	/// <summary>
	/// 添加方法的参数。
	/// </summary>
	/// <param name="argument">方法的参数。</param>
	/// <param name="name">参数的名称。</param>
	/// <returns>当前方法调用表达式构造器。</returns>
	public InvocationExpressionBuilder Argument(ExpressionBuilder argument, string name)
	{
		arguments.Add(argument, name);
		return this;
	}

	/// <summary>
	/// 构造方法调用表达式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>方法调用表达式。</returns>
	public override InvocationExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		if (arguments.Count == 0)
		{
			return SyntaxFactory.InvocationExpression(expression.GetSyntax(format));
		}
		else
		{
			return SyntaxFactory.InvocationExpression(expression.GetSyntax(format), arguments.GetSyntax(format));
		}
	}
}
