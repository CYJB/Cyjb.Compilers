using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示支持全部功能的词法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class TokenizerRejectableTrailing<T> : TokenizerBase<T>
	where T : struct
{
	/// <summary>
	/// 接受状态的堆栈。
	/// </summary>
	private readonly List<AcceptState> states = new();

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="TokenizerRejectableTrailing{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	/// <param name="reader">要使用的源文件读取器。</param>
	public TokenizerRejectableTrailing(LexerData<T> lexerData, LexerController<T> controller, SourceReader reader) :
		base(lexerData, controller, reader)
	{ }

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <param name="state">DFA 的起始状态。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected override bool NextToken(int state)
	{
		states.Clear();
		int startIndex = source.Index;
		while (true)
		{
			state = NextState(state);
			if (state == -1)
			{
				// 没有合适的转移，退出。
				break;
			}
			int[] symbols = Data.States[state].Symbols;
			if (symbols.Length > 0)
			{
				// 将接受状态记录在堆栈中。
				states.Add(new AcceptState(symbols, source.Index));
			}
		}
		// 遍历终结状态，执行相应动作。
		for (int i = states.Count - 1; i >= 0; i--)
		{
			AcceptState candidate = states[i];
			foreach (int acceptState in candidate.Symbols)
			{
				if (acceptState < 0)
				{
					// 跳过向前看的头状态。
					break;
				}
				int lastIndex = candidate.Index;
				TerminalData<T> terminal = Data.Terminals[acceptState];
				int? trailing = terminal.Trailing;
				if (trailing.HasValue)
				{
					// 是向前看状态。
					int index = trailing.Value;
					if (index > 0)
					{
						// 前面长度固定。
						lastIndex = startIndex + index;
					}
					else if (index < 0)
					{
						// 后面长度固定，注意此时 index 是负数。
						lastIndex += index;
					}
					else
					{
						// 前后长度都不固定，需要沿着堆栈向前找。
						int target = -acceptState;
						for (int j = i - 1; j >= 0; j--)
						{
							if (ContainsTrailingHead(states[j].Symbols, target))
							{
								lastIndex = states[j].Index;
								break;
							}
						}
					}
				}
				// 将文本和流调整到与接受状态匹配的状态。
				source.Index = lastIndex;
				Controller.DoAction(Start, terminal.Kind, terminal.Action);
				if (!Controller.IsReject)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 返回指定的接受状态的符号索引中是否包含特定的向前看头状态。
	/// </summary>
	/// <param name="symbols">接受状态的符号索引。</param>
	/// <param name="target">目标向前看头状态。</param>
	/// <returns>如果包含特定的目标，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	private static bool ContainsTrailingHead(int[] symbols, int target)
	{
		// 在当前状态中查找，从后向前找。
		for (int i = symbols.Length - 1; i >= 0; i--)
		{
			int idx = symbols[i];
			if (idx >= 0)
			{
				// 前面的状态已经不可能是向前看头状态了，所以直接退出。
				break;
			}
			if (idx == target)
			{
				return true;
			}
		}
		return false;
	}
}
