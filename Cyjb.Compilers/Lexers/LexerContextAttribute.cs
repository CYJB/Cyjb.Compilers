namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的上下文。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LexerContextAttribute : Attribute
{
	/// <summary>
	/// 使用指定的上下文标签初始化 <see cref="LexerContextAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="label">上下文的标签。</param>
	public LexerContextAttribute(string label)
	{
		Label = label;
	}

	/// <summary>
	/// 获取上下文的标签。
	/// </summary>
	public string Label { get; }
}
