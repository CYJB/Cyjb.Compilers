using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 特性参数列表的构造器。
/// </summary>
internal sealed class AttributeArgumentListBuilder
{
	/// <summary>
	/// 特性参数列表。
	/// </summary>
	private readonly List<AttributeArgumentBuilder> AttributeArguments = new();
	/// <summary>
	/// 特性参数的换行信息，<c>0</c> 表示不换行。
	/// </summary>
	private int wrap = 0;

	/// <summary>
	/// 参数的个数。
	/// </summary>
	public int Count => AttributeArguments.Count;

	/// <summary>
	/// 添加指定的特性参数。
	/// </summary>
	/// <param name="expression">参数表达式。</param>
	/// <returns>当前特性参数列表构造器。</returns>
	public AttributeArgumentListBuilder Add(ExpressionBuilder expression)
	{
		AttributeArguments.Add(new AttributeArgumentBuilder(expression));
		return this;
	}

	/// <summary>
	/// 添加指定的特性参数。
	/// </summary>
	/// <param name="expression">参数表达式。</param>
	/// <param name="name">参数的名称。</param>
	/// <param name="isMember">参数是否是特性成员。</param>
	/// <returns>当前特性参数列表构造器。</returns>
	public AttributeArgumentListBuilder Add(ExpressionBuilder expression, string name, bool isMember = false)
	{
		AttributeArguments.Add(new AttributeArgumentBuilder(expression).Name(name, isMember));
		return this;
	}

	/// <summary>
	/// 设置参数的换行情况，默认为 <c>0</c> 表示不换行。
	/// </summary>
	/// <param name="wrap">换行情况。</param>
	/// <returns>当前特性参数列表构造器。</returns>
	public AttributeArgumentListBuilder Wrap(int wrap)
	{
		if (wrap >= 0)
		{
			this.wrap = wrap;
		}
		return this;
	}

	/// <summary>
	/// 构造特性参数的列表。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>特性参数列表。</returns>
	public AttributeArgumentListSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.AttributeArgumentList(SyntaxBuilder.SeparatedList(
			AttributeArguments.Select(arg => arg.GetSyntax(format)), format.IncDepth(), wrap));
	}
}
