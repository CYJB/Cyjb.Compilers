using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 自定义类型的构造器。
/// </summary>
internal sealed class CustomTypeBuilder : TypeBuilder
{
	/// <summary>
	/// 待简化的名称。
	/// </summary>
	private static readonly Dictionary<string, string> SimplifyNames = new()
	{
		{ "System.Int32", "int" },
	};

	/// <summary>
	/// 类型语法单元。
	/// </summary>
	private readonly TypeSyntax type;

	/// <summary>
	/// 使用指定的类型初始化 <see cref="CustomTypeBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="type">类型语法单元。</param>
	public CustomTypeBuilder(TypeSyntax type)
	{
		this.type = type;
	}

	/// <summary>
	/// 使用指定的类型名称初始化 <see cref="CustomTypeBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="typeName">类型名称。</param>
	public CustomTypeBuilder(string typeName)
	{
		// 化简部分基础类型。
		foreach (var (oldName, newName) in SimplifyNames)
		{
			typeName = typeName.Replace(oldName, newName);
		}
		type = SyntaxFactory.ParseTypeName(typeName);
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
