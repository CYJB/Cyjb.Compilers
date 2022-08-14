using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 特性的参数信息。
/// </summary>
internal class AttributeArguments
{
	/// <summary>
	/// 参数值的字典。
	/// </summary>
	private readonly Dictionary<string, ExpressionSyntax> arguments;

	/// <summary>
	/// 初始化 <see cref="AttributeArguments"/> 类的新实例。
	/// </summary>
	public AttributeArguments()
		: this(new Dictionary<string, ExpressionSyntax>(), Array.Empty<ExpressionSyntax>())
	{ }
	/// <summary>
	/// 使用特性的参数信息初始化 <see cref="AttributeArguments"/> 类的新实例。
	/// </summary>
	/// <param name="arguments">参数值的字典。</param>
	/// <param name="paramsArgument">params 参数值。</param>
	public AttributeArguments(Dictionary<string, ExpressionSyntax> arguments, ExpressionSyntax[] paramsArgument)
	{
		this.arguments = arguments;
		ParamsArgument = paramsArgument;
	}

	/// <summary>
	/// 获取指定名称参数的值。
	/// </summary>
	/// <param name="name">要获取值的参数名称。</param>
	/// <returns>指定名称参数的值，如果不存在则返回 <c>null</c>。</returns>
	public ExpressionSyntax? this[string name]
	{
		get
		{
			if (arguments.TryGetValue(name, out var value))
			{
				return value;
			}
			return null;
		}
	}

	/// <summary>
	/// params 参数的值。
	/// </summary>
	public ExpressionSyntax[] ParamsArgument { get; }
}
