using Cyjb.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Lexers;

internal sealed partial class LexerController
{
	/// <summary>
	/// 返回指定词法分析器数据的状态数据。
	/// </summary>
	/// <param name="data">词法分析器数据</param>
	/// <returns>状态数据。</returns>
	public static ExpressionBuilder StatesValue(LexerData<int> data)
	{
		TypeBuilder stateType = "DfaStateData";
		var builder = SyntaxBuilder.ArrayCreationExpression().InitializerWrap(1);
		foreach (DfaStateData state in data.States)
		{
			var stateBuilder = SyntaxBuilder.ObjectCreationExpression().Type(stateType);
			if (state.BaseIndex == int.MinValue)
			{
				stateBuilder.Argument(SyntaxBuilder.IdentifierName("int.MinValue"));
			}
			else
			{
				stateBuilder.Argument(SyntaxBuilder.LiteralExpression(state.BaseIndex));
			}
			stateBuilder.Argument(SyntaxBuilder.LiteralExpression(state.DefaultState));
			foreach (int idx in state.Symbols)
			{
				stateBuilder.Argument(SyntaxBuilder.LiteralExpression(idx));
			}
			builder.Initializer(stateBuilder);
		}
		return builder;
	}

	/// <summary>
	/// 返回指定词法分析器数据的后继状态数据。
	/// </summary>
	/// <param name="data">词法分析器数据</param>
	/// <returns>后继状态数据。</returns>
	public static ExpressionBuilder NextValue(LexerData<int> data)
	{
		var builder = SyntaxBuilder.ArrayCreationExpression().InitializerWrap(24);
		foreach (int value in data.Next)
		{
			builder.Initializer(SyntaxBuilder.LiteralExpression(value));
		}
		return builder;
	}

	/// <summary>
	/// 返回指定词法分析器数据的状态检查数据。
	/// </summary>
	/// <param name="data">词法分析器数据</param>
	/// <returns>状态检查数据。</returns>
	public static ExpressionBuilder CheckValue(LexerData<int> data)
	{
		var builder = SyntaxBuilder.ArrayCreationExpression().InitializerWrap(24);
		foreach (int value in data.Check)
		{
			builder.Initializer(SyntaxBuilder.LiteralExpression(value));
		}
		return builder;
	}
}
