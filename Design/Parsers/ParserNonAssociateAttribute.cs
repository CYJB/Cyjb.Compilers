using System.Diagnostics;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析器的非结合符号。
/// </summary>
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ParserNonAssociateAttribute : Attribute
{

#pragma warning disable IDE0060 // 删除未使用的参数

	/// <summary>
	/// 使用指定的非结合符号初始化 <see cref="ParserNonAssociateAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="kinds">非结合符号。</param>
	public ParserNonAssociateAttribute(params object[] kinds) { }

#pragma warning restore IDE0060 // 删除未使用的参数

}
