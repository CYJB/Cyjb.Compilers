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
	/// 返回访问指定成员的表达式构造器。
	/// </summary>
	/// <param name="name">要访问的成员名称。</param>
	/// <returns>成员访问表达式构造器。</returns>
	public MemberAccessExpressionBuilder Access(string name)
	{
		return new MemberAccessExpressionBuilder(this, name);
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
