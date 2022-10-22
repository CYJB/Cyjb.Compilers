using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 强制类型转换表达式构造器。
/// </summary>
internal class CastExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 要转换到的类型。
	/// </summary>
	private readonly TypeBuilder type;
	/// <summary>
	/// 要转换的表达式。
	/// </summary>
	private readonly ExpressionBuilder expression;

	/// <summary>
	/// 使用指定的类型和表达式初始化 <see cref="TypeOfExpressionBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="type">要转换到的类型。</param>
	/// <param name="expression">要转换的表达式。</param>
	public CastExpressionBuilder(TypeBuilder type, ExpressionBuilder expression)
	{
		this.type = type;
		this.expression = expression;
	}

	/// <summary>
	/// 构造强制类型转换表达式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>强制类型转换表达式。</returns>
	public override CastExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.CastExpression(type.GetSyntax(format), expression.GetSyntax(format));
	}
}
