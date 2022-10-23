using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 提取并移除所有命名空间。
/// </summary>
internal sealed class NamespaceRewriter : CSharpSyntaxRewriter
{
	/// <summary>
	/// 所有找到的命名空间。
	/// </summary>
	private readonly HashSet<string> namespaces = new();

	/// <summary>
	/// 返回 using 语句。
	/// </summary>
	/// <param name="format">语法格式。</param>
	/// <returns>using 语句。</returns>
	public SyntaxList<UsingDirectiveSyntax> GetUsing(SyntaxFormat format)
	{
		return new SyntaxList<UsingDirectiveSyntax>(namespaces
			.OrderBy((ns) => ns, NamespaceComparer.Default)
			.Select((ns) => SyntaxBuilder.UsingDirective(ns, format))
		);
	}

	/// <summary>
	/// 访问限定名称。
	/// </summary>
	/// <param name="node">要访问的节点。</param>
	/// <returns>访问结果。</returns>
	public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
	{
		// 不处理命名空间名称。
		if (node.Parent.IsKind(SyntaxKind.NamespaceDeclaration) ||
			node.Parent.IsKind(SyntaxKind.FileScopedNamespaceDeclaration))
		{
			return node;
		}
		else
		{
			namespaces.Add(node.Left.ToString());
			return (node.Right.Accept(this) as SimpleNameSyntax)!.WithLeadingTrivia(node.GetLeadingTrivia());
		}
	}

	/// <summary>
	/// 命名空间的比较器。
	/// </summary>
	private class NamespaceComparer : IComparer<string>
	{
		/// <summary>
		/// 比较器的默认实例。
		/// </summary>
		public static readonly NamespaceComparer Default = new();

		/// <summary>
		/// 比较指定的命名空间。
		/// </summary>
		/// <param name="left">要比较的第一个命名空间。</param>
		/// <param name="right">要比较的第二个命名空间。</param>
		/// <returns>两个命名空间的比较结果。</returns>
		public int Compare(string? left, string? right)
		{
			if (left == right)
			{
				return 0;
			}
			if (left == null)
			{
				return 1;
			}
			else if (right == null)
			{
				return -1;
			}
			// System 命名空间总是排在前面。
			if (IsSystem(left))
			{
				if (IsSystem(right))
				{
					return left.CompareTo(right);
				}
				return -1;
			}
			else if (IsSystem(right))
			{
				return 1;
			}
			return left.CompareTo(right);
		}

		/// <summary>
		/// 检查指定命名空间是否是 System。
		/// </summary>
		/// <param name="ns">要检查的命名空间。</param>
		/// <returns>指定命名空间是否是 System。</returns>
		private static bool IsSystem(string ns)
		{
			return ns.StartsWith("System") && (ns.Length == 6 || ns[6] == '.');
		}
	}
}
