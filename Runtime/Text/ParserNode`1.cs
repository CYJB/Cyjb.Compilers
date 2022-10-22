using System.Diagnostics;

namespace Cyjb.Text;

/// <summary>
/// 表示语法节点。
/// </summary>
/// <typeparam name="T">语法节点标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class ParserNode<T>
	where T : struct
{
	/// <summary>
	/// 行定位器。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly LineLocator? locator;

	/// <summary>
	/// 使用指定的词法单元初始化 <see cref="ParserNode{T}"/> 类的新实例。
	/// </summary>
	/// <param name="token">初始化使用的词法单元。</param>
	public ParserNode(Token<T> token)
	{
		Kind = token.Kind;
		Text = token.Text;
		Span = token.Span;
		locator = token.Locator;
		Value = token.Value;
	}

	/// <summary>
	/// 使用指定的语法节点信息初始化 <see cref="ParserNode{T}"/> 类的新实例。
	/// </summary>
	/// <param name="kind">语法节点的类型。</param>
	/// <param name="span">语法节点的范围。</param>
	/// <param name="locator">使用的行定位器。</param>
	public ParserNode(T kind, TextSpan span, LineLocator? locator = null)
	{
		Kind = kind;
		Text = string.Empty;
		Span = span;
		this.locator = locator;
	}

	/// <summary>
	/// 获取语法节点的类型。
	/// </summary>
	public T Kind { get; init; }
	/// <summary>
	/// 获取语法节点的文本（仅终结符包含文本，非终结符为空字符串）。
	/// </summary>
	public string Text { get; init; }
	/// <summary>
	/// 获取语法节点的范围。
	/// </summary>
	public TextSpan Span { get; init; }
	/// <summary>
	/// 获取语法节点的行列位置范围。
	/// </summary>
	/// <value>语法节点的行列位置范围。</value>
	public LinePositionSpan LinePositionSpan => locator.GetSpan(Span);
	/// <summary>
	/// 获取语法节点的值。
	/// </summary>
	public object? Value { get; internal set; }
	/// <summary>
	/// 获取当前语法节点是否是由错误恢复逻辑生成的。
	/// </summary>
	public bool IsMissing { get; init; } = false;
	/// <summary>
	/// 获取当前语法节点前被跳过的词法单元列表。
	/// </summary>
	public Token<T>[] SkippedTokens { get; init; } = Array.Empty<Token<T>>();
	/// <summary>
	/// 获取当前语法节点是否包含错误。
	/// </summary>
	public bool ContainsError => IsMissing || SkippedTokens.Length > 0;

	/// <summary>
	/// 获取关联到的行定位器。
	/// </summary>
	public LineLocator? Locator => locator;

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		return $"{Kind} at {Span}";
	}
}
