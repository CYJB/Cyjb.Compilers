namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析中终结符的数据。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class TerminalData<T>
	where T : struct
{
	/// <summary>
	/// 使用指定的终结符信息初始化 <see cref="TerminalData{T}"/> 类的新实例。
	/// </summary>
	/// <param name="kind">终结符的类型。</param>
	/// <param name="value">终结符的值。</param>
	/// <param name="action">终结符的动作。</param>
	/// <param name="trailing">向前看信息。</param>
	/// <param name="useShortest">是否使用最短匹配。</param>
	public TerminalData(T? kind = null, object? value = null, Delegate? action = null,
		int? trailing = null, bool useShortest = false)
	{
		Kind = kind;
		Value = value;
		Action = action;
		Trailing = trailing;
		UseShortest = useShortest;
	}

	/// <summary>
	/// 获取终结符的类型。
	/// </summary>
	public T? Kind { get; }
	/// <summary>
	/// 获取终结符的值。
	/// </summary>
	public object? Value { get; }
	/// <summary>
	/// 获取终结符的动作。
	/// </summary>
	public Delegate? Action { get; }
	/// <summary>
	/// 获取终结符的向前看信息。
	/// </summary>
	/// <remarks><c>null</c> 表示不是向前看符号，正数表示前面长度固定，
	/// 负数表示后面长度固定，<c>0</c> 表示长度不固定。</remarks>
	public int? Trailing { get; }
	/// <summary>
	/// 获取设置是否使用终结符的最短匹配。
	/// </summary>
	/// <remarks>默认都会使用正则表达式的最长匹配，允许指定为使用最短匹配，
	/// 会在遇到第一个匹配时立即返回结果。</remarks>
	public bool UseShortest { get; }

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		string mark = UseShortest ? " (s)" : "";
		if (Trailing == null)
		{
			return $"[{Kind}]{mark}";
		}
		else
		{
			return $"[{Kind}] {Trailing}{mark}";
		}
	}
}
