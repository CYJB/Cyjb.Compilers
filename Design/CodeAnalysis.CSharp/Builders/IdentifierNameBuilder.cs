using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 标识符名称表达式的构造器。
/// </summary>
internal sealed class IdentifierNameBuilder : ExpressionBuilder
{
	/// <summary>
	/// 标识符名称。
	/// </summary>
	private readonly string identifier;

	/// <summary>
	/// 使用指定的标识符名称初始化 <see cref="IdentifierNameBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="identifier">标识符名称。</param>
	public IdentifierNameBuilder(string identifier)
	{
		this.identifier = identifier;
	}

	/// <summary>
	/// 构造标识符名称表达式。
	/// </summary>
	/// <param name="format">语法格式。</param>
	/// <returns>标识符名称表达式。</returns>
	public override IdentifierNameSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.IdentifierName(identifier);
	}
}
