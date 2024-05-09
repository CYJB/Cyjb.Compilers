using System.Globalization;
using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Lexers;

internal sealed partial class LexerController
{
	/// <summary>
	/// 声明词法分析器数据的字符类 Unicode 类别。
	/// </summary>
	/// <param name="data">词法分析器数据</param>
	/// <returns>字符类 Unicode 类别声明与。</returns>
	public static LocalDeclarationStatementBuilder? DeclareCharClassCategories(LexerData<SymbolKind> data)
	{
		if (data.CharClasses.Categories == null)
		{
			return null;
		}
		var categories = ExpressionBuilder.CreateObject().InitializerWrap(1);
		foreach (var pair in data.CharClasses.Categories!)
		{
			categories.Initializer(ExpressionBuilder.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression)
				.Add(SyntaxBuilder.Type<UnicodeCategory>().AccessMember(pair.Key.ToString()))
				.Add(pair.Value));
		}
		return SyntaxBuilder.DeclareLocal<Dictionary<UnicodeCategory, int>>("categories")
			.Comment("字符类 Unicode 类别")
			.Value(categories);
	}
}
