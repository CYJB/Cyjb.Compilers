using Cyjb.Collections;

namespace Cyjb.Compilers;

/// <summary>
/// 提供三数组压缩的能力。
/// </summary>
/// <typeparam name="T">状态的类型。</typeparam>
internal class ArrayCompress<T>
{
	/// <summary>
	/// 表示无效的值。
	/// </summary>
	private readonly int invalidValue;
	/// <summary>
	/// 表示无效的状态。
	/// </summary>
	private readonly T invalidState;
	/// <summary>
	/// 下一状态列表。
	/// </summary>
	private readonly List<int> next = new();
	/// <summary>
	/// 状态检查。
	/// </summary>
	private readonly List<T> check = new();
	/// <summary>
	/// 下一状态列表的可用空当列表。
	/// </summary>
	private readonly BitList spaces = new();
	/// <summary>
	/// 下一个可用的 next 空当。
	/// </summary>
	private int nextSpaceIndex = 0;
	/// <summary>
	/// 最后一个被填充的位置。
	/// </summary>
	private int lastFilledIndex = 0;

	/// <summary>
	/// 使用指定的无效值和无效状态初始化 <see cref="ArrayCompress{T}"/> 类的新实例。
	/// </summary>
	/// <param name="invalidValue">无效的值。</param>
	/// <param name="invalidState">无效的状态。</param>
	public ArrayCompress(int invalidValue, T invalidState)
	{
		this.invalidValue = invalidValue;
		this.invalidState = invalidState;
	}

	/// <summary>
	/// 添加指定的转移。
	/// </summary>
	/// <param name="currentState">当前状态。</param>
	/// <param name="transition">要添加的转移。</param>
	/// <returns>指定转移的基线索引。</returns>
	public int AddTransition(T currentState, IList<KeyValuePair<int, int>> transition)
	{
		if (transition.Count == 0)
		{
			return int.MinValue;
		}
		int minIndex = transition[0].Key;
		int length = transition[^1].Key + 1 - minIndex;
		BitList pattern = new(length, false);
		foreach (var (key, _) in transition)
		{
			pattern[key - minIndex] = true;
		}
		int spaceIndex = spaces.FindSpace(pattern, nextSpaceIndex);
		// 确保空白记录包含足够的空间
		if (spaces.Count < spaceIndex + length)
		{
			spaces.Resize(spaceIndex + length);
		}
		int restCount = spaceIndex + length - next.Count;
		if (restCount > 0)
		{
			next.AddRange(Enumerable.Repeat(invalidValue, restCount));
			check.AddRange(Enumerable.Repeat(invalidState, restCount));
		}
		spaceIndex -= minIndex;
		foreach (var (key, state) in transition)
		{
			int idx = key + spaceIndex;
			spaces[idx] = true;
			next[idx] = state;
			check[idx] = currentState;
		}
		// 更新 nextSpaceIndex 和 lastFilledIndex
		nextSpaceIndex = spaces.IndexOf(false, nextSpaceIndex);
		if (nextSpaceIndex < 0)
		{
			nextSpaceIndex = spaces.Count;
		}
		for (int j = spaces.Count - 1; j >= lastFilledIndex; j--)
		{
			if (spaces[j])
			{
				lastFilledIndex = j;
				break;
			}
		}
		return spaceIndex;
	}

	/// <summary>
	/// 返回下一状态列表。
	/// </summary>
	public int[] GetNext()
	{
		return next.ToArray();
	}

	/// <summary>
	/// 返回状态检查。
	/// </summary>
	public T[] GetCheck()
	{
		return check.ToArray();
	}
}
