using System.Diagnostics;
using System.Text;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 LR 状态。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class LRState<T>
	where T : struct
{
	/// <summary>
	/// 当前状态中包含的项。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly LRItemCollection<T> items;
	/// <summary>
	/// 当前状态的 GOTO 表。
	/// </summary>
	private readonly Dictionary<Symbol<T>, LRState<T>> gotos = new();

	/// <summary>
	/// 使用指定的项集合初始化 <see cref="LRState{T}"/> 类的新实例。
	/// </summary>
	/// <param name="index">当前状态的索引。</param>
	/// <param name="items">当前状态中包含的项。</param>
	internal LRState(int index, LRItemCollection<T> items)
	{
		Index = index;
		this.items = items;
	}

	/// <summary>
	/// 获取当前状态的索引。
	/// </summary>
	public int Index { get; }
	/// <summary>
	/// 获取当前状态中包含的项。
	/// </summary>
	public LRItemCollection<T> Items => items;
	/// <summary>
	/// 获取或设置当前状态中核心项的个数。
	/// </summary>
	public int CoreItemCount { get; set; }
	/// <summary>
	/// 获取当前状态的 GOTO 表。
	/// </summary>
	public Dictionary<Symbol<T>, LRState<T>> Gotos => gotos;
	/// <summary>
	/// 获取当前状态的动作集合。
	/// </summary>
	public ActionCollection<T> Actions { get; } = new();
	/// <summary>
	/// 获取当前状态的冲突集合。
	/// </summary>
	public Dictionary<Symbol<T>, ParserConflict<T>> Conflicts { get; } = new();

	/// <summary>
	/// 获取用于错误恢复的核心项。
	/// </summary>
	/// <returns>用于错误恢复的核心项。</returns>
	/// <remarks>会选择核心项中定点右边符号最少的。</remarks>
	public LRItem<T> GetRecoverItem()
	{
		LRItem<T> recoverItem = items[0];
		int size = recoverItem.Production.Body.Length - recoverItem.Index;
		for (int i = 1; i < CoreItemCount; i++)
		{
			LRItem<T> item = items[i];
			int curSize = item.Production.Body.Length - item.Index;
			if (curSize < size)
			{
				recoverItem = item;
			}
		}
		return recoverItem;
	}

	/// <summary>
	/// 返回当前状态用于错误恢复的 FOLLOW 集合。
	/// </summary>
	/// <param name="compress">三数组压缩。</param>
	/// <returns>用于错误恢复的 FOLLOW 集合的基索引。</returns>
	/// <remarks>如果某状态出现了未识别的终结符，且进入恐慌模式的错误恢复，
	/// 需要尝试将该状态规约掉，这时需要跳过多个终结符，直到出现 FOLLOW 集合中的终结符。</remarks>
	public int GetFollowBaseIndex(ArrayCompress<int> compress)
	{
		List<KeyValuePair<int, int>> follows = new();
		foreach (var (_, nextState) in gotos)
		{
			// GOTO 对应状态的非终结符，FOLLOW 集合就是规约后的状态可以接受的状态的终结符集合。
			if (gotos.TryGetValue(nextState.GetRecoverItem().Production.Head, out LRState<T>? followState))
			{
				follows.Add(new KeyValuePair<int, int>(nextState.Index, followState.Index));
			}
		}
		follows.Sort((left, right) => left.Key - right.Key);
		return compress.AddTransition(Index, follows);
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		StringBuilder text = new();
		for (int i = 0; i < CoreItemCount; i++)
		{
			text.AppendLine(items[i].ToString());
		}
		return text.ToString();
	}
}
