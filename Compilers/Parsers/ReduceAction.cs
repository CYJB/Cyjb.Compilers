namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 LR 语法分析器的规约动作。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal class ReduceAction<T> : Action
	where T : struct
{
	/// <summary>
	/// 使用指定的产生式初始化 <see cref="ReduceAction{T}"/> 类的新实例。
	/// </summary>
	/// <param name="production">规约时用到的产生式。</param>
	public ReduceAction(Production<T> production)
	{
		Production = production;
	}

	/// <summary>
	/// 规约时用到的产生式。
	/// </summary>
	public Production<T> Production { get; }

	/// <summary>
	/// 获取当前动作是否是成功动作。
	/// </summary>
	public override bool IsSuccessAction => true;

	/// <summary>
	/// 返回当前动作对应的 <see cref="ParserAction"/>。
	/// </summary>
	/// <returns>当前动作对应的 <see cref="ParserAction"/>。</returns>
	public override ParserAction ToParserAction()
	{
		return ParserAction.Reduce(Production.Index);
	}

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Equals(Action? other)
	{
		return other is ReduceAction<T> action && Production == action.Production;
	}

	/// <summary>
	/// 返回当前对象的哈希值。
	/// </summary>
	/// <returns>当前对象的哈希值。</returns>
	public override int GetHashCode()
	{
		return Production.GetHashCode();
	}

	/// <summary>
	/// 返回当前对象的字符串表示。
	/// </summary>
	/// <returns>当前对象的字符串表示。</returns>
	public override string ToString()
	{
		return $"r{Production.Index}";
	}
}
