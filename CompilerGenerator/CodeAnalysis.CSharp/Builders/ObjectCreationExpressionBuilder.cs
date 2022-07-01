using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 对象创建表达式的构造器。
/// </summary>
internal sealed class ObjectCreationExpressionBuilder : ExpressionBuilder
{
	/// <summary>
	/// 对象的类型。
	/// </summary>
	private TypeBuilder? type;
	/// <summary>
	/// 对象的构造参数构造器。
	/// </summary>
	private readonly ArgumentListBuilder arguments = new();
	/// <summary>
	/// 对象的初始化列表构造器。
	/// </summary>
	private readonly InitializerExpressionBuilder initializer = new(SyntaxKind.ObjectInitializerExpression);
	/// <summary>
	/// 表达式的注释。
	/// </summary>
	private readonly List<string> comments = new();

	/// <summary>
	/// 设置对象的类型。
	/// </summary>
	/// <param name="type">对象类型。</param>
	/// <returns>当前对象创建表达式构造器。</returns>
	public ObjectCreationExpressionBuilder Type(TypeBuilder type)
	{
		this.type = type;
		return this;
	}

	/// <summary>
	/// 添加对象的构造参数。
	/// </summary>
	/// <param name="argument">对象的构造参数。</param>
	/// <returns>当前对象创建表达式构造器。</returns>
	public ObjectCreationExpressionBuilder Argument(ExpressionBuilder argument)
	{
		arguments.Add(argument);
		return this;
	}

	/// <summary>
	/// 添加对象的构造参数。
	/// </summary>
	/// <param name="argument">对象的构造参数。</param>
	/// <param name="name">参数的名称。</param>
	/// <returns>当前对象创建表达式构造器。</returns>
	public ObjectCreationExpressionBuilder Argument(ExpressionBuilder argument, string name)
	{
		arguments.Add(argument, name);
		return this;
	}

	/// <summary>
	/// 设置构造参数的换行情况，默认为 <c>0</c> 表示不换行。
	/// </summary>
	/// <param name="wrap">换行情况。</param>
	/// <returns>当前初始化表达式构造器。</returns>
	public ObjectCreationExpressionBuilder ArgumentWrap(int wrap)
	{
		arguments.Wrap(wrap);
		return this;
	}

	/// <summary>
	/// 添加对象的初始化列表。
	/// </summary>
	/// <param name="initializer">对象初始化列表。</param>
	/// <returns>当前对象创建表达式构造器。</returns>
	public ObjectCreationExpressionBuilder Initializer(ExpressionBuilder initializer)
	{
		this.initializer.Add(initializer);
		return this;
	}

	/// <summary>
	/// 设置初始化列表的换行情况，默认为 <c>0</c> 表示不换行。
	/// </summary>
	/// <param name="wrap">换行情况。</param>
	/// <returns>当前初始化表达式构造器。</returns>
	public ObjectCreationExpressionBuilder InitializerWrap(int wrap)
	{
		initializer.Wrap(wrap);
		return this;
	}

	/// <summary>
	/// 设置表达式的注释。
	/// </summary>
	/// <param name="comment">注释的内容。</param>
	/// <returns>当前对象创建表达式构造器。</returns>
	public ObjectCreationExpressionBuilder Comment(string comment)
	{
		comments.Add(comment);
		return this;
	}

	/// <summary>
	/// 构造对象创建表达式语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>对象创建表达式语法节点。</returns>
	public override BaseObjectCreationExpressionSyntax GetSyntax(SyntaxFormat format)
	{
		ArgumentListSyntax args = arguments.GetSyntax(format);
		BaseObjectCreationExpressionSyntax syntax;
		if (type == null)
		{
			syntax = SyntaxFactory.ImplicitObjectCreationExpression(args, null);
		}
		else
		{
			syntax = SyntaxFactory.ObjectCreationExpression(
				SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.Space),
				type.GetSyntax(format),
				args,
				null
			);
		}
		if (initializer.HasInitializer)
		{
			syntax = syntax.WithInitializer(initializer.GetSyntax(format));
		}
		if (comments.Count > 0)
		{
			List<SyntaxTrivia> triviaList = new();
			foreach (string comment in comments)
			{
				triviaList.Add(SyntaxFactory.Comment($"// {comment}"));
				triviaList.Add(format.EndOfLine);
				triviaList.Add(format.Indentation);
			}
			syntax = syntax.WithLeadingTrivia(triviaList);
		}
		return syntax;
	}
}
