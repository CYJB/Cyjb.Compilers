namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析中的符号。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class Symbol<T>
	where T : struct
{
	/// <summary>
	/// 文件结束标记符。
	/// </summary>
	public static readonly Symbol<T> EndOfFile = new(GenericConvert.ChangeType<int, T>(-1), "EOF")
	{
		Type = SymbolType.EndOfFile
	};
	/// <summary>
	/// 错误标记符。
	/// </summary>
	public static readonly Symbol<T> Error = new(GenericConvert.ChangeType<int, T>(-2), "ERROR")
	{
		Type = SymbolType.Error
	};
	/// <summary>
	/// 空串标记符。
	/// </summary>
	public static readonly Symbol<T> Epsilon = new(GenericConvert.ChangeType<int, T>(-3), "ε")
	{
		Type = SymbolType.Epsilon
	};
	/// <summary>
	/// 传递标记符。
	/// </summary>
	public static readonly Symbol<T> Spread = new(GenericConvert.ChangeType<int, T>(-4), "#")
	{
		Type = SymbolType.Spread
	};

	/// <summary>
	/// 使用指定的符号索引初始化 <see cref="Symbol{T}"/> 类的新实例。
	/// </summary>
	/// <param name="kind">符号的类型。</param>
	/// <param name="name">当前符号的名称。</param>
	public Symbol(T kind, string name)
	{
		Index = -1;
		Kind = kind;
		Name = name;
	}

	/// <summary>
	/// 获取或设置当前符号的索引。
	/// </summary>
	/// <value>当前符号的索引。</value>
	public int Index { get; set; }
	/// <summary>
	/// 获取当前符号的类型。
	/// </summary>
	/// <value>当前符号的类型。</value>
	public T Kind { get; }
	/// <summary>
	/// 获取当前符号的名称。
	/// </summary>
	/// <value>当前符号的名称索引。</value>
	public string Name { get; }
	/// <summary>
	/// 获取或设置当前符号的类型。
	/// </summary>
	public SymbolType Type { get; set; }
	/// <summary>
	/// 获取或设置起始符号类型。
	/// </summary>
	public SymbolStartType StartType { get; set; } = SymbolStartType.NotStart;
	/// <summary>
	/// 获取当前非终结符的所有产生式。
	/// </summary>
	/// <value>当前非终结符的所有产生式。</value>
	public List<Production<T>> Productions { get; } = new();

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		return Name;
	}
}
