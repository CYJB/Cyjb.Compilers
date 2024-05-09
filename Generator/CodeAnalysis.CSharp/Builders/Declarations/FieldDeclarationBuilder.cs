using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 字段声明的构造器。
/// </summary>
internal sealed class FieldDeclarationBuilder : MemberDeclarationBuilder
{
	/// <summary>
	/// 字段的名称。
	/// </summary>
	private readonly string fieldName;
	/// <summary>
	/// 字段的类型。
	/// </summary>
	private readonly TypeBuilder type;
	/// <summary>
	/// 字段的初始值。
	/// </summary>
	private ExpressionBuilder? value;

	/// <summary>
	/// 使用指定的字段类型和名称初始化 <see cref="FieldDeclarationBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="type">字段的类型。</param>
	/// <param name="fieldName">字段名称。</param>
	public FieldDeclarationBuilder(TypeBuilder type, string fieldName)
	{
		this.type = type;
		this.fieldName = fieldName;
	}

	/// <summary>
	/// 设置字段的初始值。
	/// </summary>
	/// <param name="value">字段的初始值。</param>
	/// <returns>当前字段声明构造器。</returns>
	public FieldDeclarationBuilder Value(ExpressionBuilder value)
	{
		this.value = value;
		return this;
	}

	/// <summary>
	/// 构造字段声明语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>字段声明语法节点。</returns>
	public override FieldDeclarationSyntax GetSyntax(SyntaxFormat format)
	{
		SyntaxList<AttributeListSyntax> attributeLists = attributes.GetSyntax(format);
		SyntaxTokenList modifiers = this.modifiers.GetSyntax(format);
		VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(fieldName);
		if (value != null)
		{
			declarator = declarator.WithInitializer(SyntaxFactory.EqualsValueClause(
				SyntaxFactory.Token(
					new SyntaxTriviaList(SyntaxFactory.Space),
					SyntaxKind.EqualsToken,
					new SyntaxTriviaList(SyntaxFactory.Space)
				),
				value.GetSyntax(format)
			));
		}

		FieldDeclarationSyntax syntax = SyntaxFactory.FieldDeclaration(
			attributeLists, modifiers,
			SyntaxFactory.VariableDeclaration(
				type.GetSyntax(format)
					.WithLeadingTrivia(this.modifiers.Count > 0 ? SyntaxFactory.Space : format.Indentation),
				SyntaxFactory.SingletonSeparatedList(declarator.WithLeadingTrivia(SyntaxFactory.Space))
			),
			SyntaxFactory.Token(SyntaxKind.SemicolonToken).WithTrailingTrivia(format.EndOfLine)
		);
		if (commentBuilder.HasComment)
		{
			syntax = syntax.InsertLeadingTrivia(0, SyntaxFactory.Trivia(commentBuilder.GetSyntax(format)));
		}
		return syntax;
	}
}
