using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 变量声明语句的构造器。
/// </summary>
internal sealed class LocalDeclarationStatementBuilder : StatementBuilder
{
	/// <summary>
	/// 变量的名称。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string name;
	/// <summary>
	/// 变量的类型。
	/// </summary>
	private readonly TypeBuilder type;
	/// <summary>
	/// 变量的初始值。
	/// </summary>
	private ExpressionBuilder? value;

	/// <summary>
	/// 使用指定的变量类型和名称初始化 <see cref="LocalDeclarationStatementBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="type">变量的类型。</param>
	/// <param name="name">变量的名称。</param>
	public LocalDeclarationStatementBuilder(TypeBuilder type, string name)
	{
		this.type = type;
		this.name = name;
	}

	/// <summary>
	/// 设置变量的值。
	/// </summary>
	/// <param name="value">变量的值。</param>
	/// <returns>当前变量声明语句构造器。</returns>
	public LocalDeclarationStatementBuilder Value(ExpressionBuilder value)
	{
		this.value = value;
		return this;
	}

	/// <summary>
	/// 设置语句的注释。
	/// </summary>
	/// <param name="comment">注释的内容。</param>
	/// <returns>当前变量声明语句构造器。</returns>
	public new LocalDeclarationStatementBuilder Comment(string? comment)
	{
		base.Comment(comment);
		return this;
	}

	/// <summary>
	/// 获取变量的名称。
	/// </summary>
	public string Name => name;

	/// <summary>
	/// 构造变量声明语句。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>变量声明语句。</returns>
	public override LocalDeclarationStatementSyntax GetSyntax(SyntaxFormat format)
	{
		VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(name);
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

		var syntax = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
			type.GetSyntax(format),
			SyntaxFactory.SingletonSeparatedList(declarator.WithLeadingTrivia(SyntaxFactory.Space))
		));

		syntax = syntax.WithLeadingTrivia(GetLeadingTrivia(format)).WithTrailingTrivia(format.EndOfLine);
		return syntax;
	}
}
