using Cyjb.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Lexers;

internal sealed partial class LexerController
{
	/// <summary>
	/// 返回指定词法分析器数据的状态数据。
	/// </summary>
	/// <param name="data">词法分析器数据</param>
	/// <returns>状态数据。</returns>
	private static ExpressionBuilder StatesValue(LexerData<SymbolKind> data)
	{
		var builder = SyntaxBuilder.CreateArray().InitializerWrap(1);
		foreach (DfaStateData state in data.States)
		{
			var stateBuilder = SyntaxBuilder.CreateObject<DfaStateData>()
				.Argument(SyntaxBuilder.Literal(state.BaseIndex))
				.Argument(SyntaxBuilder.Literal(state.DefaultState));
			foreach (int idx in state.Symbols)
			{
				stateBuilder.Argument(SyntaxBuilder.Literal(idx));
			}
			builder.Initializer(stateBuilder);
		}
		return builder;
	}
}
