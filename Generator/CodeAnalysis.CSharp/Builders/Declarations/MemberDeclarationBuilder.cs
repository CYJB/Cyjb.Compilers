using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 成员声明的构造器。
/// </summary>
internal abstract class MemberDeclarationBuilder
{
	/// <summary>
	/// 文档注释的构造器。
	/// </summary>
	internal protected readonly DocumentationCommentTriviaBuilder commentBuilder = new();
	/// <summary>
	/// 特性列表。
	/// </summary>
	internal protected readonly AttributeListBuilder attributes = new();
	/// <summary>
	/// 成员的修饰符构造器。
	/// </summary>
	internal protected readonly ModifierBuilder modifiers = new();

	/// <summary>
	/// 构造成员声明语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>成员声明语法节点。</returns>
	public abstract MemberDeclarationSyntax GetSyntax(SyntaxFormat format);
}

/// <summary>
/// 提供成员声明构造器的辅助功能。
/// </summary>
internal static class MemberDeclarationBuilderUtil
{
	/// <summary>
	/// 设置成员声明的文档注释。
	/// </summary>
	/// <typeparam name="T">成员声明的类型。</typeparam>
	/// <param name="builder">成员声明构造器。</param>
	/// <param name="summary">摘要文档。</param>
	/// <returns>当前成员声明构造器。</returns>
	public static T Comment<T>(this T builder, params XmlNodeSyntaxOrString[] summary)
		where T : MemberDeclarationBuilder
	{
		builder.commentBuilder.Summary(summary);
		return builder;
	}

	/// <summary>
	/// 设置成员声明的文档注释。
	/// </summary>
	/// <typeparam name="T">成员声明的类型。</typeparam>
	/// <param name="builder">成员声明构造器。</param>
	/// <param name="action">文档注释处理器。</param>
	/// <returns>当前成员声明构造器。</returns>
	public static T Comment<T>(this T builder, Action<DocumentationCommentTriviaBuilder> action)
		where T : MemberDeclarationBuilder
	{
		action(builder.commentBuilder);
		return builder;
	}

	/// <summary>
	/// 设置成员声明的特性。
	/// </summary>
	/// <typeparam name="T">成员声明的类型。</typeparam>
	/// <param name="builder">成员声明构造器。</param>
	/// <param name="attribute">特性构造器。</param>
	/// <returns>当前成员声明构造器。</returns>
	public static T Attribute<T>(this T builder, AttributeBuilder attribute)
		where T : MemberDeclarationBuilder
	{
		builder.attributes.Add(attribute);
		return builder;
	}

	/// <summary>
	/// 设置成员声明的特性。
	/// </summary>
	/// <typeparam name="T">成员声明的类型。</typeparam>
	/// <param name="builder">成员声明构造器。</param>
	/// <param name="name">特性的名称。</param>
	/// <returns>当前成员声明声明构造器。</returns>
	public static T Attribute<T>(this T builder, NameBuilder name)
		where T : MemberDeclarationBuilder
	{
		builder.attributes.Add(SyntaxBuilder.Attribute(name));
		return builder;
	}

	/// <summary>
	/// 设置成员声明的修饰符。
	/// </summary>
	/// <typeparam name="T">成员声明的类型。</typeparam>
	/// <param name="builder">成员声明构造器。</param>
	/// <param name="modifiers">要设置的修饰符。</param>
	/// <returns>当前成员声明声明构造器。</returns>
	public static T Modifier<T>(this T builder, params SyntaxKind[] modifiers)
		where T : MemberDeclarationBuilder
	{
		builder.modifiers.Add(modifiers);
		return builder;
	}
}
