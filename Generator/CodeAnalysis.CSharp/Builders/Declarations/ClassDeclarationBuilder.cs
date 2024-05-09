using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 类声明的构造器。
/// </summary>
internal sealed class ClassDeclarationBuilder : MemberDeclarationBuilder
{
	/// <summary>
	/// 类的名称。
	/// </summary>
	private readonly string name;
	/// <summary>
	/// 类的基类。
	/// </summary>
	private readonly List<TypeBuilder> baseTypes = new();
	/// <summary>
	/// 类的成员构造器。
	/// </summary>
	private readonly List<MemberDeclarationBuilder> members = new();

	/// <summary>
	/// 使用指定的类名初始化 <see cref="ClassDeclarationBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="name">类的名称。</param>
	public ClassDeclarationBuilder(string name)
	{
		this.name = name;
	}

	/// <summary>
	/// 设置类的基类。
	/// </summary>
	/// <param name="baseType">要设置的基类。</param>
	/// <returns>当前类声明构造器。</returns>
	public ClassDeclarationBuilder BaseType(TypeBuilder baseType)
	{
		baseTypes.Add(baseType);
		return this;
	}

	/// <summary>
	/// 添加类的成员。
	/// </summary>
	/// <param name="member">要添加的成员。</param>
	/// <returns>当前类声明构造器。</returns>
	public ClassDeclarationBuilder Member(MemberDeclarationBuilder member)
	{
		members.Add(member);
		return this;
	}

	/// <summary>
	/// 构造类声明语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>类声明语法节点。</returns>
	public override ClassDeclarationSyntax GetSyntax(SyntaxFormat format)
	{
		var attributeLists = attributes.GetSyntax(format);
		var modifiers = this.modifiers.GetSyntax(format);
		var keyword = SyntaxFactory.Token(SyntaxKind.ClassKeyword);
		if (this.modifiers.Count > 0)
		{
			keyword = keyword.WithLeadingTrivia(SyntaxFactory.Space);
		}
		var identifier = SyntaxFactory.Identifier(name)
			.WithLeadingTrivia(SyntaxFactory.Space);
		BaseListSyntax? baseList = null;
		if (baseTypes.Count > 0)
		{
			baseList = SyntaxFactory.BaseList(
				SyntaxFactory.Token(SyntaxKind.ColonToken)
					.WithLeadingTrivia(SyntaxFactory.Space)
					.WithTrailingTrivia(SyntaxFactory.Space),
				SyntaxFactory.SeparatedList(
				baseTypes.Select(type => (BaseTypeSyntax)SyntaxFactory.SimpleBaseType(type.GetSyntax(format)))));
		}
		SyntaxFormat memberFormat = format.IncDepth();
		var members = SyntaxFactory.List(this.members.Select(member => member.GetSyntax(memberFormat)));
		ClassDeclarationSyntax syntax = SyntaxFactory.ClassDeclaration(
			attributeLists, modifiers, keyword, identifier, null, baseList,
			SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
			SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
				.WithLeadingTrivia(format.EndOfLine, format.Indentation)
				.WithTrailingTrivia(format.EndOfLine),
			members,
			SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
				.WithLeadingTrivia(format.Indentation),
			SyntaxFactory.Token(SyntaxKind.None)
		);
		syntax = syntax.WithCloseBraceToken(syntax.CloseBraceToken.WithTrailingTrivia(format.EndOfLine));
		if (commentBuilder.HasComment)
		{
			syntax = syntax.InsertLeadingTrivia(0, SyntaxFactory.Trivia(commentBuilder.GetSyntax(format)));
		}
		return syntax;
	}
}

