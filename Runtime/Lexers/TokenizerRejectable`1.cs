using System.Collections.Generic;
using Cyjb.Collections;
using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示支持 Reject 动作的词法分析器。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class TokenizerRejectable<T> : TokenizerBase<T>
	where T : struct
{
	/// <summary>
	/// 接受状态的堆栈。
	/// </summary>
	private readonly Stack<AcceptState> stateStack = new();
	/// <summary>
	/// 候选类型。
	/// </summary>
	private IReadOnlySet<T>? candidates;
	/// <summary>
	/// 当前候选。
	/// </summary>
	private AcceptState curCandidate;
	/// <summary>
	/// 当前候选索引。
	/// </summary>
	private int curCandidateIndex;
	/// <summary>
	/// 获取当前词法分析器剩余的候选类型。
	/// </summary>
	/// <remarks>仅在允许 Reject 动作的词法分析器中，返回剩余的候选类型。</remarks>
	internal override IReadOnlySet<T> Candidates
	{
		get
		{
			if (candidates == null)
			{
				HashSet<T> result = new();
				// 先添加当前候选
				for (int i = curCandidateIndex; i < curCandidate.Symbols.Length; i++)
				{
					var kind = Data.Terminals[curCandidate.Symbols[i]].Kind;
					if (kind.HasValue)
					{
						result.Add(kind.Value);
					}
				}
				result.UnionWith(stateStack.SelectMany(GetCandidates));
				candidates = result.AsReadOnly();
			}
			return candidates;
		}
	}

	/// <summary>
	/// 使用给定的词法分析器信息初始化 <see cref="TokenizerRejectable&lt;T&gt;"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">要使用的词法分析器的数据。</param>
	/// <param name="controller">词法分析控制器。</param>
	/// <param name="reader">要使用的源文件读取器。</param>
	public TokenizerRejectable(LexerData<T> lexerData, LexerController<T> controller, SourceReader reader) :
		base(lexerData, controller, reader)
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
				if (Data.UseShortest)
				{
					// 保存流的索引，避免被误修改影响后续匹配。
					int originIndex = source.Index;
					// 最短匹配时不需要生成候选列表。
					candidates = SetUtil.Empty<T>();
					// 使用最短匹配时，需要先调用 Action。
					foreach (int acceptState in symbols)
					{
						var terminal = Data.Terminals[acceptState];
						if (terminal.UseShortest)
						{
							Controller.DoAction(Start, terminal);
							if (!Controller.IsReject)
							{
								return true;
							}
							source.Index = originIndex;
						}
					}
				}
				// 将接受状态记录在堆栈中。
				stateStack.Push(new AcceptState(symbols, source.Index));
			}
		}
		// 遍历终结状态，执行相应动作。
		while (stateStack.Count > 0)
		{
			curCandidate = stateStack.Pop();
			int index = curCandidate.Index;
			curCandidateIndex = 0;
			foreach (int acceptState in curCandidate.Symbols)
			{
				curCandidateIndex++;
				// 将文本和流调整到与接受状态匹配的状态。
				source.Index = index;
				// 每次都需要清空候选集合，并在使用时重新计算。
				candidates = null;
				Controller.DoAction(Start, Data.Terminals[acceptState]);
				if (!Controller.IsReject)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 返回指定状态中的候选类型。
	/// </summary>
	/// <param name="state">要检查的状态。</param>
	/// <returns><paramref name="state"/> 中包含的候选状态。</returns>
	private IEnumerable<T> GetCandidates(AcceptState state)
	{
		foreach (int acceptState in state.Symbols)
		{
			var kind = Data.Terminals[acceptState].Kind;
			if (kind.HasValue)
			{
				yield return kind.Value;
			}
		}
	}
}
