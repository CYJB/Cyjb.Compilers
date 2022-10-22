using Cyjb.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Parsers;

internal sealed partial class ParserController
{
	/// <summary>
	/// 返回指定语法分析器数据的转移检查。
	/// </summary>
	/// <param name="gotoCheck">转移的检查数据</param>
	/// <returns>转移检查数据。</returns>
	private static ExpressionBuilder GotoCheck(SymbolKind[] gotoCheck)
	{
		var builder = SyntaxBuilder.CreateArray().InitializerWrap(1);
		foreach (SymbolKind value in gotoCheck)
		{
			builder.Initializer(value.Syntax);
		}
		return builder;
	}
}

