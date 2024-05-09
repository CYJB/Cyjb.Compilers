using Cyjb.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Lexers;

internal sealed partial class LexerController
{
	/// <summary>
	/// 返回表示上下文的键的表达式。
	/// </summary>
	/// <param name="key">上下文的键。</param>
	/// <returns>表示上下文的键的表达式。</returns>
	private static ExpressionBuilder GetContextKey(string key)
	{
		if (key == ContextData.Initial)
		{
			return SyntaxBuilder.Type<ContextData>().AccessMember("Initial");
		}
		else
		{
			return key;
		}
	}
}
