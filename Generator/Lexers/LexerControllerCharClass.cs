using System.Globalization;
using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Lexers;

internal static class LexerControllerCharClass
{
	/// <summary>
	/// 字符类 Unicode 类别的参数名称。
	/// </summary>
	public static readonly string CategoriesVarName = "categories";

	/// <summary>
	/// 定义词法分析器数据的字符类 Unicode 类别。
	/// </summary>
	/// <param name="method">方法声明。</param>
	/// <param name="data">词法分析器数据</param>
	public static MethodDeclarationBuilder DefineCharClassCategories(this MethodDeclarationBuilder method,
		LexerData<SymbolKind> data)
	{
		if (data.CharClasses.Categories == null)
		{
			return method;
		}
		var categories = SyntaxBuilder.CreateObject().InitializerWrap(1);
		foreach (var pair in data.CharClasses.Categories!)
		{
			categories.Initializer(SyntaxBuilder.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression)
				.Add(SyntaxBuilder.Type<UnicodeCategory>().AccessMember(pair.Key.ToString()))
				.Add(SyntaxBuilder.Literal(pair.Value)));
		}
		method.Statement(SyntaxBuilder
			.DeclareLocal(SyntaxBuilder.Type<Dictionary<UnicodeCategory, int>>(), CategoriesVarName)
			.Comment("字符类 Unicode 类别")
			.Value(categories)
		);
		return method;
	}
}
