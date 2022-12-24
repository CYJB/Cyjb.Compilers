using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析的终结符定义。
/// </summary>
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class LexerSymbolAttribute : Attribute
{

#pragma warning disable IDE0060 // 删除未使用的参数

	/// <summary>
	/// 使用指定的终结符信息初始化 <see cref="LexerSymbolAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="regex">终结符的正则表达式。</param>
	/// <param name="options">正则表达式的选项。</param>
	public LexerSymbolAttribute(string regex, RegexOptions options = RegexOptions.None) { }

#pragma warning restore IDE0060 // 删除未使用的参数

	/// <summary>
	/// 获取或设置终结符的词法单元类型。
	/// </summary>
	public object? Kind { get; set; }

	/// <summary>
	/// 获取或设置终结符的词法单元值。
	/// </summary>
	public object? Value { get; set; }

	/// <summary>
	/// 获取或设置是否使用终结符的最短匹配。
	/// </summary>
	/// <remarks>默认都会使用正则表达式的最长匹配，允许指定为使用最短匹配，
	/// 会在遇到第一个匹配时立即返回结果。</remarks>
	public bool UseShortest { get; set; }
}
