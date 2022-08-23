using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的工厂。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public interface ILexerFactory<T>
	where T : struct
{
	/// <summary>
	/// 创建分析指定源文件的词法分析器。
	/// </summary>
	/// <param name="source">要读取的源文件。</param>
	/// <returns>指定源文件的词法分析器。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	/// <overloads>
	/// <summary>
	/// 创建分析指定源文件的词法分析器。
	/// </summary>
	/// </overloads>
	Tokenlizer<T> CreateTokenlizer(string source);

	/// <summary>
	/// 创建分析指定源文件的词法分析器。
	/// </summary>
	/// <param name="source">要读取的源文件。</param>
	/// <returns>指定源文件的词法分析器。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
	Tokenlizer<T> CreateTokenlizer(SourceReader source);
}
