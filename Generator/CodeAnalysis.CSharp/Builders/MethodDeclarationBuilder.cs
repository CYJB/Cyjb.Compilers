using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 方法声明的构造器。
/// </summary>
internal sealed class MethodDeclarationBuilder
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
	/// 文档注释的构造器。
	/// </summary>
	private readonly DocumentationCommentTriviaBuilder commentBuilder = new();
	/// <summary>
	/// 特性列表。
	/// </summary>
	private readonly List<AttributeBuilder> attributes = new();
	/// <summary>
	/// 方法的修饰符构造器。
	/// </summary>
	private readonly ModifierBuilder modifiers = new();
	/// <summary>
	/// 方法的参数列表。
	/// </summary>
	private readonly ParameterListBuilder parameters = new();
	/// <summary>
	/// 方法体构造器。
	/// </summary>
	private BlockBuilder? block;

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
	/// 设置方法的文档注释。
	/// </summary>
	/// <param name="summary">摘要文档。</param>
	/// <returns>当前方法声明构造器。</returns>
	public MethodDeclarationBuilder Comment(params XmlNodeSyntaxOrString[] summary)
	{
		commentBuilder.Summary(summary);
		return this;
	}

	/// <summary>
	/// 设置方法的文档注释。
	/// </summary>
	/// <param name="action">文档注释处理器。</param>
	/// <returns>当前方法声明构造器。</returns>
	public MethodDeclarationBuilder Comment(Action<DocumentationCommentTriviaBuilder> action)
	{
		action(commentBuilder);
		return this;
	}

	/// <summary>
	/// 设置方法的特性。
	/// </summary>
	/// <param name="attribute">特性构造器。</param>
	/// <returns>当前方法声明构造器。</returns>
	public MethodDeclarationBuilder Attribute(AttributeBuilder attribute)
	{
		attributes.Add(attribute);
		return this;
	}

	/// <summary>
	/// 设置方法的修饰符。
	/// </summary>
	/// <param name="modifiers">要设置的修饰符。</param>
	/// <returns>当前方法声明构造器。</returns>
	public MethodDeclarationBuilder Modifier(params SyntaxKind[] modifiers)
	{
		this.modifiers.Add(modifiers);
		return this;
	}

	/// <summary>
	/// 添加方法声明的参数。
	/// </summary>
	/// <param name="name">参数的名称。</param>
	/// <param name="type">参数的类型。</param>
	/// <returns>当前方法声明构造器。</returns>
	public MethodDeclarationBuilder Parameter(string name, TypeBuilder type)
	{
		parameters.Add(name, type);
		return this;
	}

	/// <summary>
	/// 添加方法体的语句。
	/// </summary>
	/// <param name="statement">语句。</param>
	/// <returns>当前方法声明构造器。</returns>
	public MethodDeclarationBuilder Statement(StatementBuilder? statement)
	{
		if (statement != null)
		{
			block ??= new BlockBuilder();
			block.Add(statement);
		}
		return this;
	}

	/// <summary>
	/// 构造方法声明语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>方法声明语法节点。</returns>
	public MethodDeclarationSyntax GetSyntax(SyntaxFormat format)
	{
		MethodDeclarationSyntax syntax = SyntaxFactory.MethodDeclaration(
			returnType.GetSyntax(format)
				.WithLeadingTrivia(modifiers.Count > 0 ? SyntaxFactory.Space : format.Indentation),
			SyntaxFactory.Identifier(methodName).WithLeadingTrivia(SyntaxFactory.Space)
		);
		if (modifiers.Count > 0)
		{
			syntax = syntax.WithModifiers(modifiers.GetSyntax(format));
		}
		if (parameters.Count > 0)
		{
			syntax = syntax.WithParameterList(parameters.GetSyntax(format));
		}
		if (block != null)
		{
			syntax = syntax.WithTrailingTrivia(format.EndOfLine)
				.WithBody(block.GetSyntax(format)).WithTrailingTrivia(format.EndOfLine);
		}
		if (attributes.Count > 0)
		{
			syntax = syntax.WithAttributeLists(SyntaxFactory.List(attributes.Select(
				attr => attr.GetListSyntax(format)
					.WithLeadingTrivia(format.Indentation)
					.WithTrailingTrivia(format.EndOfLine))));
		}
		if (commentBuilder.HasComment)
		{
			syntax = syntax.InsertLeadingTrivia(0, SyntaxFactory.Trivia(commentBuilder.GetSyntax(format)));
		}
		return syntax;
	}
}
