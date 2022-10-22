using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers;

/// <summary>
/// 表示符号的类型。
/// </summary>
internal struct SymbolKind
{
	/// <summary>
	/// 全局符号索引。
	/// </summary>
	private static int globalIndex = 0;
	/// <summary>
	/// 已定义的符号类型。
	/// </summary>
	private readonly static Dictionary<string, SymbolKind> symbolKinds = new();
	/// <summary>
	/// 临时符号列表。
	/// </summary>
	private readonly static Dictionary<int, SymbolKind> tempKinds = new();

	/// <summary>
	/// 创建包含指定表达式的符号类型。
	/// </summary>
	/// <param name="expression">符号类型的表达式。</param>
	/// <returns>包含指定表达式的符号类型。</returns>
	public static SymbolKind GetKind(ExpressionSyntax syntax)
	{
		string key = syntax.ToString();
		if (symbolKinds.TryGetValue(key, out SymbolKind kind))
		{
			return kind;
		}
		kind = new SymbolKind(globalIndex++, syntax);
		symbolKinds[key] = kind;
		return kind;
	}

	/// <summary>
	/// 符号类型的名称。
	/// </summary>
	private readonly string name;

	/// <summary>
	/// 使用指定的符号类型索引和表达式初始化 <see cref="SymbolKind"/> 结构的新实例。
	/// </summary>
	/// <param name="index">符号类型的索引。</param>
	/// <param name="syntax">符号类型的表达式。</param>
	private SymbolKind(int index, ExpressionSyntax syntax)
	{
		Index = index;
		Syntax = syntax;
		if (syntax is MemberAccessExpressionSyntax memberAccessExp)
		{
			name = memberAccessExp.Name.ToString();
		}
		else
		{
			name = syntax.ToString();
		}
	}

	/// <summary>
	/// 获取符号类型的索引。
	/// </summary>
	public int Index { get; }
	/// <summary>
	/// 获取符号类型的表达式。
	/// </summary>
	public ExpressionSyntax Syntax { get; }

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		return name;
	}

	/// <summary>
	/// 支持将 <see cref="SymbolKind"/> 隐式转换为 <see cref="int"/>。
	/// </summary>
	/// <param name="kind">要转换的符号类型。</param>
	/// <returns>转换得到的 <see cref="int"/>。</returns>
	public static implicit operator int(SymbolKind kind)
	{
		return kind.Index;
	}

	/// <summary>
	/// 支持将 <see cref="int"/> 显式转换为 <see cref="SymbolKind"/>。
	/// </summary>
	/// <param name="index">要转换的符号索引。</param>
	/// <returns>转换得到的 <see cref="SymbolKind"/>。</returns>
	public static explicit operator SymbolKind(int index)
	{
		if (tempKinds.TryGetValue(index, out SymbolKind kind))
		{
			return kind;
		}
		// EOF 特殊处理，其他临时符号类型从 1 开始命名。
		string symbolName = (index == -1) ? "endOfFile" : $"symbol_{-1 - index}";
		kind = new(index, SyntaxFactory.IdentifierName(symbolName));
		tempKinds[index] = kind;
		return kind;
	}
}
