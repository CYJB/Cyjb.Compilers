using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 表达式语句的构造器。
/// </summary>
internal sealed class ExpressionStatementBuilder : StatementBuilder
{
	/// <summary>
	/// 语句包含的表达式。
	/// </summary>
	private readonly ExpressionBuilder expression;

	/// <summary>
	/// 使用指定的表达式初始化 <see cref="ExpressionStatementBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="expression">语句包含的表达式。</param>
	public ExpressionStatementBuilder(ExpressionBuilder expression)
	{
		this.expression = expression;
	}

	/// <summary>
	/// 设置语句的注释。
	/// </summary>
	/// <param name="comment">注释的内容。</param>
	/// <returns>当前变量声明语句构造器。</returns>
	public new ExpressionStatementBuilder Comment(string? comment)
	{
		base.Comment(comment);
		return this;
	}

	/// <summary>
	/// 构造返回语句。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>返回语句。</returns>
	public override ExpressionStatementSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.ExpressionStatement(expression.GetSyntax(format))
			.WithLeadingTrivia(GetLeadingTrivia(format)).WithTrailingTrivia(format.EndOfLine);
	}

	/// <summary>
	/// 允许从表达式构造器隐式转换为 <see cref="ExpressionStatementBuilder"/> 对象。
	/// </summary>
	/// <param name="expression">要转换的表达式构造器。</param>
	/// <returns>转换到的 <see cref="ExpressionStatementBuilder"/> 对象。</returns>
	public static implicit operator ExpressionStatementBuilder(ExpressionBuilder expression)
	{
		return new ExpressionStatementBuilder(expression);
	}
}
