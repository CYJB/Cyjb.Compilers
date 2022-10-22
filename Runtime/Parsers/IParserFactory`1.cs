using Cyjb.Text;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析器的工厂。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public interface IParserFactory<T>
	where T : struct
{
	/// <summary>
	/// 创建分析指定的词法单元序列语法分析器。
	/// </summary>
	/// <param name="tokens">要分析的词法单元序列。</param>
	/// <returns>指定词法单元序列的语法分析器。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="tokens"/> 为 <c>null</c>。</exception>
	ITokenParser<T> CreateParser(ITokenizer<T> tokens);
}
