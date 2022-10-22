using System.Text;
using Cyjb.CodeAnalysis.CSharp;
using Cyjb.Compilers.Lexers;
using Cyjb.Compilers.Parsers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers;

/// <summary>
/// 词法/语法分析控制器的访问器。
/// </summary>
internal sealed class ControllerVisitor : CSharpSyntaxVisitor
{
	/// <summary>
	/// 转换上下文。
	/// </summary>
	private readonly TransformationContext context = new();
	/// <summary>
	/// 控制器所属的命名空间。
	/// </summary>
	private readonly Stack<BaseNamespaceDeclarationSyntax> namespaceStack = new();
	/// <summary>
	/// 已找到的词法/语法分析控制器。
	/// </summary>
	private Controller? controller;
	/// <summary>
	/// 控制器的命名空间。
	/// </summary>
	private string controllerNamespace;
	/// <summary>
	/// 控制器的语法节点。
	/// </summary>
	private ClassDeclarationSyntax controllerSyntax;
	/// <summary>
	/// 语法格式化信息。
	/// </summary>
	private SyntaxFormat format;
	/// <summary>
	/// 控制器的命名空间。
	/// </summary>
	private BaseNamespaceDeclarationSyntax[] namespaces = Array.Empty<BaseNamespaceDeclarationSyntax>();

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

	public ControllerVisitor() { }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

	/// <summary>
	/// 获取或设置是否是主文件。
	/// </summary>
	public bool IsMain { get; set; } = true;
	/// <summary>
	/// 获取已找到的词法/语法分析控制器。
	/// </summary>
	public Controller? Controller => controller;
	/// <summary>
	/// 获取转换上下文。
	/// </summary>
	public TransformationContext Context => context;

	/// <summary>
	/// 访问编译单元。
	/// </summary>
	/// <param name="node">编译单元节点。</param>
	/// <returns>转换后的编译单元节点。</returns>
	public override void VisitCompilationUnit(CompilationUnitSyntax node)
	{
		if (IsMain)
		{
			format = new SyntaxFormat(node);
		}
		VisitMemberList(node.Members);
	}

	/// <summary>
	/// 访问命名空间声明。
	/// </summary>
	/// <param name="node">命名空间声明节点。</param>
	public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
	{
		namespaceStack.Push(node);
		VisitMemberList(node.Members);
		namespaceStack.Pop();
	}

	/// <summary>
	/// 访问文件级别命名空间声明。
	/// </summary>
	/// <param name="node">文件级别命名空间声明节点。</param>
	public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
	{
		namespaceStack.Push(node);
		VisitMemberList(node.Members);
		namespaceStack.Pop();
	}

	/// <summary>
	/// 访问类声明。
	/// </summary>
	/// <param name="node">类声明节点。</param>
	public override void VisitClassDeclaration(ClassDeclarationSyntax node)
	{
		// 要求是分部类
		if (!node.Modifiers.Any((modifier) => modifier.IsKind(SyntaxKind.PartialKeyword)))
		{
			return;
		}
		if (IsMain && controller == null)
		{
			// 检查是否继承 LexerController<T> 或 ParserController<T>
			GenericNameSyntax? baseType = node.BaseList?.Types.Select(GetCompilerController)
				.Where((type) => type != null).FirstOrDefault();
			if (baseType == null)
			{
				return;
			}
			controllerSyntax = node;
			controllerNamespace = GetNamespace();
			namespaces = namespaceStack.ToArray();
			string kindType = baseType.TypeArgumentList.Arguments[0].ToString();
			if (baseType.Identifier.ToString() == "LexerController")
			{
				controller = new LexerController(context, node, kindType);
			}
			else
			{
				controller = new ParserController(context, node, kindType);
			}
			controller.Parse(node);
		}
		else if (controller != null &&
			node.Identifier.ToString() == controller.Name &&
			GetNamespace() == controllerNamespace)
		{
			controller.Parse(node);
		}
	}

	/// <summary>
	/// 访问成员声明列表。
	/// </summary>
	/// <param name="list">成员声明列表。</param>
	private void VisitMemberList(SyntaxList<MemberDeclarationSyntax> list)
	{
		foreach (MemberDeclarationSyntax member in list)
		{
			if (member is ClassDeclarationSyntax classDeclaration)
			{
				VisitClassDeclaration(classDeclaration);
			}
			else if (member is BaseNamespaceDeclarationSyntax)
			{
				member.Accept(this);
			}
			// 忽略其它成员声明。
		}
	}

	/// <summary>
	/// 检查指定类型是否是 <c>LexerController&lt;T&gt;</c> 或 <c>ParserController&lt;T&gt;</c>，
	/// 并返回其类型。
	/// </summary>
	/// <param name="type">要检查的类型。</param>
	/// <returns>如果指定类型是 <c>LexerController&lt;T&gt;</c> 或 <c>ParserController&lt;T&gt;</c>，
	/// 则为其类型；否则为 <c>null</c>。</returns>
	private static GenericNameSyntax? GetCompilerController(BaseTypeSyntax type)
	{
		if (type.Type is NameSyntax name &&
			name.GetSimpleName() is GenericNameSyntax genericName &&
			genericName.TypeArgumentList.Arguments.Count == 1)
		{
			string text = genericName.Identifier.Text;
			if (text == "LexerController" || text == "ParserController")
			{
				return genericName;
			}
		}
		return null;
	}

	/// <summary>
	/// 返回当前命名空间。
	/// </summary>
	/// <returns>当前命名空间。</returns>
	private string GetNamespace()
	{
		StringBuilder text = new();
		foreach (BaseNamespaceDeclarationSyntax syntax in namespaceStack)
		{
			if (text.Length > 0)
			{
				text.Insert(0, '.');
			}
			text.Insert(0, syntax.Name);
		}
		return text.ToString();
	}

	/// <summary>
	/// 生成控制器代码。
	/// </summary>
	/// <returns>生成好的控制器代码。</returns>
	public string Generate()
	{
		if (controller == null || context.HasError)
		{
			return string.Empty;
		}
		// 将 BaseList 的 TrailingTrivia 添加到 openBraceToken 之前，避免丢失换行符。
		SyntaxToken openBraceToken = controllerSyntax.OpenBraceToken.InsertLeadingTrivia(0,
			controllerSyntax.BaseList!.GetTrailingTrivia());
		// 保留类声明前的空行
		IEnumerable<SyntaxTrivia> newLines = controllerSyntax.GetLeadingTrivia()
			.Where((trivia) => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
		MemberDeclarationSyntax declaration = SyntaxFactory.ClassDeclaration(
			SyntaxFactory.List<AttributeListSyntax>(),
			controllerSyntax.Modifiers,
			controllerSyntax.Keyword,
			controllerSyntax.Identifier,
			controllerSyntax.TypeParameterList,
			null,
			SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
			openBraceToken,
			SyntaxFactory.List(controller.Generate()),
			controllerSyntax.CloseBraceToken,
			controllerSyntax.SemicolonToken).InsertLeadingTrivia(0, newLines);

		foreach (BaseNamespaceDeclarationSyntax syntax in namespaces)
		{
			if (syntax is NamespaceDeclarationSyntax namespaceDecl)
			{
				declaration = SyntaxFactory.NamespaceDeclaration(
					new SyntaxList<AttributeListSyntax>(),
					namespaceDecl.Modifiers,
					namespaceDecl.NamespaceKeyword,
					namespaceDecl.Name,
					namespaceDecl.OpenBraceToken,
					new SyntaxList<ExternAliasDirectiveSyntax>(),
					namespaceDecl.Usings,
					new SyntaxList<MemberDeclarationSyntax>(declaration),
					namespaceDecl.CloseBraceToken,
					namespaceDecl.SemicolonToken);
			}
			else
			{
				var fileNamespaceDecl = (FileScopedNamespaceDeclarationSyntax)syntax;
				declaration = SyntaxFactory.FileScopedNamespaceDeclaration(
					new SyntaxList<AttributeListSyntax>(),
					fileNamespaceDecl.Modifiers,
					fileNamespaceDecl.NamespaceKeyword,
					fileNamespaceDecl.Name,
					fileNamespaceDecl.SemicolonToken,
					new SyntaxList<ExternAliasDirectiveSyntax>(),
					fileNamespaceDecl.Usings,
					new SyntaxList<MemberDeclarationSyntax>(declaration));
			}
		}

		NamespaceRewriter rewriter = new();
		declaration = (declaration.Accept(rewriter) as MemberDeclarationSyntax)!;

		var compilationUnit = SyntaxFactory.CompilationUnit(
			new SyntaxList<ExternAliasDirectiveSyntax>(),
			rewriter.GetUsing(format),
			new SyntaxList<AttributeListSyntax>(),
			new SyntaxList<MemberDeclarationSyntax>(declaration)
		).WithLeadingTrivia(
			SyntaxFactory.Comment("//------------------------------------------------------------------------------"),
			format.EndOfLine,
			SyntaxFactory.Comment("// <auto-generated>"),
			format.EndOfLine,
			SyntaxFactory.Comment("// 此代码由工具生成。"),
			format.EndOfLine,
			SyntaxFactory.Comment("//"),
			format.EndOfLine,
			SyntaxFactory.Comment("// 对此文件的更改可能会导致不正确的行为，并且如果"),
			format.EndOfLine,
			SyntaxFactory.Comment("// 重新生成代码，这些更改将会丢失。"),
			format.EndOfLine,
			SyntaxFactory.Comment("// </auto-generated>"),
			format.EndOfLine,
			SyntaxFactory.Comment("//------------------------------------------------------------------------------"),
			format.EndOfLine,
			format.EndOfLine
		);

		return compilationUnit.ToFullString();
	}
}
