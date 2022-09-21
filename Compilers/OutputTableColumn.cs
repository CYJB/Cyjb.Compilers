namespace Cyjb.Compilers;

/// <summary>
/// 文本输出的列定义。
/// </summary>
internal class OutputTableColumn
{
	/// <summary>
	/// 使用列的名称初始化 <see cref="OutputTableColumn"/> 类的新实例。
	/// </summary>
	/// <param name="name">列的名称。</param>
	/// <param name="isFixedWidth">列是否是固定宽度。</param>
	public OutputTableColumn(string name, bool isFixedWidth = true)
	{
		Name = name;
		IsFixedWidth = isFixedWidth;
	}

	/// <summary>
	/// 获取列的名称。
	/// </summary>
	public string Name { get; }
	/// <summary>
	/// 获取列是否是固定宽度。
	/// </summary>
	public bool IsFixedWidth { get;private set; }
}
