using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示支持 Reject 动作的词法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class RejectableReader<T> : TokenReaderBase<T>
	where T : struct
{
	/// <summary>
	/// 接受状态的堆栈。
	/// </summary>
	private readonly Stack<AcceptState> stateStack = new();

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="RejectableReader&lt;T&gt;"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="env">词法分析器的环境信息。</param>
	/// <param name="reader">要使用的源文件读取器。</param>
	public RejectableReader(LexerData<T> lexerData, object? env, SourceReader reader) :
		base(lexerData, env, true, reader)
	{ }

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <param name="state">DFA 的起始状态。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected override bool NextToken(int state)
	{
		stateStack.Clear();
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
				stateStack.Push(new AcceptState(symbols, Source.Index));
			}
		}
		// 遍历终结状态，执行相应动作。
		while (stateStack.Count > 0)
		{
			AcceptState candidate = stateStack.Pop();
			int index = candidate.Index;
			foreach (int acceptState in candidate.Symbols)
			{
				// 将文本和流调整到与接受状态匹配的状态。
				Source.Index = index;
				TerminalData<T> terminal = Data.Terminals[acceptState];
				Controller.DoAction(Start, terminal.Kind, terminal.Action);
				if (!Controller.IsReject)
				{
					return true;
				}
			}
		}
		return false;
	}
}
