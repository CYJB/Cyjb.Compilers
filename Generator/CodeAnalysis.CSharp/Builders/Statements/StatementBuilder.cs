using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 语句的构造器。
/// </summary>
internal abstract class StatementBuilder
{
	/// <summary>
	/// 语句的注释。
	/// </summary>
	private readonly List<string> comments = new();

	/// <summary>
	/// 设置语句的注释。
	/// </summary>
	/// <param name="comment">注释的内容。</param>
	/// <returns>当前变量声明语句构造器。</returns>
	public StatementBuilder Comment(string? comment)
	{
		if (comment == null)
		{
			return this;
		}
		// 支持带换行的注释
		string[] lines = comment.Split(Environment.NewLine);
		// 移除最后的换行
		if (lines[^1].Length == 0)
		{
			comments.AddRange(lines[0..^1]);
		}
		else
		{
			comments.AddRange(lines);
		}
		return this;
	}

	/// <summary>
	/// 构造语句语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>语句语法节点。</returns>
	public abstract StatementSyntax GetSyntax(SyntaxFormat format);

	/// <summary>
	/// 返回当前语句的前置空白。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>当前语句的前置空白。</returns>
	protected IEnumerable<SyntaxTrivia> GetLeadingTrivia(SyntaxFormat format)
	{
		if (comments.Count > 0)
		{
			foreach (string comment in comments)
			{
				yield return format.Indentation;
				yield return SyntaxFactory.Comment($"// {comment}");
				yield return format.EndOfLine;
			}
		}
		yield return format.Indentation;
	}
}
