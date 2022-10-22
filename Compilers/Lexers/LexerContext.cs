namespace Cyjb.Compilers.Lexers;

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

/// <summary>
/// 表示词法分析器的上下文。
/// </summary>
internal sealed class LexerContext
{
	/// <summary>
	/// 使用指定的上下文信息初始化 <see cref="LexerContext"/> 的新实例。
	/// </summary>
	/// <param name="index">上下文的索引。</param>
	/// <param name="label">上下文的标签。</param>
	/// <param name="type">上下文的类型。</param>
	public LexerContext(int index, string label, LexerContextType type)
	{
		Index = index;
		Label = label;
		Type = type;
	}

	/// <summary>
	/// 获取上下文的索引。
	/// </summary>
	public int Index { get; }
	/// <summary>
	/// 获取上下文的标签。
	/// </summary>
	public string Label { get; }
	/// <summary>
	/// 获取上下文的类型。
	/// </summary>
	public LexerContextType Type { get; }
	/// <summary>
	/// 上下文的 EOF 动作。
	/// </summary>
	public Delegate? EofAction { get; set; }
	/// <summary>
	/// 上下文的 EOF 动作的值。
	/// </summary>
	public object? EofValue { get; set; }

	/// <summary>
	/// 返回词法分析器的上下文数据。
	/// </summary>
	/// <returns>词法分析器的上下文数据。</returns>
	public ContextData GetData()
	{
		return new ContextData(Index, Label, EofAction, EofValue);
	}
}
