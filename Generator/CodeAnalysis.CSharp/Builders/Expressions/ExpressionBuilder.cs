using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 表达式的构造器。
/// </summary>
internal abstract class ExpressionBuilder
{
	/// <summary>
	/// 构造表达式语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>表达式语法节点。</returns>
	public abstract ExpressionSyntax GetSyntax(SyntaxFormat format);

	/// <summary>
	/// 返回当前表达式的括号表达式构造器。
	/// </summary>
	/// <returns>括号表达式构造器。</returns>
	public ParenthesizedExpressionBuilder Parenthesized()
	{
		return new ParenthesizedExpressionBuilder(this);
	}

	/// <summary>
	/// 返回强制转换到指定类型的表达式构造器。
	/// </summary>
	/// <param name="type">要强制转换到的类型。</param>
	/// <returns>强制类型转换表达式构造器。</returns>
	public CastExpressionBuilder Cast(TypeBuilder type)
	{
		return new CastExpressionBuilder(type, this);
	}

	/// <summary>
	/// 返回强制转换到指定类型的表达式构造器。
	/// </summary>
	/// <param name="type">要强制转换到的类型。</param>
	/// <returns>强制类型转换表达式构造器。</returns>
	public CastExpressionBuilder Cast(string type)
	{
		return new CastExpressionBuilder(SyntaxBuilder.Type(type), this);
	}

	/// <summary>
	/// 返回赋值表达式的构造器。
	/// </summary>
	/// <param name="kind">赋值的类型。</param>
	/// <param name="value">赋值的值。</param>
	/// <returns>赋值表达式的构造器。</returns>
	public AssignmentExpressionBuilder Assign(SyntaxKind kind, ExpressionBuilder value)
	{
		return new AssignmentExpressionBuilder(kind, this, value);
	}

	/// <summary>
	/// 返回访问指定成员的表达式构造器。
	/// </summary>
	/// <param name="name">要访问的成员名称。</param>
	/// <returns>成员访问表达式构造器。</returns>
	public MemberAccessExpressionBuilder AccessMember(string name)
	{
		return new MemberAccessExpressionBuilder(this, name);
	}

	/// <summary>
	/// 返回访问指定元素的表达式构造器。
	/// </summary>
	/// <param name="expression">要访问的元素。</param>
	/// <returns>元素访问表达式构造器。</returns>
	public ElementAccessExpressionBuilder AccessElement(ExpressionBuilder expression)
	{
		return new ElementAccessExpressionBuilder(this).Argument(expression);
	}

	/// <summary>
	/// 返回调用当前方法的表达式。
	/// </summary>
	/// <param name="arguments">要调用的方法参数。</param>
	/// <returns>方法调用表达式的构造器。</returns>
	public InvocationExpressionBuilder Invoke(params ExpressionBuilder[] arguments)
	{
		InvocationExpressionBuilder builder = new(this);
		foreach (ExpressionBuilder arg in arguments)
		{
			builder.Argument(arg);
		}
		return builder;
	}

	/// <summary>
	/// 返回当前表达式的语句。
	/// </summary>
	/// <returns>当前表达式的语句。</returns>
	public ExpressionStatementBuilder Statement()
	{
		return new ExpressionStatementBuilder(this);
	}

	/// <summary>
	/// 允许从表达式节点隐式转换为 <see cref="ExpressionBuilder"/> 对象。
	/// </summary>
	/// <param name="expression">转换到的 <see cref="ExpressionBuilder"/> 对象。</param>
	public static implicit operator ExpressionBuilder(ExpressionSyntax expression)
	{
		return new ExpressionWrapperBuilder(expression);
	}

	/// <summary>
	/// 表达式节点的构造器包装。
	/// </summary>
	private sealed class ExpressionWrapperBuilder : ExpressionBuilder
	{
		/// <summary>
		/// 被包装的表达式节点。
		/// </summary>
		private readonly ExpressionSyntax expression;

		/// <summary>
		/// 使用指定的表达式节点初始化 <see cref="ExpressionWrapperBuilder"/> 类的新实例。
		/// </summary>
		/// <param name="expression">被包装的表达式节点。</param>
		public ExpressionWrapperBuilder(ExpressionSyntax expression)
		{
			this.expression = expression;
		}

		/// <summary>
		/// 构造表达式语法节点。
		/// </summary>
		/// <param name="format">语法的格式信息。</param>
		/// <returns>表达式语法节点。</returns>
		public override ExpressionSyntax GetSyntax(SyntaxFormat format)
		{
			return expression;
		}
	}
}
