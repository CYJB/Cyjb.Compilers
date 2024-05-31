namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的工厂。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public interface ILexerFactory<T>
	where T : struct
{
	/// <summary>
	/// 创建词法分析器。
	/// </summary>
	/// <param name="debug">是否需要打印调试信息。</param>
	/// <returns>已创建的词法分析器。</returns>
	LexerTokenizer<T> CreateTokenizer(bool debug = false);

	/// <summary>
	/// 创建词法分析运行器。
	/// </summary>
	/// <param name="debug">是否需要打印调试信息。</param>
	/// <returns>已创建的词法分析运行器。</returns>
	LexerRunner<T> CreateRunner(bool debug = false);
}
