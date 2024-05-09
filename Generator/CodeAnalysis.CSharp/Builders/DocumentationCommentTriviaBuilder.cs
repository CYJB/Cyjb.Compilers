using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 文档注释的构造器。
/// </summary>
internal sealed class DocumentationCommentTriviaBuilder
{
	/// <summary>
	/// 文档注释的字符。
	/// </summary>
	private static readonly SyntaxTrivia Exterior = SyntaxFactory.DocumentationCommentExterior("///");

	/// <summary>
	/// 摘要文档。
	/// </summary>
	private XmlNodeSyntaxOrString[]? summary;
	/// <summary>
	/// 参数文档。
	/// </summary>
	private readonly List<KeyValuePair<string, XmlNodeSyntaxOrString[]>> paramList = new();
	/// <summary>
	/// 返回值文档。
	/// </summary>
	private XmlNodeSyntaxOrString[]? returns;
	/// <summary>
	/// 异常文档。
	/// </summary>
	private readonly List<KeyValuePair<string, XmlNodeSyntaxOrString[]>> exceptions = new();
	/// <summary>
	/// 重载文档。
	/// </summary>
	private XmlNodeSyntaxOrString[]? overloads;

	/// <summary>
	/// 获取是否包含文档注释。
	/// </summary>
	public bool HasComment => summary?.Length > 0 || paramList.Count > 0 ||
		returns?.Length > 0 || exceptions.Count > 0 || overloads?.Length > 0;

	/// <summary>
	/// 添加摘要文档。
	/// </summary>
	/// <param name="comment">摘要文档。</param>
	/// <returns>当前文档注释的构造器。</returns>
	public DocumentationCommentTriviaBuilder Summary(params XmlNodeSyntaxOrString[] comment)
	{
		summary = comment;
		return this;
	}

	/// <summary>
	/// 添加参数文档。
	/// </summary>
	/// <param name="paramName">参数名称。</param>
	/// <param name="comment">参数的文档。</param>
	/// <returns>当前文档注释的构造器。</returns>
	public DocumentationCommentTriviaBuilder Param(string paramName, params XmlNodeSyntaxOrString[] comment)
	{
		paramList.Add(new KeyValuePair<string, XmlNodeSyntaxOrString[]>(paramName, comment));
		return this;
	}

	/// <summary>
	/// 添加返回值文档。
	/// </summary>
	/// <param name="comment">返回值文档。</param>
	/// <returns>当前文档注释的构造器。</returns>
	public DocumentationCommentTriviaBuilder Returns(params XmlNodeSyntaxOrString[] comment)
	{
		returns = comment;
		return this;
	}

	/// <summary>
	/// 添加异常文档。
	/// </summary>
	/// <param name="exceptionType">异常类型。</param>
	/// <param name="comment">异常的文档。</param>
	/// <returns>当前文档注释的构造器。</returns>
	public DocumentationCommentTriviaBuilder Exception(string exceptionType, params XmlNodeSyntaxOrString[] comment)
	{
		exceptions.Add(new KeyValuePair<string, XmlNodeSyntaxOrString[]>(exceptionType, comment));
		return this;
	}

	/// <summary>
	/// 添加重载文档。
	/// </summary>
	/// <param name="comment">重载文档。</param>
	/// <returns>当前文档注释的构造器。</returns>
	public DocumentationCommentTriviaBuilder Overloads(params XmlNodeSyntaxOrString[] comment)
	{
		overloads = comment;
		return this;
	}

	/// <summary>
	/// 构造文档注释语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>文档注释语法节点。</returns>
	public DocumentationCommentTriviaSyntax GetSyntax(SyntaxFormat format)
	{
		List<XmlNodeSyntax> nodes = new();
		XmlTextSyntax startExterior = SyntaxFactory.XmlText(SyntaxFactory.XmlTextLiteral(string.Empty)
			.WithLeadingTrivia(format.Indentation, Exterior, SyntaxFactory.Space));
		XmlTextSyntax newLine = SyntaxFactory.XmlText(SyntaxFactory.XmlTextLiteral(format.EndOfLine.ToString()));
		// 添加摘要。
		if (summary?.Length > 0)
		{
			nodes.Add(startExterior);
			nodes.Add(SyntaxFactory.XmlSummaryElement(GetExteriorList(summary, newLine, startExterior)));
			nodes.Add(newLine);
		}
		// 添加参数。
		if (paramList.Count > 0)
		{
			foreach (var pair in paramList)
			{
				nodes.Add(startExterior);
				nodes.Add(SyntaxFactory.XmlParamElement(pair.Key, GetList(pair.Value)));
				nodes.Add(newLine);
			}
		}
		// 添加返回值
		if (returns?.Length > 0)
		{
			nodes.Add(startExterior);
			nodes.Add(SyntaxFactory.XmlReturnsElement(GetList(returns)));
			nodes.Add(newLine);
		}
		// 添加异常
		if (exceptions.Count > 0)
		{
			foreach (var pair in exceptions)
			{
				nodes.Add(startExterior);
				nodes.Add(SyntaxFactory.XmlExceptionElement(
					SyntaxFactory.TypeCref(SyntaxFactory.IdentifierName(pair.Key)),
					GetList(pair.Value)
				));
				nodes.Add(newLine);
			}
		}
		// 添加重载
		if (overloads?.Length > 0)
		{
			nodes.Add(startExterior);
			nodes.Add(SyntaxFactory.XmlElement("overloads",
				SyntaxFactory.List(new XmlNodeSyntax[] {
					newLine,
					startExterior,
					SyntaxFactory.XmlSummaryElement(GetExteriorList(overloads, newLine, startExterior)),
					newLine,
					startExterior
				})
			));
			nodes.Add(newLine);
		}
		return SyntaxFactory.DocumentationCommentTrivia(
			SyntaxKind.SingleLineDocumentationCommentTrivia,
			SyntaxFactory.List(nodes)
		);
	}

	/// <summary>
	/// 返回指定 <see cref="XmlNodeSyntaxOrString"/> 数组对应的列表。
	/// </summary>
	/// <param name="nodes">要检查的数组。</param>
	/// <returns>列表实例。</returns>
	private static SyntaxList<XmlNodeSyntax> GetList(XmlNodeSyntaxOrString[] nodes)
	{
		return SyntaxFactory.List(nodes.Select(node => node.Node));
	}

	/// <summary>
	/// 返回指定 <see cref="XmlNodeSyntaxOrString"/> 数组对应的列表。
	/// </summary>
	/// <param name="nodes">要检查的数组。</param>
	/// <param name="newLine">换行符。</param>
	/// <param name="startExterior">文档注释的起始字符。</param>
	/// <returns>列表实例。</returns>
	private static SyntaxList<XmlNodeSyntax> GetExteriorList(XmlNodeSyntaxOrString[] nodes,
		XmlTextSyntax newLine, XmlTextSyntax startExterior)
	{
		List<XmlNodeSyntax> list = new()
		{
			newLine,
			startExterior
		};
		list.AddRange(nodes.Select(node => node.Node));
		list.Add(newLine);
		list.Add(startExterior);
		return SyntaxFactory.List(list);
	}
}
