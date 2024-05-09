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
	private readonly TypeBuilder? type;
	/// <summary>
	/// 数组的秩。
	/// </summary>
	private readonly List<ExpressionBuilder> ranks = new();
	/// <summary>
	/// 数组的初始化列表构造器。
	/// </summary>
	private readonly InitializerExpressionBuilder initializer = new(SyntaxKind.ArrayInitializerExpression);

	/// <summary>
	/// 使用指定的元素类型初始化 <see cref="ArrayCreationExpressionBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="type">数组元素的类型。</param>
	public ArrayCreationExpressionBuilder(TypeBuilder? type)
	{
		this.type = type;
	}

	/// <summary>
	/// 设置数组的秩。
	/// </summary>
	/// <param name="ranks">数组的秩。</param>
	/// <returns>当前数组创建表达式构造器。</returns>
	public ArrayCreationExpressionBuilder Rank(params ExpressionBuilder[] ranks)
	{
		this.ranks.AddRange(ranks);
		return this;
	}

	/// <summary>
	/// 设置数组的秩。
	/// </summary>
	/// <param name="ranks">数组的秩。</param>
	/// <returns>当前数组创建表达式构造器。</returns>
	public ArrayCreationExpressionBuilder Rank(params int[] ranks)
	{
		for (int i = 0; i < ranks.Length; i++)
		{
			this.ranks.Add(ranks[i]);
		}
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
				SyntaxFactory.SeparatedList(this.ranks.Select(builder => builder.GetSyntax(format)), SeparatedListUtils.GetSeparators(this.ranks.Count))
			)
		);
		ArrayCreationExpressionSyntax creationSyntax = SyntaxFactory.ArrayCreationExpression(
			SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.Space),
			SyntaxFactory.ArrayType(type.GetSyntax(format), ranks),
			null
		);
		if (initializer.HasInitializer)
		{
			return creationSyntax.WithInitializer(initializer.GetSyntax(format));
		}
		return creationSyntax;
	}
}
