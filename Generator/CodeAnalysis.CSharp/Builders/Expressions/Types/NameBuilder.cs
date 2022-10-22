using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 名称表达式的构造器。
/// </summary>
internal sealed class NameBuilder : TypeBuilder
{
	/// <summary>
	/// 标识符名称。
	/// </summary>
	private readonly string identifier;
	/// <summary>
	/// 类型参数。
	/// </summary>
	private readonly List<TypeBuilder> typeArguments = new();
	/// <summary>
	/// 限定符名称。
	/// </summary>
	private string? qualifier;

	/// <summary>
	/// 使用指定的标识符名称初始化 <see cref="NameBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="identifier">标识符名称。</param>
	public NameBuilder(string identifier)
	{
		this.identifier = identifier;
	}

	/// <summary>
	/// 设置标识符的限定符。
	/// </summary>
	/// <param name="qualifier">限定符。</param>
	/// <returns>当前标识符名称表达式构造器。</returns>
	public NameBuilder Qualifier(string qualifier)
	{
		this.qualifier = qualifier;
		return this;
	}

	/// <summary>
	/// 添加指定的类型参数。
	/// </summary>
	/// <param name="type">类型参数。</param>
	/// <returns>当前标识符名称表达式构造器。</returns>
	public NameBuilder TypeArgument(TypeBuilder type)
	{
		typeArguments.Add(type);
		return this;
	}

	/// <summary>
	/// 添加指定的类型参数。
	/// </summary>
	/// <param name="type">类型参数。</param>
	/// <returns>当前标识符名称表达式构造器。</returns>
	public NameBuilder TypeArgument(string type)
	{
		typeArguments.Add(SyntaxBuilder.Name(type));
		return this;
	}

	/// <summary>
	/// 构造标识符名称表达式。
	/// </summary>
	/// <param name="format">语法格式。</param>
	/// <returns>标识符名称表达式。</returns>
	public override NameSyntax GetSyntax(SyntaxFormat format)
	{
		SimpleNameSyntax simpleName;
		if (typeArguments.Count == 0)
		{
			simpleName = SyntaxFactory.IdentifierName(identifier);
		}
		else
		{
			TypeArgumentListSyntax typeArgs = SyntaxFactory.TypeArgumentList(
				SyntaxBuilder.SeparatedList(typeArguments.Select(arg => arg.GetSyntax(format)), format)
			);
			simpleName = SyntaxFactory.GenericName(identifier).WithTypeArgumentList(typeArgs);
		}
		if (qualifier == null)
		{
			return simpleName;
		}
		else
		{
			return SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(qualifier), simpleName);
		}
	}

	/// <summary>
	/// 允许从字符串隐式转换为 <see cref="NameBuilder"/> 对象。
	/// </summary>
	/// <param name="typeName">要转换的类型名称。</param>
	/// <returns>转换到的 <see cref="NameBuilder"/> 对象。</returns>
	public static implicit operator NameBuilder(string typeName)
	{
		return new NameBuilder(typeName);
	}
}
