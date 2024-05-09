using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 方法声明的构造器。
/// </summary>
internal sealed class MethodDeclarationBuilder : BaseMethodDeclarationBuilder
{
	/// <summary>
	/// 方法的返回值。
	/// </summary>
	private readonly TypeBuilder returnType;
	/// <summary>
	/// 方法的名称。
	/// </summary>
	private readonly string methodName;

	/// <summary>
	/// 使用指定的方法返回类型和名称初始化 <see cref="MethodDeclarationBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="returnType">方法的返回类型。</param>
	/// <param name="methodName">方法的名称。</param>
	public MethodDeclarationBuilder(TypeBuilder returnType, string methodName)
	{
		this.returnType = returnType;
		this.methodName = methodName;
	}

	/// <summary>
	/// 构造方法声明语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>方法声明语法节点。</returns>
	public override MethodDeclarationSyntax GetSyntax(SyntaxFormat format)
	{
		SyntaxList<AttributeListSyntax> attributeLists = attributes.GetSyntax(format);
		SyntaxTokenList modifiers = this.modifiers.GetSyntax(format);
		var returnType = this.returnType.GetSyntax(format)
			.WithLeadingTrivia(this.modifiers.Count > 0 ? SyntaxFactory.Space : format.Indentation);
		var identifier = SyntaxFactory.Identifier(methodName).WithLeadingTrivia(SyntaxFactory.Space);
		ParameterListSyntax parameterList = parameters.GetSyntax(format);
		BlockSyntax? body = null;
		if (block != null)
		{
			body = block.GetSyntax(format)
				.WithLeadingTrivia(format.EndOfLine, format.Indentation)
				.WithTrailingTrivia(format.EndOfLine);
		}

		MethodDeclarationSyntax syntax = SyntaxFactory.MethodDeclaration(
			attributeLists, modifiers, returnType, null, identifier, null,
			parameterList, SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
			body, null
		);
		if (commentBuilder.HasComment)
		{
			syntax = syntax.InsertLeadingTrivia(0, SyntaxFactory.Trivia(commentBuilder.GetSyntax(format)));
		}
		return syntax;
	}
}
