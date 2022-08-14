using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 语法节点的缩进检测器。
/// </summary>
internal class IndentDetector : CSharpSyntaxWalker
{
	/// <summary>
	/// 检测到的缩进。
	/// </summary>
	private string indent = string.Empty;

	/// <summary>
	/// 获取当前缩进。
	/// </summary>
	public string Indent => indent;

	/// <summary>
	/// 访问指定的语法节点。
	/// </summary>
	/// <param name="node">要访问的语法节点。</param>
	public override void Visit(SyntaxNode? node)
	{
		if (node == null)
		{
			return;
		}
		if (indent.Length > 0)
		{
			// 已经检测到缩进，返回。
			return;
		}
		// 找到成员声明或语句的首个空白琐事作为缩进。
		if (node is MemberDeclarationSyntax || node is StatementSyntax)
		{
			indent = GetIndent(node);
		}
		base.Visit(node);
	}

	/// <summary>
	/// 返回指定语法节点的缩进。
	/// </summary>
	/// <param name="node">要检查的语法节点。</param>
	/// <returns>语法节点的缩进。</returns>
	public static string GetIndent(SyntaxNode? node)
	{
		if (node == null)
		{
			return string.Empty;
		}
		foreach (SyntaxTrivia trivia in node.GetLeadingTrivia())
		{
			if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
			{
				continue;
			}
			else if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
			{
				return trivia.ToString();
			}
			break;
		}
		return string.Empty;
	}
}
