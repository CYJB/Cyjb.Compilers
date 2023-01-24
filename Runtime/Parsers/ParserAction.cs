namespace Cyjb.Compilers.Parsers;

/// <summary>
/// LR 语法分析器的动作。
/// </summary>
public readonly record struct ParserAction
{
	/// <summary>
	/// 获取表示接受的分析器动作。
	/// </summary>
	public static readonly ParserAction Accept = new(ParserActionType.Accept, 0);
	/// <summary>
	/// 获取表示错误的分析器动作。
	/// </summary>
	public static readonly ParserAction Error = new(ParserActionType.Error, 0);

	/// <summary>
	/// 返回表示归约的分析器动作。
	/// </summary>
	/// <param name="index">归约使用的产生式编号。</param>
	/// <returns>表示归约的分析器动作。</returns>
	public static ParserAction Reduce(int index) => new(ParserActionType.Reduce, index);
	/// <summary>
	/// 返回表示移入的分析器动作。
	/// </summary>
	/// <param name="index">移入后要压栈的状态编号。</param>
	/// <returns>表示移入的分析器动作。</returns>
	public static ParserAction Shift(int index) => new(ParserActionType.Shift, index);

	/// <summary>
	/// 使用指定的动作类型和索引初始化。
	/// </summary>
	/// <param name="type">动作类型。</param>
	/// <param name="index">动作关联到的索引。</param>
	private ParserAction(ParserActionType type, int index)
	{
		Type = type;
		Index = index;
	}

	/// <summary>
	/// 获取动作类型。
	/// </summary>
	public ParserActionType Type { get; init; }
	/// <summary>
	/// 获取动作关联到的索引。
	/// </summary>
	public int Index { get; init; }

	/// <summary>
	/// 返回当前对象的字符串表示。
	/// </summary>
	/// <returns>当前对象的字符串表示。</returns>
	public override string ToString()
	{
		return Type switch
		{
			ParserActionType.Accept => "acc",
			ParserActionType.Shift => $"s{Index}",
			ParserActionType.Reduce => $"r{Index}",
			_ => "err",
		};
	}
}
