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
				// 未用到 Reject 动作的时候，只需要保留第一个终结符。
				if (!data.Rejectable)
				{
					break;
				}
			}
			builder.Initializer(stateBuilder);
		}
		return builder;
	}
}
