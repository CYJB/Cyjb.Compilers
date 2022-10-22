using System.Text;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示语法分析器的冲突和解决办法。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal class ParserConflict<T> : ReadOnlyListBase<string>
	where T : struct
{
	/// <summary>
	/// 冲突的产生式列表。
	/// </summary>
	private readonly List<string> conflicts = new();

	/// <summary>
	/// 使用指定的状态和符号初始化 <see cref="ParserConflict{T}"/> 类的新实例。
	/// </summary>
	/// <param name="state">产生冲突的状态。</param>
	/// <param name="symbol">产生冲突的终结符。</param>
	/// <param name="selected">被选择的候选动作。</param>
	public ParserConflict(LRState<T> state, Symbol<T> symbol, CandidateAction<T> selected)
	{
		State = state;
		Symbol = symbol;
		Selected = GetConflictInfo(selected);
	}

	/// <summary>
	/// 获取产生冲突的状态。
	/// </summary>
	/// <value>产生冲突的状态。</value>
	public LRState<T> State { get; }
	/// <summary>
	/// 获取产生冲突的终结符。
	/// </summary>
	/// <value>产生冲突的终结符。</value>
	public Symbol<T> Symbol { get; }
	/// <summary>
	/// 被选择的产生式。
	/// </summary>
	public string Selected { get; private set; }

	/// <summary>
	/// 设置被选择的产生式。
	/// </summary>
	/// <param name="info">冲突信息。</param>
	public void SetSelected(CandidateAction<T> info)
	{
		conflicts.Add(Selected);
		Selected = GetConflictInfo(info);
	}

	/// <summary>
	/// 添加指定的冲突信息。
	/// </summary>
	/// <param name="info">冲突信息。</param>
	public void Add(CandidateAction<T> info)
	{
		conflicts.Add(GetConflictInfo(info));
	}

	/// <summary>
	/// 替换指定的冲突信息。
	/// </summary>
	/// <param name="info">冲突信息。</param>
	public void Replace(CandidateAction<T> info)
	{
		conflicts.Clear();
		Selected = GetConflictInfo(info);
	}

	/// <summary>
	/// 返回冲突信息的字符串表示。
	/// </summary>
	/// <param name="info">冲突信息。</param>
	/// <returns>冲突信息的字符串表示。</returns>
	private static string GetConflictInfo(CandidateAction<T> info)
	{
		string type = info.Action switch
		{
			ReduceAction<T> => "reduce",
			_ => "shift",
		};
		return $"{info.Item} ({type})";
	}

	#region ReadOnlyListBase<string> 成员

	/// <summary>
	/// 获取当前集合包含的元素数。
	/// </summary>
	/// <value>当前集合中包含的元素数。</value>
	public override int Count => conflicts.Count;

	/// <summary>
	/// 返回指定索引处的元素。
	/// </summary>
	/// <param name="index">要返回元素的从零开始的索引。</param>
	/// <returns>位于指定索引处的元素。</returns>
	protected override string GetItemAt(int index)
	{
		return conflicts[index];
	}

	/// <summary>
	/// 确定当前列表中指定对象的索引。
	/// </summary>
	/// <param name="item">要在当前列表中定位的对象。</param>
	/// <returns>如果在当前列表中找到 <paramref name="item"/>，则为该对象的索引；否则为 <c>-1</c>。</returns>
	public override int IndexOf(string item)
	{
		return conflicts.IndexOf(item);
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<string> GetEnumerator()
	{
		return conflicts.GetEnumerator();
	}

	#endregion // ReadOnlyListBase<string> 成员

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		StringBuilder text = new();
		text.Append("state ");
		text.Append(State.Index);
		text.Append(", ");
		text.Append(Symbol.Name);
		text.Append(": ");
		text.AppendLine(Selected);
		bool isFirst = true;
		foreach (string item in conflicts)
		{
			if (isFirst)
			{
				text.Append("  conflict: ");
				isFirst = false;
			}
			else
			{
				text.Append("            ");
			}
			text.AppendLine(item);
		}
		return text.ToString();
	}
}
