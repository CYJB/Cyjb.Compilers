namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的包含型上下文。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LexerInclusiveContextAttribute : Attribute
{
	/// <summary>
	/// 使用指定的上下文标签初始化 <see cref="LexerInclusiveContextAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="label">上下文的标签。</param>
	public LexerInclusiveContextAttribute(string label)
	{
		Label = label;
	}

	/// <summary>
	/// 获取上下文的标签。
	/// </summary>
	public string Label { get; }
}
