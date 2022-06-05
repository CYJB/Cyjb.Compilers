namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析中终结符的数据。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
[Serializable]
public class TerminalData<T>
	where T : struct
{
	/// <summary>
	/// 使用指定的终结符信息初始化 <see cref="TerminalData{T}"/> 类的新实例。
	/// </summary>
	/// <param name="kind">终结符的类型。</param>
	/// <param name="action">终结符的动作。</param>
	/// <param name="trailing">向前看信息。</param>
	public TerminalData(T? kind, Action<LexerController<T>>? action, int? trailing)
	{
		Kind = kind;
		Action = action;
		Trailing = trailing;
	}

	/// <summary>
	/// 终结符的类型。
	/// </summary>
	public T? Kind { get; }
	/// <summary>
	/// 终结符的动作。
	/// </summary>
	public Action<LexerController<T>>? Action { get; }
	/// <summary>
	/// 终结符的向前看信息。
	/// </summary>
	/// <remarks><c>null</c> 表示不是向前看符号，正数表示前面长度固定，
	/// 负数表示后面长度固定，<c>0</c> 表示长度不固定。</remarks>
	public int? Trailing { get; }

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		if (Trailing == null)
		{
			return $"[{Kind}]";
		}
		else
		{
			return $"[{Kind}] {Trailing}";
		}
	}
}
