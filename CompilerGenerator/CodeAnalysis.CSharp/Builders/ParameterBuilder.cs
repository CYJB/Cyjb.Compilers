using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 形参的构造器。
/// </summary>
internal sealed class ParameterBuilder
{
	/// <summary>
	/// 参数的名称。
	/// </summary>
	private readonly string name;
	/// <summary>
	/// 参数类型。
	/// </summary>
	private TypeBuilder? type;

	/// <summary>
	/// 使用指定的参数名称初始化 <see cref="ParameterBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="name">参数的名称。</param>
	public ParameterBuilder(string name)
	{
		this.name = name;
	}

	/// <summary>
	/// 是否是简单参数（只包含参数名称）。
	/// </summary>
	public bool IsSimple => type == null;

	/// <summary>
	/// 设置参数的类型。
	/// </summary>
	/// <param name="type">参数的类型。</param>
	/// <returns>当前方法形参的构造器。</returns>
	public ParameterBuilder Type(TypeBuilder type)
	{
		this.type = type;
		return this;
	}

	/// <summary>
	/// 构造形参语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>形参语法节点。</returns>
	public ParameterSyntax GetSyntax(SyntaxFormat format)
	{
		ParameterSyntax syntax = SyntaxFactory.Parameter(SyntaxFactory.Identifier(name));
		if (type != null)
		{
			syntax = syntax.WithType(type.GetSyntax(format).WithTrailingTrivia(SyntaxFactory.Space));
		}
		return syntax;
	}
}
