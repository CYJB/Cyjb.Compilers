using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 类型的构造器。
/// </summary>
internal class TypeBuilder : ExpressionBuilder
{
	/// <summary>
	/// 类型语法单元。
	/// </summary>
	private readonly TypeSyntax type;

	/// <summary>
	/// 使用指定的类型初始化 <see cref="TypeBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="type">类型语法单元。</param>
	public TypeBuilder(TypeSyntax type)
	{
		this.type = type;
	}

	/// <summary>
	/// 使用指定的类型名称初始化 <see cref="TypeBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="typeName">类型名称。</param>
	public TypeBuilder(string typeName)
	{
		type = SyntaxFactory.ParseTypeName(typeName);
	}

	/// <summary>
	/// 允许从类型语法单元隐式转换为 <see cref="TypeBuilder"/> 对象。
	/// </summary>
	/// <param name="expression">转换到的 <see cref="TypeBuilder"/> 对象。</param>
	public static implicit operator TypeBuilder(TypeSyntax type)
	{
		return new TypeBuilder(type);
	}

	/// <summary>
	/// 允许从字符串隐式转换为 <see cref="TypeBuilder"/> 对象。
	/// </summary>
	/// <param name="expression">转换到的 <see cref="TypeBuilder"/> 对象。</param>
	public static implicit operator TypeBuilder(string typeName)
	{
		return new TypeBuilder(typeName);
	}

	/// <summary>
	/// 构造类型法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>类型语法节点。</returns>
	public override TypeSyntax GetSyntax(SyntaxFormat format)
	{
		return type;
	}
}
