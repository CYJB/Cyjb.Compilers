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
	/// <param name="trans">DFA 的状态转移。</param>
	public DfaData(int[] states, int[] trans)
	{
		States = states;
		Trans = trans;
	}

	/// <summary>
	/// 获取 DFA 的状态列表。
	/// </summary>
	public int[] States { get; }
	/// <summary>
	/// 获取 DFA 的状态转移。
	/// </summary>
	/// <remarks>使用 <c>trans[i]</c> 表示 <c>check</c>，<c>trans[i+1]</c> 表示 <c>next</c>。</remarks>
	public int[] Trans { get; }
}
