using Cyjb.Compilers.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 提供构造语法实例的功能。
/// </summary>
internal static class SyntaxBuilder
{
	/// <summary>
	/// 创建类型构造器。
	/// </summary>
	/// <typeparam name="T">类型。</typeparam>
	/// <returns>类型构造器。</returns>
	public static TypeBuilder Type<T>() => new(typeof(T));

	/// <summary>
	/// 创建 using 指令。
	/// </summary>
	/// <param name="name">要引用的命名空间。</param>
	/// <param name="format">语法格式。</param>
	/// <returns>using 指令。</returns>
	public static UsingDirectiveSyntax UsingDirective(string name, SyntaxFormat format)
	{
		return SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(name)
			.WithLeadingTrivia(SyntaxFactory.Space))
			.WithTrailingTrivia(format.EndOfLine);
	}

	/// <summary>
	/// 创建分割列表。
	/// </summary>
	/// <typeparam name="TNode">列表包含的节点类型。</typeparam>
	/// <param name="nodes">列表包含的节点。</param>
	/// <param name="format">语法格式。</param>
	/// <param name="wrap">换行情况，使用 <c>0</c> 表示不换行。</param>
	/// <param name="startNewLine">列表是否从换行符开始。</param>
	/// <returns>分割列表。</returns>
	public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<TNode> nodes, SyntaxFormat format,
		int wrap = 0, bool startNewLine = false)
		 where TNode : SyntaxNode
	{
		List<SyntaxNodeOrToken> list = new();
		foreach (TNode node in nodes)
		{
			list.Add(node);
			list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space));
		}
		if (list.Count > 0)
		{
			// 移除末尾的逗号。
			list.RemoveAt(list.Count - 1);
			if (wrap > 0)
			{
				wrap *= 2;
				if (startNewLine)
				{
					list[0] = list[0].AsNode()!.InsertLeadingTrivia(0, format.Indentation);
				}
				for (int i = wrap - 1; i < list.Count; i += wrap)
				{
					list[i] = list[i].WithTrailingTrivia(format.EndOfLine);
					list[i + 1] = list[i + 1].AsNode()!.InsertLeadingTrivia(0, format.Indentation);
				}
			}
		}
		return SyntaxFactory.SeparatedList<TNode>(list);
	}

	/// <summary>
	/// 创建特性的构造器。
	/// </summary>
	/// <param name="name">特性的名称。</param>
	/// <returns>特性的构造器。</returns>
	public static AttributeBuilder Attribute(NameBuilder name) => new(name);

	/// <summary>
	/// 创建特性的构造器。
	/// </summary>
	/// <typeparam name="T">特性的类型。</typeparam>
	/// <returns>特性的构造器。</returns>
	public static AttributeBuilder Attribute<T>() => new(typeof(T));

	#region Statement

	/// <summary>
	/// 创建语句块的构造器。
	/// </summary>
	/// <returns>语句块的构造器。</returns>
	public static BlockBuilder Block() => new();

	/// <summary>
	/// 创建变量声明语句的构造器。
	/// </summary>
	/// <param name="type">变量的类型。</param>
	/// <param name="fieldName">变量的名称。</param>
	/// <returns>变量声明语句的构造器。</returns>
	public static LocalDeclarationStatementBuilder DeclareLocal(TypeBuilder type, string name) => new(type, name);

	/// <summary>
	/// 创建变量声明语句的构造器。
	/// </summary>
	/// <typeparam name="T">变量的类型。</typeparam>
	/// <param name="fieldName">变量的名称。</param>
	/// <returns>变量声明语句的构造器。</returns>
	public static LocalDeclarationStatementBuilder DeclareLocal<T>(string name) => new(Type<T>(), name);

	/// <summary>
	/// 创建返回语句的构造器。
	/// </summary>
	/// <param name="expression">要返回的表达式。</param>
	/// <returns>返回语句的构造器。</returns>
	public static ReturnStatementBuilder Return(ExpressionBuilder expression) => new(expression);

	/// <summary>
	/// 创建 if 语句的构造器。
	/// </summary>
	/// <returns>if 语句的构造器。</returns>
	public static IfStatementBuilder If(ExpressionBuilder condition) => new(condition);

	/// <summary>
	/// 创建 while 语句的构造器。
	/// </summary>
	/// <param name="condition">while 语句的条件表达式。</param>
	/// <returns>while 语句的构造器。</returns>
	public static WhileStatementBuilder While(ExpressionBuilder condition) => new(condition);

	/// <summary>
	/// 创建 switch 语句的构造器。
	/// </summary>
	/// <param name="condition">switch 语句的条件表达式。</param>
	/// <returns>switch 语句的构造器。</returns>
	public static SwitchStatementBuilder Switch(ExpressionBuilder expression) => new(expression);

	/// <summary>
	/// 创建 switch 语句的段构造器。
	/// </summary>
	/// <returns>switch 语句的段构造器。</returns>
	public static SwitchSectionBuilder SwitchSection() => new();

	/// <summary>
	/// 创建 break 语句的构造器。
	/// </summary>
	/// <returns>break 语句的构造器。</returns>
	public static BreakStatementBuilder Break() => new();

	/// <summary>
	/// 创建 goto 语句的构造器。
	/// </summary>
	/// <returns>goto 语句的构造器。</returns>
	public static GotoStatementBuilder Goto(string identifier) => new(identifier);

	#endregion // Statement

	#region Declaration

	/// <summary>
	/// 创建字段声明的构造器。
	/// </summary>
	/// <param name="type">字段的类型。</param>
	/// <param name="name">字段的名称。</param>
	/// <returns>字段声明的构造器。</returns>
	public static FieldDeclarationBuilder DeclareField(TypeBuilder type, string name) => new(type, name);

	/// <summary>
	/// 创建字段声明的构造器。
	/// </summary>
	/// <typeparam name="T">字段的类型。</typeparam>
	/// <param name="name">字段的名称。</param>
	/// <returns>字段声明的构造器。</returns>
	public static FieldDeclarationBuilder DeclareField<T>(string name) => new(Type<T>(), name);

	/// <summary>
	/// 创建方法声明的构造器。
	/// </summary>
	/// <param name="returnType">方法的返回值类型。</param>
	/// <param name="name">方法的名称。</param>
	/// <returns>方法声明的构造器。</returns>
	public static MethodDeclarationBuilder DeclareMethod(TypeBuilder returnType, string name) =>
		new(returnType, name);

	/// <summary>
	/// 创建构造函数声明的构造器。
	/// </summary>
	/// <param name="name">构造函数的名称。</param>
	/// <returns>构造函数声明的构造器。</returns>
	public static ConstructorDeclarationBuilder DeclareConstructor(string name) =>
		new(name);

	/// <summary>
	/// 创建类声明的构造器。
	/// </summary>
	/// <param name="name">类的名称。</param>
	/// <returns>类声明的构造器。</returns>
	public static ClassDeclarationBuilder DeclareClass(string name) => new(name);

	#endregion // Declaration

}
