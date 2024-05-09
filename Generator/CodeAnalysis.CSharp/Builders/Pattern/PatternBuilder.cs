using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.CodeAnalysis;

/// <summary>
/// 模式的构造器。
/// </summary>
internal abstract class PatternBuilder
{
	/// <summary>
	/// 返回小于等于模式。
	/// </summary>
	/// <returns>小于等于模式。</returns>
	public static RelationalPatternBuilder LessThanEquals(ExpressionBuilder expression) =>
		new(SyntaxKind.LessThanEqualsToken, expression);

	/// <summary>
	/// 返回大于等于模式。
	/// </summary>
	/// <returns>大于等于模式。</returns>
	public static RelationalPatternBuilder GreaterThanEquals(ExpressionBuilder expression) =>
		new(SyntaxKind.GreaterThanEqualsToken, expression);

	/// <summary>
	/// 构造模式语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>表达式语法节点。</returns>
	public abstract PatternSyntax GetSyntax(SyntaxFormat format);

	/// <summary>
	/// 返回当前模式与指定模式的构造器。
	/// </summary>
	/// <returns>二元模式构造器。</returns>
	public BinaryPatternBuilder And(PatternBuilder pattern)
	{
		return new BinaryPatternBuilder(SyntaxKind.AndPattern, this, pattern);
	}
}
