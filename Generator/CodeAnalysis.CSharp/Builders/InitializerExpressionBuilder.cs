using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 初始化表达式的构造器。
/// </summary>
internal sealed class InitializerExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 初始化表达式的类型。
	/// </summary>
	private readonly SyntaxKind kind;
	/// <summary>
	/// 对象的初始化列表。
	/// </summary>
	private readonly List<ExpressionBuilder> initializer = new();
	/// <summary>
	/// 初始化列表的换行情况，<c>0</c> 表示不换行。
	/// </summary>
	private int wrap = 0;

	/// <summary>
	/// 使用指定的表达式类型初始化 <see cref="InitializerExpressionBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="kind">初始化表达式的类型。</param>
	public InitializerExpressionBuilder(SyntaxKind kind)
	{
		this.kind = kind;
	}

	/// <summary>
	/// 获取是否包含初始化的值。
	/// </summary>
	public bool HasInitializer => initializer.Count > 0;

	/// <summary>
	/// 设置初始化列表的换行情况，默认为 <c>0</c> 表示不换行。
	/// </summary>
	/// <param name="wrap">换行情况。</param>
	/// <returns>当前初始化表达式构造器。</returns>
	public InitializerExpressionBuilder Wrap(int wrap)
	{
		if (wrap >= 0)
		{
			this.wrap = wrap;
		}
		return this;
	}

	/// <summary>
	/// 添加初始化列表的值。
	/// </summary>
	/// <param name="value">初始化列表的值。</param>
	/// <returns>当前初始化表达式构造器。</returns>
	public InitializerExpressionBuilder Add(ExpressionBuilder value)
	{
		initializer.Add(value);
		return this;
	}

	/// <summary>
	/// 构造初始化表达式。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>初始化表达式。</returns>
	public override InitializerExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		if (initializer.Count == 0)
		{
			return SyntaxFactory.InitializerExpression(kind).WithLeadingTrivia(SyntaxFactory.Space);
		}
		if (wrap == 0)
		{
			return SyntaxFactory.InitializerExpression(
				kind,
				SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(SyntaxFactory.Space),
				SyntaxBuilder.SeparatedList(initializer.Select((value) => value.GetSyntax(format)), format),
				SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(SyntaxFactory.Space)
			).WithLeadingTrivia(SyntaxFactory.Space);
		}
		else
		{
			SyntaxFormat valueFormat = format.IncDepth();
			return SyntaxFactory.InitializerExpression(
				kind,
				SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(format.EndOfLine),
				SyntaxBuilder.SeparatedList(initializer.Select((value) => value.GetSyntax(valueFormat)),
					valueFormat, wrap, true),
				SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(format.EndOfLine, format.Indentation)
			).WithLeadingTrivia(format.EndOfLine, format.Indentation);
		}
	}
}
