namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 方法声明的基构造器。
/// </summary>
internal abstract class BaseMethodDeclarationBuilder : MemberDeclarationBuilder
{
	/// <summary>
	/// 方法的参数列表。
	/// </summary>
	internal protected readonly ParameterListBuilder parameters = new();
	/// <summary>
	/// 方法体构造器。
	/// </summary>
	internal protected BlockBuilder? block;
}

/// <summary>
/// 提供方法声明基构造器的辅助功能。
/// </summary>
internal static class BaseMethodDeclarationBuilderUtil
{
	/// <summary>
	/// 添加方法声明的参数。
	/// </summary>
	/// <typeparam name="T">方法声明的类型。</typeparam>
	/// <param name="name">参数的名称。</param>
	/// <param name="type">参数的类型。</param>
	/// <returns>当前方法声明构造器。</returns>
	public static T Parameter<T>(this T builder, string name, TypeBuilder type)
		where T : BaseMethodDeclarationBuilder
	{
		builder.parameters.Add(name, type);
		return builder;
	}

	/// <summary>
	/// 添加方法体的语句。
	/// </summary>
	/// <typeparam name="T">方法声明的类型。</typeparam>
	/// <param name="statement">语句。</param>
	/// <returns>当前方法声明构造器。</returns>
	public static T Statement<T>(this T builder, StatementBuilder? statement)
		where T : BaseMethodDeclarationBuilder
	{
		if (statement != null)
		{
			builder.block ??= new BlockBuilder();
			builder.block.Add(statement);
		}
		return builder;
	}
}
