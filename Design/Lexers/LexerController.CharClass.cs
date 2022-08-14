using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Lexers;

internal sealed partial class LexerController
{
	/// <summary>
	/// 返回指定词法分析器数据的字符类索引数据。
	/// </summary>
	/// <param name="data">词法分析器数据</param>
	/// <returns>字符类索引数据。</returns>
	public static ExpressionBuilder CharClassIndexes(LexerData<int> data)
	{
		var builder = SyntaxBuilder.ArrayCreationExpression().InitializerWrap(8);
		foreach (int value in data.CharClasses.Indexes)
		{
			builder.Initializer(SyntaxBuilder.LiteralExpression(value));
		}
		return builder;
	}

	/// <summary>
	/// 返回指定词法分析器数据的字符类列表数据。
	/// </summary>
	/// <param name="data">词法分析器数据</param>
	/// <returns>字符类列表数据。</returns>
	public static ExpressionBuilder CharClassClasses(LexerData<int> data)
	{
		var builder = SyntaxBuilder.ArrayCreationExpression().InitializerWrap(24);
		foreach (int value in data.CharClasses.CharClasses)
		{
			builder.Initializer(SyntaxBuilder.LiteralExpression(value));
		}
		return builder;
	}

	/// <summary>
	/// 返回指定词法分析器数据的字符类 Unicode 类别数据。
	/// </summary>
	/// <param name="data">词法分析器数据</param>
	/// <returns>字符类 Unicode 类别数据。</returns>
	public static ExpressionBuilder CharClassCategories(LexerData<int> data)
	{
		var builder = SyntaxBuilder.ObjectCreationExpression().InitializerWrap(1);
		foreach (var pair in data.CharClasses.Categories!)
		{
			builder.Initializer(SyntaxBuilder.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression)
				.Add(SyntaxBuilder.IdentifierName("UnicodeCategory").Access(pair.Key.ToString()))
				.Add(SyntaxBuilder.LiteralExpression(pair.Value)));
		}
		return builder;
	}
}
