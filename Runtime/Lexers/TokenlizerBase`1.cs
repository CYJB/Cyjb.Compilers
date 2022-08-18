using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的基类。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal abstract class TokenlizerBase<T> : Tokenlizer<T>
	where T : struct
{
	/// <summary>
	/// 词法分析器的数据。
	/// </summary>
	private readonly LexerData<T> data;
	/// <summary>
	/// 当前词法分析器的控制器。
	/// </summary>
	private readonly LexerController<T> controller;

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="TokenlizerBase{T}"/> 类的新实例。
	/// </summary>
	/// <param name="data">要使用的词法分析器数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	/// <param name="source">要使用的源文件读取器。</param>
	protected TokenlizerBase(LexerData<T> data, LexerController<T> controller, SourceReader source)
		: base(source)
	{
		this.data = data;
		this.controller = controller;
	}

	/// <summary>
	/// 获取词法分析器数据。
	/// </summary>
	protected LexerData<T> Data => data;
	/// <summary>
	/// 获取词法分析器的控制器。
	/// </summary>
	protected LexerController<T> Controller => controller;
	/// <summary>
	/// 获取当前词法单元的起始位置。
	/// </summary>
	/// <value>当前词法单元的起始位置。</value>
	protected int Start { get; private set; }

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <returns>输入流中的下一个词法单元。</returns>
	protected override Token<T> InternalRead()
	{
		while (true)
		{
			ContextData<T> context = controller.CurrentContext;
			if (Source.Peek() == SourceReader.InvalidCharacter)
			{
				// 到达了流的结尾。
				if (context.EofAction != null)
				{
					controller.DoAction(Source.Index, null, context.EofAction);
					if (controller.IsAccept)
					{
						return controller.CreateToken();
					}
				}
				return Token<T>.GetEndOfFile(Source.Index);
			}
			// 起始状态与当前上下文相关。
			int state = context.Index;
			if (data.ContainsBeginningOfLine)
			{
				state *= 2;
				if (Source.IsLineStart)
				{
					// 行首规则。
					state++;
				}
			}
			if (!controller.IsMore)
			{
				Start = Source.Index;
			}
			if (NextToken(state))
			{
				if (!controller.IsMore && !controller.IsReject)
				{
					Source.Drop();
				}
				if (controller.IsAccept)
				{
					return controller.CreateToken();
				}
			}
			else
			{
				// 到达死状态。
				string text = Source.Accept();
				if (text.Length == 0)
				{
					// 如果没有匹配任何字符，强制读入一个字符，可以防止死循环出现。
					Source.Read();
					text = Source.Accept();
				}
				throw new InvalidOperationException(Resources.UnrecognizedToken(text, Start));
			}
		}
	}

	/// <summary>
	/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
	/// </summary>
	/// <param name="startState">DFA 的起始状态。</param>
	/// <returns>词法单元读入是否成功。</returns>
	protected abstract bool NextToken(int startState);

	/// <summary>
	/// 使用源文件中的下一个字符转移到后续状态。
	/// </summary>
	/// <param name="state">当前状态索引。</param>
	/// <returns>转以后的状态，使用 <c>-1</c> 表示没有找到合适的状态。</returns>
	protected int NextState(int state)
	{
		char ch = Source.Read();
		if (ch == SourceReader.InvalidCharacter)
		{
			return DfaStateData.InvalidState;
		}
		int charClass = data.CharClasses.GetCharClass(ch);
		DfaStateData stateData;
		int len = data.Check.Length;
		while (state >= 0)
		{
			stateData = data.States[state];
			int idx = stateData.BaseIndex + charClass;
			if (idx < 0 || idx >= len)
			{
				return DfaStateData.InvalidState;
			}
			if (data.Check[idx] == state)
			{
				return data.Next[idx];
			}
			else
			{
				state = stateData.DefaultState;
			}
		}
		return DfaStateData.InvalidState;
	}
}
