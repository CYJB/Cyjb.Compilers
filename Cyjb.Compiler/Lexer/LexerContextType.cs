namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 词法分析器上下文的类型。
	/// </summary>
	internal enum LexerContextType
	{
		/// <summary>
		/// 包含型上下文，默认上下文的规则也会有效。
		/// </summary>
		Inclusive,
		/// <summary>
		/// 排除型上下文，默认上下文的规则无效。
		/// </summary>
		Exclusive
	}
}
