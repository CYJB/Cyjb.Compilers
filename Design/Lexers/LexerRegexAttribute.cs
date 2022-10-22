using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析的正则表达式定义。
/// </summary>
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LexerRegexAttribute : Attribute
{

#pragma warning disable IDE0060 // 删除未使用的参数

	/// <summary>
	/// 使用指定的正则表达式信息初始化 <see cref="LexerRegexAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="name">正则表达式的名称。</param>
	/// <param name="regex">定义的正则表达式。</param>
	/// <param name="options">正则表达式的选项。</param>
	public LexerRegexAttribute(string name, string regex, RegexOptions options = RegexOptions.None) { }

#pragma warning restore IDE0060 // 删除未使用的参数

}
