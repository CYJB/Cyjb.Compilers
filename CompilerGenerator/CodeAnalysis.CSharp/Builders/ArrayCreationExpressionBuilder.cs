using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 数组创建表达式的构造器。
/// </summary>
internal sealed class ArrayCreationExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 数组元素的类型。
	/// </summary>
	private TypeBuilder? type;
	/// <summary>
	/// 数组的秩。
	/// </summary>
	private readonly List<ExpressionSyntax> ranks = new();
	/// <summary>
	/// 数组的初始化列表构造器。
	/// </summary>
	private readonly InitializerExpressionBuilder initializer = new(SyntaxKind.ArrayInitializerExpression);

	/// <summary>
	/// 设置数组元素的类型。
	/// </summary>
	/// <param name="type">数组元素的类型。</param>
	/// <returns>当前数组创建表达式构造器。</returns>
	public ArrayCreationExpressionBuilder Type(TypeBuilder type)
	{
		this.type = type;
		return this;
	}

	/// <summary>
	/// 设置数组的秩。
	/// </summary>
	/// <param name="ranks">数组的秩。</param>
	/// <returns>当前数组创建表达式构造器。</returns>
	public ArrayCreationExpressionBuilder Rank(params ExpressionSyntax[] ranks)
	{
		this.ranks.AddRange(ranks);
		return this;
	}

	/// <summary>
	/// 添加数组的初始化列表。
	/// </summary>
	/// <param name="initializer">数组初始化列表。</param>
	/// <returns>当前数组创建表达式构造器。</returns>
	public ArrayCreationExpressionBuilder Initializer(ExpressionBuilder initializer)
	{
		this.initializer.Add(initializer);
		return this;
	}

	/// <summary>
	/// 设置初始化列表的换行情况，默认为 <c>0</c> 表示不换行。
	/// </summary>
	/// <param name="wrap">换行情况。</param>
	/// <returns>当前数组创建表达式构造器。</returns>
	public ArrayCreationExpressionBuilder InitializerWrap(int wrap)
	{
		initializer.Wrap(wrap);
		return this;
	}

	/// <summary>
	/// 构造表达式语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>表达式语法节点。</returns>
	public override ExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		if (type == null)
		{
			return SyntaxFactory.ImplicitArrayCreationExpression(
				SyntaxFactory.TokenList(),
				initializer.GetSyntax(format)
			);
		}
		SyntaxList<ArrayRankSpecifierSyntax> ranks = SyntaxFactory.SingletonList(
			SyntaxFactory.ArrayRankSpecifier(
				SyntaxFactory.SeparatedList(this.ranks, SeparatedListUtils.GetSeparators(this.ranks.Count))
			)
		);
		ArrayCreationExpressionSyntax creationSyntax = SyntaxFactory.ArrayCreationExpression(
			SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.Space),
			SyntaxFactory.ArrayType(type.GetSyntax(format), ranks),
			null
		);
		return creationSyntax.WithInitializer(initializer.GetSyntax(format));
	}
}
