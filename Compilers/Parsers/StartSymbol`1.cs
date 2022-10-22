namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示起始非终结符。
/// </summary>
/// <typeparam name="T"></typeparam>
internal class StartSymbol<T>
	where T : struct
{
	/// <summary>
	/// 使用指定的非终结符类型和增广起始符号初始化 <see cref="StartSymbol{T}"/> 类的新实例。
	/// </summary>
	/// <param name="kind">非终结符的类型。</param>
	/// <param name="augmentedStartSymbol">增广起始符号。</param>
	/// <param name="option">分析选项。</param>
	public StartSymbol(T kind, Symbol<T> augmentedStartSymbol, ParseOption option)
	{
		Kind = kind;
		AugmentedStartSymbol = augmentedStartSymbol;
		Option = option;
	}

	/// <summary>
	/// 获取非终结符的类型。
	/// </summary>
	public T Kind { get; }
	/// <summary>
	/// 获取增广起始符号。
	/// </summary>
	public Symbol<T> AugmentedStartSymbol { get; }
	/// <summary>
	/// 获取分析选项。
	/// </summary>
	public ParseOption Option { get; }
}
