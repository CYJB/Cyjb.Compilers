namespace Cyjb.Compilers.Lexers;

/// <summary>
/// DFA 的数据。
/// </summary>
public sealed class DfaData
{
	/// <summary>
	/// 使用指定的数据初始化 <see cref="DfaData"/> 类的新实例。
	/// </summary>
	/// <param name="states">DFA 的状态列表。</param>
	/// <param name="next">下一状态列表。</param>
	/// <param name="check">状态检查。</param>
	public DfaData(DfaStateData[] states, int[] next, int[] check)
	{
		States = states;
		Next = next;
		Check = check;
	}

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
}
