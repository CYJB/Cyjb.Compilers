using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.CodeAnalysis;

/// <summary>
/// 二元模式的构造器。
/// </summary>
internal class BinaryPatternBuilder : PatternBuilder
{
	/// <summary>
	/// 二元模式的类型。
	/// </summary>
	private readonly SyntaxKind kind;
	/// <summary>
	/// 二元模式左侧的模式。
	/// </summary>
	private readonly PatternBuilder left;
	/// <summary>
	/// 二元模式右侧的模式。
	/// </summary>
	private readonly PatternBuilder right;

	/// <summary>
	/// 使用指定的模式初始化 <see cref="BinaryPatternBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="kind">模式的类型。</param>
	/// <param name="left">左侧的模式。</param>
	/// <param name="right">右侧的模式。</param>
	public BinaryPatternBuilder(SyntaxKind kind, PatternBuilder left, PatternBuilder right)
	{
		this.kind = kind;
		this.left = left;
		this.right = right;
	}

	/// <summary>
	/// 构造二元模式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>二元模式。</returns>
	public override BinaryPatternSyntax GetSyntax(SyntaxFormat format)
	{
		return SyntaxFactory.BinaryPattern(kind,
			left.GetSyntax(format).WithTrailingTrivia(SyntaxFactory.Space),
			right.GetSyntax(format).WithLeadingTrivia(SyntaxFactory.Space));
	}
}
