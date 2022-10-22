using System.Diagnostics;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的上下文。
/// </summary>
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LexerContextAttribute : Attribute
{

#pragma warning disable IDE0060 // 删除未使用的参数

	/// <summary>
	/// 使用指定的上下文标签初始化 <see cref="LexerContextAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="label">上下文的标签。</param>
	public LexerContextAttribute(string label) { }

#pragma warning restore IDE0060 // 删除未使用的参数

}
