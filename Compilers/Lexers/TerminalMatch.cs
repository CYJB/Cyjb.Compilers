using Cyjb.Compilers.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析中的终结符的匹配信息。
/// </summary>
internal sealed class TerminalMatch
{
	/// <summary>
	/// 使用终结符的正则表达式初始化 <see cref="TerminalMatch"/> 类的新实例。
	/// </summary>
	/// <param name="regex">终结符对应的正则表达式。</param>
	public TerminalMatch(LexRegex regex)
	{
		RegularExpression = regex;
		Context = new HashSet<LexerContext>();
	}

	/// <summary>
	/// 获取当前终结符对应的正则表达式。
	/// </summary>
	public LexRegex RegularExpression { get; }
	/// <summary>
	/// 获取定义当前终结符的上下文。
	/// </summary>
	/// <value>定义当前终结符的上下文。</value>
	public HashSet<LexerContext> Context { get; }
}
