using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// typeof 表达式构造器。
/// </summary>
internal class TypeOfExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 类型。
	/// </summary>
	private readonly TypeBuilder type;

	/// <summary>
	/// 使用指定的类型初始化 <see cref="TypeOfExpressionBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="type">类型。</param>
	public TypeOfExpressionBuilder(TypeBuilder type)
	{
		this.type = type;
	}

	/// <summary>
	/// 构造 typeof 表达式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>typeof 表达式。</returns>
	public override TypeOfExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.TypeOfExpression(type.GetSyntax(format));
	}
}
