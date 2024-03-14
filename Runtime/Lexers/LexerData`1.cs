using System.Diagnostics;

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
	/// 词法分析的上下文数据。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IReadOnlyDictionary<string, ContextData> contexts;
	/// <summary>
	/// 词法分析的终结符列表。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly TerminalData<T>[] terminals;
	/// <summary>
	/// 词法分析的字符类映射。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly CharClassMap charClasses;
	/// <summary>
	/// DFA 的状态列表。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly int[] states;
	/// <summary>
	/// DFA 的状态转移。
	/// </summary>
	/// <remarks>使用 <c>trans[i]</c> 表示 <c>check</c>，<c>trans[i+1]</c> 表示 <c>next</c>。</remarks>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly int[] trans;
	/// <summary>
	/// 词法分析的向前看符号的类型。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly TrailingType trailingType;
	/// <summary>
	/// 词法分析中是否包含与行首匹配对应的头节点。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly bool containsBeginningOfLine;
	/// <summary>
	/// 是否用到了 Reject 动作。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly bool rejectable;
	/// <summary>
	/// 词法分析控制器的类型。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Type controllerType;
	/// <summary>
	/// 是否使用了最短匹配。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly bool useShortest;

	/// <summary>
	/// 使用指定的词法分析器数据初始化 <see cref="LexerData{T}"/> 类的新实例。
	/// </summary>
	/// <param name="contexts">上下文数据。</param>
	/// <param name="terminals">终结符列表。</param>
	/// <param name="charClasses">字符类的映射。</param>
	/// <param name="states">DFA 的状态列表。</param>
	/// <param name="trans">DFA 的状态转移。</param>
	/// <param name="trailingType">向前看符号的类型。</param>
	/// <param name="containsBeginningOfLine">是否包含行首匹配的规则。</param>
	/// <param name="rejectable">是否用到了 Reject 动作。</param>
	/// <param name="controllerType">词法分析控制器的类型。</param>
	[CLSCompliant(false)]
	public LexerData(IReadOnlyDictionary<string, ContextData>? contexts, TerminalData<T>[] terminals,
			CharClassMap charClasses, int[] states, int[] trans,
			TrailingType trailingType, bool containsBeginningOfLine, bool rejectable, Type controllerType)
	{
		this.contexts = contexts ?? ContextData.Default;
		this.terminals = terminals;
		this.charClasses = charClasses;
		this.states = states;
		this.trans = trans;
		this.trailingType = trailingType;
		this.containsBeginningOfLine = containsBeginningOfLine;
		this.rejectable = rejectable;
		this.controllerType = controllerType;
		useShortest = terminals.Any(t => t.UseShortest);
	}

	/// <summary>
	/// 获取词法分析的上下文数据。
	/// </summary>
	public IReadOnlyDictionary<string, ContextData> Contexts => contexts;
	/// <summary>
	/// 获取词法分析的终结符列表。
	/// </summary>
	public TerminalData<T>[] Terminals => terminals;
	/// <summary>
	/// 获取词法分析的字符类映射。
	/// </summary>
	[CLSCompliant(false)]
	public CharClassMap CharClasses => charClasses;
	/// <summary>
	/// 获取 DFA 的状态列表。
	/// </summary>
	/// <remarks>假设状态数为 <c>n</c>，<c>n * 4</c> 为状态的基索引，
	/// <c>n * 4 + 1</c> 为默认状态，<c>n * 4 + 2</c> 为符号的长度，
	/// <c>n * 4 + 3</c> 为符号的索引。状态数据之后是符号列表，按符号索引排序，
	/// 使用负数表示向前看的头状态。</remarks>
	public int[] States => states;
	/// <summary>
	/// 获取 DF 的状态转移。
	/// </summary>
	public int[] Trans => trans;
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
	/// 获取是否用到了最短匹配。
	/// </summary>
	public bool UseShortest => useShortest;

	/// <summary>
	/// 返回指定状态使用指定字符转移后的状态。
	/// </summary>
	/// <param name="state">当前状态索引。</param>
	/// <param name="ch">转移的字符。</param>
	/// <returns>转以后的状态，使用 <c>-1</c> 表示没有找到合适的状态。</returns>
	public int NextState(int state, char ch)
	{
		int charClass = charClasses.GetCharClass(ch);
		int offset;
		int len = trans.Length;
		while (state >= 0)
		{
			offset = state * 4;
			int idx = (states[offset] + charClass) * 2;
			if (idx >= 0 && idx < len && trans[idx] == state)
			{
				return trans[idx + 1];
			}
			state = states[offset + DfaStateData.DefaultStateOffset];
		}
		return DfaStateData.InvalidState;
	}

	/// <summary>
	/// 返回指定状态对应的符号。
	/// </summary>
	/// <param name="state">当前状态索引。</param>
	/// <returns><paramref name="state"/> 对应的符号。</returns>
	public ArraySegment<int> GetSymbols(int state)
	{
		int offset = state * 4;
		int count = states[offset + DfaStateData.SymbolsLengthOffset];
		if (count == 0)
		{
			return ArraySegment<int>.Empty;
		}
		else
		{
			return new ArraySegment<int>(states, states[offset + DfaStateData.SymbolIndexOffset], count);
		}
	}
}
