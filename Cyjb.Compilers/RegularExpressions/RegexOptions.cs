using System;

namespace Cyjb.Compilers.RegularExpressions
{
	/// <summary>
	/// 提供用于设置正则表达式选项的枚举值。
	/// </summary>
	[Flags]
	public enum RegexOptions
	{
		/// <summary>
		/// 指定不设置选项。
		/// </summary>
		None = 0,
		/// <summary>
		/// 指定不区分大小写的匹配。
		/// </summary>
		IgnoreCase = 1,
		/// <summary>
		/// 指定单行模式。更改点 (.) 的含义，使它与每一个字符匹配（而不是与除 \n 之外的每个字符匹配）。
		/// </summary>
		Singleline = 2,
		/// <summary>
		/// 消除模式中的非转义空白并启用由 # 标记的注释。
		/// 但是，<see cref="RegexOptions.IgnorePatternWhiteSpace"/> 值不会影响或消除字符类中的空白。 
		/// </summary>
		IgnorePatternWhiteSpace = 4,
		/// <summary>
		/// 为表达式启用符合 ECMAScript 的行为。该值只能与 IgnoreCase 值一起使用。
		/// 该值与其他任何值一起使用均将导致异常。
		/// </summary>
		EcmaScript = 8,
		/// <summary>
		/// 指定忽略语言中的区域性差异。
		/// </summary>
		CultureInvariant = 0x10,
		/// <summary>
		/// 遇到非转义空白则结束模式。
		/// 但是，<see cref="RegexOptions.EndPatternByWhiteSpace"/> 值不会影响字符类和括号中的空白。 
		/// </summary>
		EndPatternByWhiteSpace = 0x20
	}
}
