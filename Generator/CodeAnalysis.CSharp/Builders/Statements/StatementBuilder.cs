using Cyjb.Compilers.CodeAnalysis;
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
	internal protected readonly List<string> comments = new();

	/// <summary>
	/// 返回为当前语句添加标签的语句。
	/// </summary>
	/// <param name="label">标签的标识符。</param>
	/// <returns>标签语句。</returns>
	public LabeledStatementBuilder Labeled(string label)
	{
		return new LabeledStatementBuilder(label, this);
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

/// <summary>
/// 提供语句构造器的辅助功能。
/// </summary>
internal static class StatementBuilderUtil
{
	/// <summary>
	/// 设置语句的注释。
	/// </summary>
	/// <typeparam name="T">语句的类型。</typeparam>
	/// <param name="builder">成员声明构造器。</param>
	/// <param name="comment">注释的内容。</param>
	/// <returns>当前语句构造器。</returns>
	public static T Comment<T>(this T builder, string? comment)
		where T : StatementBuilder
	{
		if (comment == null)
		{
			return builder;
		}
		// 支持带换行的注释
		string[] lines = comment.Split(Environment.NewLine);
		// 移除最后的换行
		if (lines[^1].Length == 0)
		{
			builder.comments.AddRange(lines[0..^1]);
		}
		else
		{
			builder.comments.AddRange(lines);
		}
		return builder;
	}
}
