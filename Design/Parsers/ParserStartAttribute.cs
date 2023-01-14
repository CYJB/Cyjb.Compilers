using System.Diagnostics;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析器的起始符号。
/// </summary>
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ParserStartAttribute : Attribute
{

#pragma warning disable IDE0060 // 删除未使用的参数

	/// <summary>
	/// 使用指定的起始符号初始化 <see cref="ParserProductionAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="kind">语法分析器的起始符号。</param>
	public ParserStartAttribute(object kind) { }

	/// <summary>
	/// 使用指定的起始符号和分析选项初始化 <see cref="ParserProductionAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="kind">语法分析器的起始符号。</param>
	/// <param name="option">语法分析器的分析选项。</param>
	public ParserStartAttribute(object kind, ParseOptions option) { }

#pragma warning restore IDE0060 // 删除未使用的参数

}
