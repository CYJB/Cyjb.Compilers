using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 语句的构造器。
/// </summary>
internal abstract class StatementBuilder
{
	/// <summary>
	/// 构造语句语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>语句语法节点。</returns>
	public abstract StatementSyntax GetSyntax(SyntaxFormat format);
}
