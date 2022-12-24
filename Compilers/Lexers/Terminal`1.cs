using Cyjb.Compilers.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析中的终结符。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class Terminal<T>
	where T : struct
{
	/// <summary>
	/// 使用终结符的正则表达式初始化 <see cref="Terminal{T}"/> 类的新实例。
	/// </summary>
	/// <param name="index">终结符的索引。</param>
	/// <param name="regex">终结符对应的正则表达式。</param>
	public Terminal(int index, LexRegex regex)
	{
		Index = index;
		RegularExpression = regex;
		Context = new HashSet<LexerContext>();
		Trailing = null;
	}

	/// <summary>
	/// 获取当前终结符的索引。
	/// </summary>
	public int Index { get; }
	/// <summary>
	/// 获取当前终结符对应的正则表达式。
	/// </summary>
	public LexRegex RegularExpression { get; }
	/// <summary>
	/// 获取定义当前终结符的上下文。
	/// </summary>
	/// <value>定义当前终结符的上下文。</value>
	public HashSet<LexerContext> Context { get; }
	/// <summary>
	/// 获取或设置当前终结符的类型。
	/// </summary>
	public T? Kind { get; set; }
	/// <summary>
	/// 获取或设置当前终结符的值。
	/// </summary>
	public object? Value { get; set; }
	/// <summary>
	/// 获取或设置当前终结符的动作。
	/// </summary>
	/// <value>当前终结符的动作。</value>
	public Delegate? Action { get; set; }
	/// <summary>
	/// 获取或设置终结符的向前看信息。
	/// </summary>
	/// <remarks><c>null</c> 表示不是向前看符号，正数表示前面长度固定，
	/// 负数表示后面长度固定，<c>0</c> 表示长度不固定。</remarks>
	public int? Trailing { get; set; }
	/// <summary>
	/// 是否使用当前终结符的最短匹配。
	/// </summary>
	public bool UseShortest { get; set; } = false;

	/// <summary>
	/// 返回词法分析器的终结符数据。
	/// </summary>
	/// <returns>词法分析器的终结符数据。</returns>
	public TerminalData<T> GetData()
	{
		return new TerminalData<T>(Kind, Value, Action, Trailing, UseShortest);
	}
}
