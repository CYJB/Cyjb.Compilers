namespace Cyjb.Compilers.Parsers.Production;

/// <summary>
/// 产生式节点。
/// </summary>
internal class ProductionNode
{
	/// <summary>
	/// 使用指定的符号初始化 <see cref="ProductionNode"/> 类的新实例。
	/// </summary>
	/// <param name="symbol">符号。</param>
	public ProductionNode(object symbol)
	{
		Symbols.Add(symbol);
	}

	/// <summary>
	/// 当前节点包含的符号列表。
	/// </summary>
	public List<object> Symbols { get; } = new();
	/// <summary>
	/// 获取当前节点的重复类型。
	/// </summary>
	public ProductionKind RepeatKind { get; set; } = ProductionKind.Id;
}
