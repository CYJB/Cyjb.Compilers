namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析中上下文的数据。
/// </summary>
public class ContextData
{
	/// <summary>
	/// 初始的词法分析器上下文名称。
	/// </summary>
	public const string Initial = "Initial";

	/// <summary>
	/// 默认的词法分析上下文。
	/// </summary>
	public readonly static IReadOnlyDictionary<string, ContextData> Default =
		new Dictionary<string, ContextData>()
		{
			 { Initial, new ContextData(0, Initial) }
		};

	/// <summary>
	/// 使用指定的上下文数据初始化 <see cref="ContextData"/> 类的新实例。
	/// </summary>
	/// <param name="index">上下文的索引。</param>
	/// <param name="label">上下文的标签。</param>
	/// <param name="eofAction">EOF 动作。</param>
	/// <param name="eofValue">EOF 动作的值。</param>
	public ContextData(int index, string label, Delegate? eofAction = null, object? eofValue = null)
	{
		Index = index;
		Label = label;
		EofAction = eofAction;
		EofValue = eofValue;
	}

	/// <summary>
	/// 上下文的索引。
	/// </summary>
	public int Index { get; }
	/// <summary>
	/// 上下文的标签。
	/// </summary>
	public string Label { get; }
	/// <summary>
	/// 上下文的 EOF 动作。
	/// </summary>
	public Delegate? EofAction { get; }
	/// <summary>
	/// 上下文的 EOF 动作的值。
	/// </summary>
	public object? EofValue { get; }
}
