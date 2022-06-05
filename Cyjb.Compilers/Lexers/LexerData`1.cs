namespace Cyjb.Compilers.Lexers
{
	/// <summary>
	/// 表示词法分析器的数据。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
	/// <remarks><para><see cref="LexerData{T}"/> 类包含了用于构造词法分析器的全部信息，
	/// 可以用于自己构造词法分析器。或者可以使用 <see cref="LexerData{T}.GetFactory"/>
	/// 获取默认实现。</para>
	/// <para>关于如何构造自己的词法分析器，可以参考我的博文
	/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html">
	/// 《C# 词法分析器（六）构造词法分析器》</see>。</para></remarks>
	/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html">
	/// 《C# 词法分析器（六）构造词法分析器》</seealso>
	[Serializable]
	public sealed class LexerData<T>
		where T : struct
	{
		/// <summary>
		/// 使用指定的词法分析器数据初始化 <see cref="LexerData{T}"/> 类的新实例。
		/// </summary>
		/// <param name="contexts">上下文数据。</param>
		/// <param name="terminals">终结符列表。</param>
		/// <param name="charClasses">字符类的映射。</param>
		/// <param name="dfa">DFA 的数据。</param>
		/// <param name="trailingType">向前看符号的类型。</param>
		/// <param name="containsBeginningOfLine">是否包含行首匹配的规则。</param>
		public LexerData(Dictionary<string, ContextData<T>> contexts, TerminalData<T>[] terminals,
			CharClassMap charClasses, DfaData dfa, TrailingType trailingType, bool containsBeginningOfLine)
		{
			Contexts = contexts;
			Terminals = terminals;
			CharClasses = charClasses;
			States = dfa.States;
			Next = dfa.Next;
			Check = dfa.Check;
			TrailingType = trailingType;
			ContainsBeginningOfLine = containsBeginningOfLine;
		}

		/// <summary>
		/// 获取词法分析的上下文数据。
		/// </summary>
		public IReadOnlyDictionary<string, ContextData<T>> Contexts { get; }
		/// <summary>
		/// 获取词法分析的终结符列表。
		/// </summary>
		public TerminalData<T>[] Terminals { get; }
		/// <summary>
		/// 获取词法分析的字符类映射。
		/// </summary>
		public CharClassMap CharClasses { get; }
		/// <summary>
		/// 获取 DFA 的状态列表。
		/// </summary>
		public DfaStateData[] States { get; }
		/// <summary>
		/// 获取下一状态列表。
		/// </summary>
		public int[] Next { get; }
		/// <summary>
		/// 获取状态检查。
		/// </summary>
		public int[] Check { get; }
		/// <summary>
		/// 获取词法分析的向前看符号的类型。
		/// </summary>
		public TrailingType TrailingType { get; }
		/// <summary>
		/// 获取词法分析中是否包含与行首匹配对应的头节点。
		/// </summary>
		/// <value>如果包含与行首匹配对应的头节点，则为 <c>true</c>，包含上下文个数×2 个头节点；
		/// 否则为 <c>false</c>，包含上下文个数个头节点。</value>
		public bool ContainsBeginningOfLine { get; }

		/// <summary>
		/// 返回使用当前数据的词法分析器工厂。
		/// </summary>
		/// <param name="rejectable">是否允许 Reject 动作。</param>
		/// <returns>词法分析器工厂。</returns>
		/// <overloads>
		/// <summary>
		/// 返回使用当前数据的词法分析器工厂。
		/// </summary>
		/// </overloads>
		public TokenReaderFactory<T> GetFactory(bool rejectable = false)
		{
			return new TokenReaderFactory<T>(this, null, TrailingType, rejectable);
		}

		/// <summary>
		/// 返回使用当前数据的词法分析器工厂。
		/// </summary>
		/// <param name="envInitializer">词法分析器的环境初始化器。</param>
		/// <param name="rejectable">是否允许 Reject 动作。</param>
		/// <returns>词法分析器工厂。</returns>
		public TokenReaderFactory<T> GetFactory(Func<object?>? envInitializer, bool rejectable = false)
		{
			return new TokenReaderFactory<T>(this, envInitializer, TrailingType, rejectable);
		}
	}
}
