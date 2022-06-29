using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 实参列表的构造器。
/// </summary>
internal sealed class ArgumentListBuilder
{
	/// <summary>
	/// 实参列表。
	/// </summary>
	private readonly List<ArgumentBuilder> arguments = new();
	/// <summary>
	/// 实参的换行信息，<c>0</c> 表示不换行。
	/// </summary>
	private int wrap = 0;

	/// <summary>
	/// 参数的个数。
	/// </summary>
	public int Count => arguments.Count;

	/// <summary>
	/// 添加指定的实参。
	/// </summary>
	/// <param name="expression">参数表达式。</param>
	/// <returns>当前实参列表构造器。</returns>
	public ArgumentListBuilder Add(ExpressionBuilder expression)
	{
		arguments.Add(new ArgumentBuilder(expression));
		return this;
	}

	/// <summary>
	/// 添加指定的实参。
	/// </summary>
	/// <param name="expression">参数表达式。</param>
	/// <param name="name">参数的名称。</param>
	/// <returns>当前实参列表构造器。</returns>
	public ArgumentListBuilder Add(ExpressionBuilder expression, string name)
	{
		arguments.Add(new ArgumentBuilder(expression).Name(name));
		return this;
	}

	/// <summary>
	/// 设置参数的换行情况，默认为 <c>0</c> 表示不换行。
	/// </summary>
	/// <param name="wrap">换行情况。</param>
	/// <returns>当前实参列表构造器。</returns>
	public ArgumentListBuilder Wrap(int wrap)
	{
		if (wrap >= 0)
		{
			this.wrap = wrap;
		}
		return this;
	}

	/// <summary>
	/// 构造实参的列表。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>实参列表。</returns>
	public ArgumentListSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.ArgumentList(SyntaxBuilder.SeparatedList(
			arguments.Select(arg => arg.GetSyntax(format)), format.IncDepth(), wrap));
	}
}
