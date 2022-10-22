using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 类型的构造器。
/// </summary>
internal abstract class TypeBuilder : ExpressionBuilder
{
	/// <summary>
	/// 构造类型法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>类型语法节点。</returns>
	public override abstract TypeSyntax GetSyntax(SyntaxFormat format);
}
