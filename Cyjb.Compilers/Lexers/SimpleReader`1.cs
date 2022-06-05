using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示基本的词法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class SimpleReader<T> : TokenReaderBase<T>
	where T : struct
{
	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="SimpleReader{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="env">词法分析器的环境信息。</param>
	/// <param name="reader">要使用的源文件读取器。</param>
	public SimpleReader(LexerData<T> lexerData, object? env, SourceReader reader) :
		base(lexerData, env, false, reader)
	{ }

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <param name="state">DFA 的起始状态。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected override bool NextToken(int state)
	{
		// 最后一次匹配的符号和文本索引。
		int lastAccept = -1, lastIndex = Source.Index;
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
				lastAccept = symbols[0];
				lastIndex = Source.Index;
			}
		}
		if (lastAccept >= 0)
		{
			// 将流调整到与接受状态匹配的状态。
			Source.Index = lastIndex;
			TerminalData<T> terminal = Data.Terminals[lastAccept];
			Controller.DoAction(Start, terminal.Kind, terminal.Action);
			return true;
		}
		return false;
	}
}
