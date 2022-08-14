using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// <see cref="SeparatedSyntaxList{TNode}"/> 相关的工具方法。
/// </summary>
internal static class SeparatedListUtils
{
	/// <summary>
	/// 逗号单元。
	/// </summary>
	private static readonly SyntaxToken CommaToken = SyntaxFactory.Token(SyntaxKind.CommaToken)
		.WithTrailingTrivia(SyntaxFactory.Space);

	/// <summary>
	/// 返回指定项个数的分隔符列表。
	/// </summary>
	/// <param name="itemCount">项的个数。</param>
	/// <returns>分隔符列表。</returns>
	public static IEnumerable<SyntaxToken> GetSeparators(int itemCount)
	{
		if (itemCount <= 1)
		{
			return Array.Empty<SyntaxToken>();
		}
		return Enumerable.Repeat(CommaToken, itemCount - 1);
	}
}
