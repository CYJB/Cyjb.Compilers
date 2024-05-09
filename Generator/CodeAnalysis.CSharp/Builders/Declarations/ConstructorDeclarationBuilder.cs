using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 构造函数声明的构造器。
/// </summary>
internal sealed class ConstructorDeclarationBuilder : BaseMethodDeclarationBuilder
{
	/// <summary>
	/// 构造函数的名称。
	/// </summary>
	private readonly string name;

	/// <summary>
	/// 初始化类型。
	/// </summary>
	private SyntaxKind initializer = SyntaxKind.None;

	/// <summary>
	/// 初始化方法的调用参数。
	/// </summary>
	private readonly ArgumentListBuilder initializerArguments = new();

	/// <summary>
	/// 使用指定的构造函数名称初始化 <see cref="ConstructorDeclarationBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="name">构造函数的名称。</param>
	public ConstructorDeclarationBuilder(string name)
	{
		this.name = name;
		block = new BlockBuilder();
	}

	/// <summary>
	/// 添加基类的初始化调用。
	/// </summary>
	/// <param name="arguments">初始化调用的参数。</param>
	/// <returns>当前构造函数声明构造器。</returns>
	public ConstructorDeclarationBuilder BaseInitializer(params ExpressionBuilder[] arguments)
	{
		initializer = SyntaxKind.BaseConstructorInitializer;
		initializerArguments.Add(arguments);
		return this;
	}

	/// <summary>
	/// 添加当前类的初始化调用。
	/// </summary>
	/// <param name="arguments">初始化调用的参数。</param>
	/// <returns>当前构造函数声明构造器。</returns>
	public ConstructorDeclarationBuilder ThisInitializer(params ExpressionBuilder[] arguments)
	{
		initializer = SyntaxKind.ThisConstructorInitializer;
		initializerArguments.Add(arguments);
		return this;
	}

	/// <summary>
	/// 构造构造函数声明语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>构造函数声明语法节点。</returns>
	public override ConstructorDeclarationSyntax GetSyntax(SyntaxFormat format)
	{
		SyntaxList<AttributeListSyntax> attributeLists = attributes.GetSyntax(format);
		SyntaxTokenList modifiers = this.modifiers.GetSyntax(format);
		var identifier = SyntaxFactory.Identifier(name)
			.WithLeadingTrivia(this.modifiers.Count > 0 ? SyntaxFactory.Space : format.Indentation);
		ParameterListSyntax parameterList = parameters.GetSyntax(format);
		ConstructorInitializerSyntax? initializer = null;
		if (this.initializer != SyntaxKind.None)
		{
			SyntaxKind keyword = SyntaxKind.ThisKeyword;
			if (this.initializer == SyntaxKind.BaseConstructorInitializer)
			{
				keyword = SyntaxKind.BaseKeyword;
			}
			initializer = SyntaxFactory.ConstructorInitializer(this.initializer,
				SyntaxFactory.Token(SyntaxKind.ColonToken)
					.WithTrailingTrivia(SyntaxFactory.Space),
				SyntaxFactory.Token(keyword),
				initializerArguments.GetSyntax(format));
		}

		BlockSyntax body = block!.GetSyntax(format)
			.WithLeadingTrivia(format.EndOfLine, format.Indentation)
			.WithTrailingTrivia(format.EndOfLine);

		ConstructorDeclarationSyntax syntax = SyntaxFactory.ConstructorDeclaration(
			attributeLists, modifiers, identifier, parameterList, initializer, body);
		if (commentBuilder.HasComment)
		{
			syntax = syntax.InsertLeadingTrivia(0, SyntaxFactory.Trivia(commentBuilder.GetSyntax(format)));
		}
		return syntax;
	}
}
