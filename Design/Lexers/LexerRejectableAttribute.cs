using System.Diagnostics;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析是否用到了 Reject 动作。
/// </summary>
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class)]
public sealed class LexerRejectableAttribute : Attribute
{
	/// <summary>
	/// 初始化 <see cref="LexerRejectableAttribute"/> 类的新实例。
	/// </summary>
	public LexerRejectableAttribute() { }
}
