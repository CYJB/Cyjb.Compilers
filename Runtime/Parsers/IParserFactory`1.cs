namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析器的工厂。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public interface IParserFactory<T>
	where T : struct
{
	/// <summary>
	/// 创建语法分析器。
	/// </summary>
	/// <returns>已创建的语法分析器。</returns>
	LRParser<T> CreateParser();
}
