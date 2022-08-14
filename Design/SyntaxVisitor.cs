using Cyjb.CodeAnalysis.CSharp;
using Cyjb.Compilers.Lexers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers;

/// <summary>
/// 词法/语法分析的访问器。
/// </summary>
internal sealed class SyntaxVisitor : CSharpSyntaxVisitor<SyntaxNode>
{
	/// <summary>
	/// 转换上下文。
	/// </summary>
	private readonly TransformationContext context;

	/// <summary>
	/// 使用指定的转换上下文初始化 <see cref="SyntaxVisitor"/> 类的新实例。
	/// </summary>
	/// <param name="context">转换上下文。</param>
	public SyntaxVisitor(TransformationContext context)
	{
		this.context = context;
	}

	/// <summary>
	/// 访问编译单元。
	/// </summary>
	/// <param name="node">编译单元节点。</param>
	/// <returns>转换后的编译单元节点。</returns>
	public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
	{
		return SyntaxFactory.CompilationUnit(
			new SyntaxList<ExternAliasDirectiveSyntax>(),
			node.Usings,
			new SyntaxList<AttributeListSyntax>(),
			VisitMemberList(node.Members));
	}

	/// <summary>
	/// 访问命名空间声明。
	/// </summary>
	/// <param name="node">命名空间声明节点。</param>
	/// <returns>转换后的命名空间声明节点。</returns>
	public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
	{
		NamespaceDeclarationSyntax syntax = SyntaxFactory.NamespaceDeclaration(
			new SyntaxList<AttributeListSyntax>(),
			node.Modifiers,
			node.NamespaceKeyword,
			node.Name,
			node.OpenBraceToken,
			new SyntaxList<ExternAliasDirectiveSyntax>(),
			node.Usings,
			VisitMemberList(node.Members),
			node.CloseBraceToken,
			node.SemicolonToken);
		return syntax;
	}

	/// <summary>
	/// 访问文件级别命名空间声明。
	/// </summary>
	/// <param name="node">文件级别命名空间声明节点。</param>
	/// <returns>转换后的文件级别命名空间声明节点。</returns>
	public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
	{
		FileScopedNamespaceDeclarationSyntax syntax = SyntaxFactory.FileScopedNamespaceDeclaration(
			new SyntaxList<AttributeListSyntax>(),
			node.Modifiers,
			node.NamespaceKeyword,
			node.Name,
			node.SemicolonToken,
			new SyntaxList<ExternAliasDirectiveSyntax>(),
			node.Usings,
			VisitMemberList(node.Members));
		return syntax;
	}

	/// <summary>
	/// 访问类声明。
	/// </summary>
	/// <param name="node">类声明节点。</param>
	/// <returns>转换后的类声明节点。</returns>
	public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
	{
		// 要求是分部类
		if (!node.Modifiers.Any((modifier) => modifier.IsKind(SyntaxKind.PartialKeyword)))
		{
			return null;
		}
		// 检查是否继承 LexerController<T>
		TypeSyntax? kindType = node.BaseList?.Types.Select((type) =>
		{
			if (type.Type is NameSyntax name &&
				name.GetSimpleName() is GenericNameSyntax genericName &&
				genericName.Identifier.Text == "LexerController" &&
				genericName.TypeArgumentList.Arguments.Count == 1)
			{
				return genericName.TypeArgumentList.Arguments[0];
			}
			return null;
		}).Where((type) => type != null).FirstOrDefault();
		if (kindType == null)
		{
			return null;
		}
		LexerController controller = new(context, node, kindType);
		controller.Parse();
		if (context.HasError)
		{
			return null;
		}
		return controller.Generate();
	}

	/// <summary>
	/// 访问成员声明列表。
	/// </summary>
	/// <param name="list">成员声明列表。</param>
	/// <returns>转换后的成员声明列表。</returns>
	private SyntaxList<MemberDeclarationSyntax> VisitMemberList(SyntaxList<MemberDeclarationSyntax> list)
	{
		List<MemberDeclarationSyntax> newList = new();
		foreach (MemberDeclarationSyntax member in list)
		{
			if (member is ClassDeclarationSyntax classDeclaration)
			{
				if (VisitClassDeclaration(classDeclaration) is ClassDeclarationSyntax newClass)
				{
					newList.Add(newClass);
				}
			}
			else if (member is BaseNamespaceDeclarationSyntax)
			{
				newList.Add((member.Accept(this) as MemberDeclarationSyntax)!);
			}
			// 忽略其它成员声明。
		}
		return new SyntaxList<MemberDeclarationSyntax>(newList);
	}
}
