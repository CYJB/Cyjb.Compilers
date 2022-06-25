using System.Text.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析的终结符定义。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class LexerSymbolAttribute : Attribute
{
	/// <summary>
	/// 使用指定的终结符信息初始化 <see cref="LexerSymbolAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="regex">终结符的正则表达式。</param>
	/// <param name="options">正则表达式的选项。</param>
	public LexerSymbolAttribute(string regex, RegexOptions options = RegexOptions.None)
	{
		Regex = regex;
		Options = options;
	}

	/// <summary>
	/// 获取终结符的正则表达式。
	/// </summary>
	public string Regex { get; }
	/// <summary>
	/// 获取正则表达式的选项。
	/// </summary>
	public RegexOptions Options { get; }
	/// <summary>
	/// 获取或设置终结符的词法单元类型。
	/// </summary>
	public object? Kind { get; set; }
	/// <summary>
	/// 获取或设置正则表达式的优先级，默认为 <c>0</c>。
	/// </summary>
	public int Priority { get; set; }
}
