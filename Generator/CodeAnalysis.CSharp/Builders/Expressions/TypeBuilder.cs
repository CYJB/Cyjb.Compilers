using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 类型的构造器。
/// </summary>
internal sealed class TypeBuilder : ExpressionBuilder
{

	/// <summary>
	/// 类型语法单元。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TypeSyntax syntax;

	/// <summary>
	/// 使用指定的类型初始化 <see cref="TypeBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="type">类型语法单元。</param>
	public TypeBuilder(TypeSyntax type)
	{
		syntax = type;
	}

	/// <summary>
	/// 使用指定的类型初始化 <see cref="TypeBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="type">类型。</param>
	public TypeBuilder(Type type)
	{
		syntax = GetSyntax(type);
	}

	/// <summary>
	/// 使用指定的类型名称初始化 <see cref="TypeBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="typeName">类型名称。</param>
	public TypeBuilder(string typeName)
	{
		syntax = SyntaxFactory.ParseTypeName(typeName);
	}

	/// <summary>
	/// 返回指定类型的类型语法节点。
	/// </summary>
	/// <param name="type">要检查的类型。</param>
	/// <returns>指定类型的类型语法节点。</returns>
	private static TypeSyntax GetSyntax(Type type)
	{
		if (type.IsArray)
		{
			int rank = type.GetArrayRank();
			var rankSpecifier = SyntaxFactory.ArrayRankSpecifier(
				SyntaxFactory.SeparatedList<ExpressionSyntax>(
					Enumerable.Repeat(SyntaxFactory.OmittedArraySizeExpression(), rank)));
			TypeSyntax elementType = GetSyntax(type.GetElementType()!);
			if (elementType is ArrayTypeSyntax arrayType)
			{
				return arrayType.AddRankSpecifiers(rankSpecifier);
			}
			else
			{
				return SyntaxFactory.ArrayType(elementType, SyntaxFactory.SingletonList(rankSpecifier));
			}
		}
		return new NameBuilder(type).Syntax;
	}

	/// <summary>
	/// 添加指定的数组。
	/// </summary>
	/// <param name="rank">数组的秩。</param>
	/// <returns>类型构造器。</returns>
	public TypeBuilder Array(int rank = 1)
	{
		var rankSpecifier = SyntaxFactory.ArrayRankSpecifier(
			SyntaxFactory.SeparatedList<ExpressionSyntax>(
				Enumerable.Repeat(SyntaxFactory.OmittedArraySizeExpression(), rank)));
		if (syntax is ArrayTypeSyntax arrayType)
		{
			arrayType.AddRankSpecifiers(rankSpecifier);
		}
		else
		{
			syntax = SyntaxFactory.ArrayType(syntax, SyntaxFactory.SingletonList(rankSpecifier));
		}
		return this;
	}

	/// <summary>
	/// 获取类型语法单元。
	/// </summary>
	public TypeSyntax Syntax => syntax;

	/// <summary>
	/// 构造类型法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>类型语法节点。</returns>
	public override TypeSyntax GetSyntax(SyntaxFormat format)
	{
		return syntax;
	}

	/// <summary>
	/// 返回当前类型构造器的副本。
	/// </summary>
	/// <returns>当前类型构造器的副本。</returns>
	public TypeBuilder Clone()
	{
		return new TypeBuilder(syntax);
	}

	/// <summary>
	/// 允许从字符串隐式转换为 <see cref="TypeBuilder"/> 对象。
	/// </summary>
	/// <param name="typeName">要转换的类型名称。</param>
	/// <returns>转换到的 <see cref="TypeBuilder"/> 对象。</returns>
	public static implicit operator TypeBuilder(string typeName)
	{
		return new TypeBuilder(typeName);
	}

	/// <summary>
	/// 允许从 <see cref="Type"/> 隐式转换为 <see cref="TypeBuilder"/> 对象。
	/// </summary>
	/// <param name="type">要转换的类型。</param>
	/// <returns>转换到的 <see cref="TypeBuilder"/> 对象。</returns>
	public static implicit operator TypeBuilder(Type type)
	{
		return new TypeBuilder(type);
	}

	/// <summary>
	/// 允许从 <see cref="TypeSyntax"/> 隐式转换为 <see cref="TypeBuilder"/> 对象。
	/// </summary>
	/// <param name="type">要转换的类型语法节点。</param>
	/// <returns>转换到的 <see cref="TypeBuilder"/> 对象。</returns>
	public static implicit operator TypeBuilder(TypeSyntax type)
	{
		return new TypeBuilder(type);
	}

	/// <summary>
	/// 允许从 <see cref="NameBuilder"/> 隐式转换为 <see cref="TypeBuilder"/> 对象。
	/// </summary>
	/// <param name="typeName">要转换的类型名称。</param>
	/// <returns>转换到的 <see cref="TypeBuilder"/> 对象。</returns>
	public static implicit operator TypeBuilder(NameBuilder typeName)
	{
		return new TypeBuilder(typeName.Syntax);
	}
}
