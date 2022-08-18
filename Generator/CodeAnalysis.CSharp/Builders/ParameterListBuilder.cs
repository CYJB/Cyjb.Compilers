using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 形参列表的构造器。
/// </summary>
internal sealed class ParameterListBuilder
{
	/// <summary>
	/// 形参列表。
	/// </summary>
	private readonly List<ParameterBuilder> parameters = new();

	/// <summary>
	/// 参数的个数。
	/// </summary>
	public int Count => parameters.Count;
	/// <summary>
	/// 是否是简单参数列表（只包含单个参数）。
	/// </summary>
	public bool IsSimple => parameters.Count == 1 && parameters[0].IsSimple;

	/// <summary>
	/// 添加指定的形参。
	/// </summary>
	/// <param name="name">参数的名称。</param>
	/// <returns>当前形参列表构造器。</returns>
	public ParameterListBuilder Add(string name)
	{
		parameters.Add(new ParameterBuilder(name));
		return this;
	}

	/// <summary>
	/// 添加指定的形参。
	/// </summary>
	/// <param name="name">参数的名称。</param>
	/// <param name="type">参数的类型。</param>
	/// <returns>当前形参列表构造器。</returns>
	public ParameterListBuilder Add(string name, TypeBuilder type)
	{
		parameters.Add(new ParameterBuilder(name).Type(type));
		return this;
	}

	/// <summary>
	/// 构造简单形参。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>简单形参。</returns>
	public ParameterSyntax GetSimpleSyntax(SyntaxFormat format)
	{
		return parameters[0].GetSyntax(format);
	}

	/// <summary>
	/// 构造形参的列表。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>形参列表。</returns>
	public ParameterListSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.ParameterList(SyntaxBuilder.SeparatedList(
			parameters.Select(arg => arg.GetSyntax(format)), format));
	}
}
