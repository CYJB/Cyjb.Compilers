using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 修饰符的构造器。
/// </summary>
internal sealed class ModifierBuilder
{
	/// <summary>
	/// 修饰符列表。
	/// </summary>
	private readonly List<SyntaxKind> modifiers = new();

	/// <summary>
	/// 获取修饰符的个数。
	/// </summary>
	public int Count => modifiers.Count;

	/// <summary>
	/// 添加修饰符。
	/// </summary>
	/// <param name="modifiers">要添加的修饰符。</param>
	/// <returns>当前修饰符构造器。</returns>
	public ModifierBuilder Add(params SyntaxKind[] modifiers)
	{
		this.modifiers.AddRange(modifiers);
		return this;
	}

	/// <summary>
	/// 构造修饰符的节点列表。
	/// </summary>
	/// <param name="format">语法格式。</param>
	/// <returns>修饰符的节点列表。</returns>
	public SyntaxTokenList GetSyntax(SyntaxFormat format)
	{
		List<SyntaxToken> tokens = new();
		bool isFirst = true;
		foreach (SyntaxKind modifier in modifiers)
		{
			SyntaxToken token = SyntaxFactory.Token(modifier);
			if (isFirst)
			{
				token = token.WithLeadingTrivia(format.Indentation);
				isFirst = false;
			}
			else
			{
				token = token.WithLeadingTrivia(SyntaxFactory.Space);
			}
			tokens.Add(token);
		}
		return SyntaxFactory.TokenList(tokens);
	}
}
