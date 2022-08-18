using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 表示 <see cref="XmlNodeSyntax"/> 或者字符串。
/// </summary>
internal sealed class XmlNodeSyntaxOrString
{
	/// <summary>
	/// 使用指定的 <see cref="XmlNodeSyntax"/> 初始化 <see cref="XmlNodeSyntaxOrString"/> 类的新实例。
	/// </summary>
	/// <param name="node">要封装的 <see cref="XmlNodeSyntax"/>。</param>
	public XmlNodeSyntaxOrString(XmlNodeSyntax node)
	{
		Node = node;
	}

	/// <summary>
	/// 使用指定的字符串初始化 <see cref="XmlNodeSyntaxOrString"/> 类的新实例。
	/// </summary>
	/// <param name="text">要封装的字符串。</param>
	public XmlNodeSyntaxOrString(string text)
	{
		Node = SyntaxFactory.XmlText(text);
	}

	/// <summary>
	/// 包含的 <see cref="XmlNodeSyntax"/>。
	/// </summary>
	public XmlNodeSyntax Node { get; }

	/// <summary>
	/// 允许从 <see cref="XmlNodeSyntax"/> 实例隐式转换为 <see cref="XmlNodeSyntaxOrString"/> 实例。
	/// </summary>
	/// <param name="node">要封装的 <see cref="XmlNodeSyntax"/>。</param>
	public static implicit operator XmlNodeSyntaxOrString(XmlNodeSyntax node)
	{
		return new XmlNodeSyntaxOrString(node);
	}

	/// <summary>
	/// 允许从字符串实例隐式转换为 <see cref="XmlNodeSyntaxOrString"/> 实例。
	/// </summary>
	/// <param name="text">要封装的字符串。</param>
	public static implicit operator XmlNodeSyntaxOrString(string text)
	{
		return new XmlNodeSyntaxOrString(text);
	}
}
