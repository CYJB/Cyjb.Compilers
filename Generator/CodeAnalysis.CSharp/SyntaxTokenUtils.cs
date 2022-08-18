using Microsoft.CodeAnalysis;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 提供 <see cref="SyntaxToken"/> 的扩展方法。
/// </summary>
internal static class SyntaxTokenUtils
{

	/// <summary>
	/// 插入指定的前置琐事。
	/// </summary>
	/// <param name="token">语法单元。</param>
	/// <param name="index">要插入的索引。</param>
	/// <param name="trivia">要插入的琐事。</param>
	/// <returns>更新后的语法单元。</returns>
	public static SyntaxToken InsertLeadingTrivia(this SyntaxToken token, int index, IEnumerable<SyntaxTrivia>? trivia)
	{
		if (trivia == null)
		{
			return token;
		}
		return token.WithLeadingTrivia(token.LeadingTrivia.InsertRange(index, trivia));
	}
}
