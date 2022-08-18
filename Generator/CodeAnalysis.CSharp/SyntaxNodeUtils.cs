using Microsoft.CodeAnalysis;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 提供 <see cref="SyntaxNode"/> 的扩展方法。
/// </summary>
internal static class SyntaxNodeUtils
{
	/// <summary>
	/// 插入指定的前置琐事。
	/// </summary>
	/// <typeparam name="TNode">语法节点的类型。</typeparam>
	/// <param name="node">语法节点。</param>
	/// <param name="index">要插入的索引。</param>
	/// <param name="trivia">要插入的琐事。</param>
	/// <returns>更新后的语法节点。</returns>
	public static TNode InsertLeadingTrivia<TNode>(this TNode node, int index, IEnumerable<SyntaxTrivia>? trivia)
		where TNode : SyntaxNode
	{
		if (trivia == null)
		{
			return node;
		}
		return node.WithLeadingTrivia(node.GetLeadingTrivia().InsertRange(index, trivia));
	}

	/// <summary>
	/// 插入指定的前置琐事。
	/// </summary>
	/// <typeparam name="TNode">语法节点的类型。</typeparam>
	/// <param name="node">语法节点。</param>
	/// <param name="index">要插入的索引。</param>
	/// <param name="trivia">要插入的琐事。</param>
	/// <returns>更新后的语法节点。</returns>
	public static TNode InsertLeadingTrivia<TNode>(this TNode node, int index, params SyntaxTrivia[] trivia)
		where TNode : SyntaxNode
	{
		return InsertLeadingTrivia(node, index, (IEnumerable<SyntaxTrivia>)trivia);
	}

	/// <summary>
	/// 添加指定的后置琐事。
	/// </summary>
	/// <typeparam name="TNode">语法节点的类型。</typeparam>
	/// <param name="node">语法节点。</param>
	/// <param name="trivia">要添加的琐事。</param>
	/// <returns>更新后的语法节点。</returns>
	public static TNode AddTrailingTrivia<TNode>(this TNode node, params SyntaxTrivia[] trivia)
		where TNode : SyntaxNode
	{
		if (trivia.Length == 0)
		{
			return node;
		}
		return node.WithTrailingTrivia(node.GetTrailingTrivia().AddRange(trivia));
	}

	/// <summary>
	/// 添加指定的后置琐事。
	/// </summary>
	/// <typeparam name="TNode">语法节点的类型。</typeparam>
	/// <param name="node">语法节点。</param>
	/// <param name="trivia">要添加的琐事。</param>
	/// <returns>更新后的语法节点。</returns>
	public static TNode AddTrailingTrivia<TNode>(this TNode node, IEnumerable<SyntaxTrivia> trivia)
		where TNode : SyntaxNode
	{
		return node.WithTrailingTrivia(node.GetTrailingTrivia().AddRange(trivia));
	}
}
