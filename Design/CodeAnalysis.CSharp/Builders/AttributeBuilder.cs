using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 特性构造器。
/// </summary>
internal sealed class AttributeBuilder
{
	/// <summary>
	/// 特性的名称。
	/// </summary>
	private readonly string name;
	/// <summary>
	/// 特性的参数。
	/// </summary>
	private readonly AttributeArgumentListBuilder arguments = new();

	/// <summary>
	/// 使用指定的名称初始化 <see cref="AttributeBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="name">特性的名称。</param>
	public AttributeBuilder(string name)
	{
		this.name = name;
	}

	/// <summary>
	/// 添加特性的参数。
	/// </summary>
	/// <param name="argument">特性的参数。</param>
	/// <returns>当前特性构造器。</returns>
	public AttributeBuilder Argument(ExpressionBuilder argument)
	{
		arguments.Add(argument);
		return this;
	}

	/// <summary>
	/// 添加特性的参数。
	/// </summary>
	/// <param name="argument">特性的参数。</param>
	/// <param name="name">参数的名称。</param>
	/// <param name="isMember">参数是否是特性成员。</param>
	/// <returns>当前特性构造器。</returns>
	public AttributeBuilder Argument(ExpressionBuilder argument, string name, bool isMember = false)
	{
		arguments.Add(argument, name, isMember);
		return this;
	}

	/// <summary>
	/// 构造特性语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>特性语法节点。</returns>
	public AttributeSyntax GetSyntax(SyntaxFormat format)
	{
		var syntax = SyntaxFactory.Attribute(SyntaxFactory.ParseName(name));
		if (arguments.Count > 0)
		{
			syntax = syntax.WithArgumentList(arguments.GetSyntax(format));
		}
		return syntax;
	}

	/// <summary>
	/// 构造特性列表语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>特性列表语法节点。</returns>
	public AttributeListSyntax GetListSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(GetSyntax(format)));
	}
}
