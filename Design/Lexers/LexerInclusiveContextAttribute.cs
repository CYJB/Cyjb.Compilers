using System.Diagnostics;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的包含型上下文。
/// </summary>
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LexerInclusiveContextAttribute : Attribute
{

#pragma warning disable IDE0060 // 删除未使用的参数

	/// <summary>
	/// 使用指定的上下文标签初始化 <see cref="LexerInclusiveContextAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="label">上下文的标签。</param>
	public LexerInclusiveContextAttribute(string label) { }

#pragma warning restore IDE0060 // 删除未使用的参数

}
