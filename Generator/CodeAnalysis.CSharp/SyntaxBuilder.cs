using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 提供构造语法实例的功能。
/// </summary>
internal static class SyntaxBuilder
{

	#region Expression

	/// <summary>
	/// 返回指定字符串的字面量表达式。
	/// </summary>
	/// <param name="value">字面量的值。</param>
	/// <returns>指定字符串的字面量表达式。</returns>
	public static ExpressionBuilder Literal(string? value)
	{
		if (value == null)
		{
			return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
		}
		else
		{
			return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
		}
	}

	/// <summary>
	/// 返回指定 <see cref="bool"/> 的字面量表达式。
	/// </summary>
	/// <param name="value">字面量的值。</param>
	/// <returns>指定 <see cref="bool"/> 的字面量表达式。</returns>
	public static ExpressionBuilder Literal(bool value)
	{
		if (value)
		{
			return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
		}
		else
		{
			return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
		}
	}

	/// <summary>
	/// 返回指定 <see cref="int"/> 的字面量表达式。
	/// </summary>
	/// <param name="value">字面量的值。</param>
	/// <returns>指定 <see cref="int"/> 的字面量表达式。</returns>
	public static ExpressionBuilder Literal(int value)
	{
		if (value == int.MinValue)
		{
			return Name("int.MinValue");
		}
		return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
	}

	/// <summary>
	/// 返回指定 <see cref="int[]"/> 的字面量数组表达式。
	/// </summary>
	/// <param name="value">字面量的值。</param>
	/// <param name="wrap">换行情况。</param>
	/// <returns>指定 <see cref="int[]"/> 的字面量数组表达式。</returns>
	public static ExpressionBuilder LiteralArray(IEnumerable<int> value, int wrap = 0)
	{
		var builder = CreateArray().InitializerWrap(wrap);
		bool isEmpty = true;
		foreach (int item in value)
		{
			builder.Initializer(Literal(item));
			isEmpty = false;
		}
		if (isEmpty)
		{
			return Name("Array").Qualifier("System").AccessMember("Empty<int>").Invoke();
		}
		else
		{
			return builder;
		}
	}

	/// <summary>
	/// 创建 typeof 表达式。
	/// </summary>
	/// <param name="type">类型。</param>
	/// <returns>typeof 表达式。</returns>
	public static TypeOfExpressionBuilder TypeOf(TypeBuilder type) => new(type);

	/// <summary>
	/// 创建名称构造器。
	/// </summary>
	/// <param name="name">名称。</param>
	/// <returns>名称构造器。</returns>
	public static NameBuilder Name(string name) => new(name);

	/// <summary>
	/// 创建类型构造器。
	/// </summary>
	/// <param name="type">类型。</param>
	/// <returns>类型构造器。</returns>
	public static TypeBuilder Type(string type) => new CustomTypeBuilder(type);

	/// <summary>
	/// 创建类型构造器。
	/// </summary>
	/// <param name="type">类型。</param>
	/// <returns>类型构造器。</returns>
	public static TypeBuilder Type(TypeSyntax type) => new CustomTypeBuilder(type);

	/// <summary>
	/// 创建类型构造器。
	/// </summary>
	/// <typeparam name="T">类型。</typeparam>
	/// <returns>类型构造器。</returns>
	public static TypeBuilder Type<T>() => new CustomTypeBuilder(typeof(T).ToString());

	#endregion // Expression

	/// <summary>
	/// 创建 using 指令。
	/// </summary>
	/// <param name="name">要引用的命名空间。</param>
	/// <param name="format">语法格式。</param>
	/// <returns>using 指令。</returns>
	public static UsingDirectiveSyntax UsingDirective(string name, SyntaxFormat format)
	{
		return SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(name).WithLeadingTrivia(SyntaxFactory.Space))
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

	#region Builder

	/// <summary>
	/// 创建初始化表达式的构造器。
	/// </summary>
	/// <param name="kind">初始化表达式的类型。</param>
	/// <returns>初始化表达式的构造器。</returns>
	public static InitializerExpressionBuilder InitializerExpression(SyntaxKind kind) => new(kind);

	/// <summary>
	/// 创建对象创建表达式的构造器。
	/// </summary>
	/// <returns>对象创建表达式的构造器。</returns>
	public static ObjectCreationExpressionBuilder CreateObject() => new();

	/// <summary>
	/// 创建对象创建表达式的构造器。
	/// </summary>
	/// <typeparam name="T">对象的类型。</typeparam>
	/// <returns>对象创建表达式的构造器。</returns>
	public static ObjectCreationExpressionBuilder CreateObject<T>() =>
		new ObjectCreationExpressionBuilder().Type(Type<T>());

	/// <summary>
	/// 创建数组创建表达式的构造器。
	/// </summary>
	/// <returns>数组创建表达式的构造器。</returns>
	public static ArrayCreationExpressionBuilder CreateArray() => new();

	/// <summary>
	/// 创建 Lambda 表达式的构造器。
	/// </summary>
	/// <returns>Lambda 表达式的构造器。</returns>
	public static LambdaExpressionBuilder Lambda() => new();

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
	/// <param name="type">变量的类型。</param>
	/// <param name="fieldName">变量的名称。</param>
	/// <returns>变量声明语句的构造器。</returns>
	public static LocalDeclarationStatementBuilder DeclareLocal(string type, string name) => new(Type(type), name);

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
	/// 创建特性的构造器。
	/// </summary>
	/// <param name="name">特性的名称。</param>
	/// <returns>特性的构造器。</returns>
	public static AttributeBuilder Attribute(string name) => new(name);

	/// <summary>
	/// 创建字段声明的构造器。
	/// </summary>
	/// <param name="type">字段的类型。</param>
	/// <param name="name">字段的名称。</param>
	/// <returns>字段声明的构造器。</returns>
	public static FieldDeclarationBuilder DeclareField(TypeBuilder type, string name) => new(type, name);

	/// <summary>
	/// 创建方法声明的构造器。
	/// </summary>
	/// <param name="returnType">方法的返回值类型。</param>
	/// <param name="name">方法的名称。</param>
	/// <returns>方法声明的构造器。</returns>
	public static MethodDeclarationBuilder DeclareMethod(TypeBuilder returnType, string name) =>
		new(returnType, name);

	#endregion // Builder

}
