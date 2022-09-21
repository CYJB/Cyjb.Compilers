namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析中上下文的数据。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class ContextData<T>
	where T : struct
{
	/// <summary>
	/// 使用指定的上下文数据初始化 <see cref="ContextData{T}"/> 类的新实例。
	/// </summary>
	/// <param name="index">上下文的索引。</param>
	/// <param name="label">上下文的标签。</param>
	/// <param name="eofAction">EOF 动作。</param>
	public ContextData(int index, string label, Delegate? eofAction = null)
	{
		Index = index;
		Label = label;
		EofAction = eofAction;
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
}
