using System.Diagnostics;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析器的左结合符号。
/// </summary>
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ParserLeftAssociateAttribute : Attribute
{

#pragma warning disable IDE0060 // 删除未使用的参数

	/// <summary>
	/// 使用指定的左结合符号初始化 <see cref="ParserLeftAssociateAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="kinds">左结合符号。</param>
	public ParserLeftAssociateAttribute(params object[] kinds) { }

#pragma warning restore IDE0060 // 删除未使用的参数

}
