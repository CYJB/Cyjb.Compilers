using System.Text.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析的正则表达式定义。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LexerRegexAttribute : Attribute
{
	/// <summary>
	/// 使用指定的正则表达式信息初始化 <see cref="LexerRegexAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="name">正则表达式的名称。</param>
	/// <param name="regex">定义的正则表达式。</param>
	/// <param name="options">正则表达式的选项。</param>
	public LexerRegexAttribute(string name, string regex, RegexOptions options = RegexOptions.None)
	{
		Name = name;
		Regex = regex;
		Options = options;
	}

	/// <summary>
	/// 获取正则表达式的名称。
	/// </summary>
	public string Name { get; }
	/// <summary>
	/// 获取正则表达式的内容。
	/// </summary>
	public string Regex { get; }
	/// <summary>
	/// 获取正则表达式的选项。
	/// </summary>
	public RegexOptions Options { get; }
}
