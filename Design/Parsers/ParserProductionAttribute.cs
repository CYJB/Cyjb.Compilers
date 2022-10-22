using System.Diagnostics;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析器的产生式。
/// </summary>
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ParserProductionAttribute : Attribute
{

#pragma warning disable IDE0060 // 删除未使用的参数

	/// <summary>
	/// 使用指定的产生式所属的非终结符和内容初始化 <see cref="ParserProductionAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="kind">产生式所属的非终结符。</param>
	/// <param name="body">产生式的内容。</param>
	public ParserProductionAttribute(object kind, params object[] body) { }

#pragma warning restore IDE0060 // 删除未使用的参数

	/// <summary>
	/// 获取或设置设置当前产生式的优先级与指定的非终结符相同。
	/// </summary>
	public object? Prec { get; set; }

}
