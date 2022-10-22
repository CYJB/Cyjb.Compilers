namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 LR 语法分析器的接受动作。
/// </summary>
internal class AcceptAction : Action
{
	/// <summary>
	/// 高优先级接受动作的实例。
	/// </summary>
	public static readonly AcceptAction HighPriority = new(true);
	/// <summary>
	/// 低优先级接受动作的实例。
	/// </summary>
	public static readonly AcceptAction LowPriority = new(false);

	private AcceptAction(bool isHighPriority)
	{
		IsHighPriority = isHighPriority;
	}

	/// <summary>
	/// 获取当前动作是否是成功动作。
	/// </summary>
	public override bool IsSuccessAction => true;
	/// <summary>
	/// 获取当前动作是否是高优先级的。
	/// </summary>
	public bool IsHighPriority { get; }

	/// <summary>
	/// 返回当前动作对应的 <see cref="ParserAction"/>。
	/// </summary>
	/// <returns>当前动作对应的 <see cref="ParserAction"/>。</returns>
	public override ParserAction ToParserAction()
	{
		return ParserAction.Accept;
	}

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Equals(Action? other)
	{
		return other is AcceptAction;
	}

	/// <summary>
	/// 返回当前对象的哈希值。
	/// </summary>
	/// <returns>当前对象的哈希值。</returns>
	public override int GetHashCode()
	{
		return 38189501;
	}

	/// <summary>
	/// 返回当前对象的字符串表示。
	/// </summary>
	/// <returns>当前对象的字符串表示。</returns>
	public override string ToString()
	{
		return "acc";
	}
}
