using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的数据。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <remarks><para><see cref="LexerData{T}"/> 类包含了用于构造词法分析器的全部信息，
/// 可以用于构造词法分析器。也可以使用默认的词法分析器工厂 <see cref="LexerFactory{T}"/>。</para>
/// <para>关于如何构造自己的词法分析器，可以参考我的博文
/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html">
/// 《C# 词法分析器（六）构造词法分析器》</see>。</para></remarks>
/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html">
/// 《C# 词法分析器（六）构造词法分析器》</seealso>
public class LexerData<T>
	where T : struct
{
	/// <summary>
	/// 默认的词法分析上下文。
	/// </summary>
	private readonly static Dictionary<string, ContextData<T>> defaultContexts = new()
	{
		 { ContextData.Initial, new ContextData<T>(0, ContextData.Initial) }
	};

	/// <summary>
	/// 词法分析的上下文数据。
	/// </summary>
	private readonly IReadOnlyDictionary<string, ContextData<T>> contexts;
	/// <summary>
	/// 词法分析的终结符列表。
	/// </summary>
	private readonly TerminalData<T>[] terminals;
	/// <summary>
	/// 词法分析的字符类映射。
	/// </summary>
	private readonly CharClassMap charClasses;
	/// <summary>
	/// DFA 的状态列表。
	/// </summary>
	private readonly DfaStateData[] states;
	/// <summary>
	/// 下一状态列表。
	/// </summary>
	private readonly int[] next;
	/// <summary>
	/// 状态检查。
	/// </summary>
	private readonly int[] check;
	/// <summary>
	/// 词法分析的向前看符号的类型。
	/// </summary>
	private readonly TrailingType trailingType;
	/// <summary>
	/// 词法分析中是否包含与行首匹配对应的头节点。
	/// </summary>
	private readonly bool containsBeginningOfLine;
	/// <summary>
	/// 是否用到了 Reject 动作。
	/// </summary>
	private readonly bool rejectable;
	/// <summary>
	/// 词法分析控制器的类型。
	/// </summary>
	private readonly Type controllerType;

	/// <summary>
	/// 使用指定的词法分析器数据初始化 <see cref="LexerData{T}"/> 类的新实例。
	/// </summary>
	/// <param name="contexts">上下文数据。</param>
	/// <param name="terminals">终结符列表。</param>
	/// <param name="charClasses">字符类的映射。</param>
	/// <param name="states">DFA 的状态列表。</param>
	/// <param name="next">下一状态列表。</param>
	/// <param name="check">状态检查。</param>
	/// <param name="trailingType">向前看符号的类型。</param>
	/// <param name="containsBeginningOfLine">是否包含行首匹配的规则。</param>
	/// <param name="rejectable">是否用到了 Reject 动作。</param>
	/// <param name="controllerType">词法分析控制器的类型。</param>
	public LexerData(Dictionary<string, ContextData<T>>? contexts, TerminalData<T>[] terminals,
		CharClassMap charClasses, DfaStateData[] states, int[] next, int[] check,
		TrailingType trailingType, bool containsBeginningOfLine, bool rejectable, Type controllerType)
	{
		this.contexts = contexts ?? defaultContexts;
		this.terminals = terminals;
		this.charClasses = charClasses;
		this.states = states;
		this.next = next;
		this.check = check;
		this.trailingType = trailingType;
		this.containsBeginningOfLine = containsBeginningOfLine;
		this.rejectable = rejectable;
		this.controllerType = controllerType;
	}

	/// <summary>
	/// 获取词法分析的上下文数据。
	/// </summary>
	public IReadOnlyDictionary<string, ContextData<T>> Contexts => contexts;
	/// <summary>
	/// 获取词法分析的终结符列表。
	/// </summary>
	public TerminalData<T>[] Terminals => terminals;
	/// <summary>
	/// 获取词法分析的字符类映射。
	/// </summary>
	public CharClassMap CharClasses => charClasses;
	/// <summary>
	/// 获取 DFA 的状态列表。
	/// </summary>
	public DfaStateData[] States => states;
	/// <summary>
	/// 获取下一状态列表。
	/// </summary>
	public int[] Next => next;
	/// <summary>
	/// 获取状态检查。
	/// </summary>
	public int[] Check => check;
	/// <summary>
	/// 获取词法分析的向前看符号的类型。
	/// </summary>
	public TrailingType TrailingType => trailingType;
	/// <summary>
	/// 获取词法分析中是否包含与行首匹配对应的头节点。
	/// </summary>
	/// <value>如果包含与行首匹配对应的头节点，则为 <c>true</c>，包含上下文个数×2 个头节点；
	/// 否则为 <c>false</c>，包含上下文个数个头节点。</value>
	public bool ContainsBeginningOfLine => containsBeginningOfLine;
	/// <summary>
	/// 获取是否用到了 Reject 动作。
	/// </summary>
	public bool Rejectable => rejectable;
	/// <summary>
	/// 获取词法分析控制器的类型。
	/// </summary>
	public Type ControllerType => controllerType;

	/// <summary>
	/// 返回指定状态使用指定字符转移后的状态。
	/// </summary>
	/// <param name="state">当前状态索引。</param>
	/// <param name="ch">转移的字符。</param>
	/// <returns>转以后的状态，使用 <c>-1</c> 表示没有找到合适的状态。</returns>
	public int NextState(int state, char ch)
	{
		if (ch == SourceReader.InvalidCharacter)
		{
			return DfaStateData.InvalidState;
		}
		int charClass = charClasses.GetCharClass(ch);
		DfaStateData stateData;
		int len = check.Length;
		while (state >= 0)
		{
			stateData = states[state];
			int idx = stateData.BaseIndex + charClass;
			if (idx < 0 || idx >= len)
			{
				return DfaStateData.InvalidState;
			}
			if (check[idx] == state)
			{
				return next[idx];
			}
			else
			{
				state = stateData.DefaultState;
			}
		}
		return DfaStateData.InvalidState;
	}
}
