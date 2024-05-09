using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.CodeAnalysis;

/// <summary>
/// 前缀一元表达式的构造器。
/// </summary>
internal class PrefixUnaryExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 前缀一元表达式的类型。
	/// </summary>
	private readonly SyntaxKind kind;
	/// <summary>
	/// 前缀一元表达式的操作数表达式。
	/// </summary>
	private readonly ExpressionBuilder operand;

	/// <summary>
	/// 使用指定的表达式初始化 <see cref="PrefixUnaryExpressionBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="kind">表达式的类型。</param>
	/// <param name="operand">操作数表达式。</param>
	public PrefixUnaryExpressionBuilder(SyntaxKind kind, ExpressionBuilder operand)
	{
		this.kind = kind;
		this.operand = operand;
	}

	/// <summary>
	/// 构造前缀一元表达式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>前缀一元表达式。</returns>
	public override PrefixUnaryExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.PrefixUnaryExpression(kind, operand.GetSyntax(format));
	}
}
