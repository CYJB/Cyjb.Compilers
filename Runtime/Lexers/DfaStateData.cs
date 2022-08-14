using System.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// DFA 的状态数据。
/// </summary>
public sealed class DfaStateData
{
	/// <summary>
	/// 表示无效的状态。
	/// </summary>
	public const int InvalidState = -1;

	/// <summary>
	/// 使用指定的索引和符号初始化 <see cref="DfaStateData"/> 类的新实例。
	/// </summary>
	/// <param name="baseIndex">基索引。</param>
	/// <param name="defaultState">默认状态。</param>
	/// <param name="symbols">符号索引。</param>
	public DfaStateData(int baseIndex, int defaultState, params int[] symbols)
	{
		BaseIndex = baseIndex;
		DefaultState = defaultState;
		Symbols = symbols;
	}

	/// <summary>
	/// 获取状态的基索引。
	/// </summary>
	public int BaseIndex { get; }
	/// <summary>
	/// 获取默认状态。
	/// </summary>
	public int DefaultState { get; }
	/// <summary>
	/// 符号索引列表，按符号索引排序。
	/// </summary>
	/// <remarks>使用负数表示向前看的头状态。</remarks>
	public int[] Symbols { get; }

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		StringBuilder builder = new();
		if (BaseIndex == int.MinValue)
		{
			builder.Append('-');
		}
		else
		{
			builder.Append(BaseIndex);
		}
		builder.Append(", ");
		if (DefaultState == InvalidState)
		{
			builder.Append('-');
		}
		else
		{
			builder.Append(DefaultState);
		}
		if (Symbols.Length > 0)
		{
			builder.Append(" [");
			builder.Append(string.Join(",", Symbols));
			builder.Append(']');
		}
		return builder.ToString();
	}
}
