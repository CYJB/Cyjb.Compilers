using Cyjb.Compilers.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 表达式的构造器。
/// </summary>
internal abstract class ExpressionBuilder
{
	/// <summary>
	/// 返回 <c>null</c> 字面量表达式。
	/// </summary>
	/// <returns><c>null</c> 字面量表达式。</returns>
	public static ExpressionBuilder Null() => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

	/// <summary>
	/// 创建 typeof 表达式。
	/// </summary>
	/// <param name="type">类型。</param>
	/// <returns>typeof 表达式。</returns>
	public static TypeOfExpressionBuilder TypeOf(TypeBuilder type) => new(type);

	/// <summary>
	/// 创建初始化表达式的构造器。
	/// </summary>
	/// <param name="kind">初始化表达式的类型。</param>
	/// <returns>初始化表达式的构造器。</returns>
	public static InitializerExpressionBuilder InitializerExpression(SyntaxKind kind) => new(kind);

	/// <summary>
	/// 创建对象创建表达式的构造器。
	/// </summary>
	/// <param name="type">对象的类型。</param>
	/// <returns>对象创建表达式的构造器。</returns>
	public static ObjectCreationExpressionBuilder CreateObject(TypeBuilder? type = null) => new(type);

	/// <summary>
	/// 创建对象创建表达式的构造器。
	/// </summary>
	/// <typeparam name="T">对象的类型。</typeparam>
	/// <returns>对象创建表达式的构造器。</returns>
	public static ObjectCreationExpressionBuilder CreateObject<T>() => new(SyntaxBuilder.Type<T>());

	/// <summary>
	/// 创建数组创建表达式的构造器。
	/// </summary>
	/// <param name="type">数组元素的类型。</param>
	/// <returns>数组创建表达式的构造器。</returns>
	public static ArrayCreationExpressionBuilder CreateArray(TypeBuilder? type = null) => new(type);

	/// <summary>
	/// 创建 Lambda 表达式的构造器。
	/// </summary>
	/// <returns>Lambda 表达式的构造器。</returns>
	public static LambdaExpressionBuilder Lambda() => new();

	/// <summary>
	/// 构造表达式语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>表达式语法节点。</returns>
	public abstract ExpressionSyntax GetSyntax(SyntaxFormat format);

	/// <summary>
	/// 返回当前表达式的括号表达式构造器。
	/// </summary>
	/// <returns>括号表达式构造器。</returns>
	public ParenthesizedExpressionBuilder Parenthesized()
	{
		return new ParenthesizedExpressionBuilder(this);
	}

	/// <summary>
	/// 返回强制转换到指定类型的表达式构造器。
	/// </summary>
	/// <param name="type">要强制转换到的类型。</param>
	/// <returns>强制类型转换表达式构造器。</returns>
	public CastExpressionBuilder Cast(TypeBuilder type)
	{
		return new CastExpressionBuilder(type, this);
	}

	/// <summary>
	/// 返回将指定值赋值给当前变量的表达式。
	/// </summary>
	/// <param name="value">要赋值的值。</param>
	/// <returns>赋值表达式的构造器。</returns>
	public AssignmentExpressionBuilder Assign(ExpressionBuilder value)
	{
		return new AssignmentExpressionBuilder(SyntaxKind.SimpleAssignmentExpression, this, value);
	}

	/// <summary>
	/// 返回将指定值加和赋值给当前变量的表达式。
	/// </summary>
	/// <param name="value">要加和赋值的值。</param>
	/// <returns>赋值表达式的构造器。</returns>
	public AssignmentExpressionBuilder AddAssign(ExpressionBuilder value)
	{
		return new AssignmentExpressionBuilder(SyntaxKind.AddAssignmentExpression, this, value);
	}

	/// <summary>
	/// 返回表达式的逻辑非的表达式。
	/// </summary>
	/// <returns>前缀一元表达式的构造器。</returns>
	public PrefixUnaryExpressionBuilder LogicalNot()
	{
		return new PrefixUnaryExpressionBuilder(SyntaxKind.LogicalNotExpression, this);
	}

	/// <summary>
	/// 返回加上指定值的表达式。
	/// </summary>
	/// <param name="value">要加上的值。</param>
	/// <returns>二元表达式的构造器。</returns>
	public BinaryExpressionBuilder Add(ExpressionBuilder value)
	{
		return new BinaryExpressionBuilder(SyntaxKind.AddExpression, this, value);
	}

	/// <summary>
	/// 返回大于指定值的表达式。
	/// </summary>
	/// <param name="value">要比较的值。</param>
	/// <returns>二元表达式的构造器。</returns>
	public BinaryExpressionBuilder GreaterThan(ExpressionBuilder value)
	{
		return new BinaryExpressionBuilder(SyntaxKind.GreaterThanExpression, this, value);
	}

	/// <summary>
	/// 返回大于等于指定值的表达式。
	/// </summary>
	/// <param name="value">要比较的值。</param>
	/// <returns>二元表达式的构造器。</returns>
	public BinaryExpressionBuilder GreaterThanOrEqual(ExpressionBuilder value)
	{
		return new BinaryExpressionBuilder(SyntaxKind.GreaterThanOrEqualExpression, this, value);
	}

	/// <summary>
	/// 返回访问指定成员的表达式构造器。
	/// </summary>
	/// <param name="name">要访问的成员名称。</param>
	/// <returns>成员访问表达式构造器。</returns>
	public MemberAccessExpressionBuilder AccessMember(string name)
	{
		return new MemberAccessExpressionBuilder(this, name);
	}

	/// <summary>
	/// 返回访问指定成员的表达式构造器。
	/// </summary>
	/// <param name="name">要访问的成员名称。</param>
	/// <param name="otherNames">要访问的其它成员名称。</param>
	/// <returns>成员访问表达式构造器。</returns>
	public MemberAccessExpressionBuilder AccessMember(string name, params string[] otherNames)
	{
		MemberAccessExpressionBuilder result = new(this, name);
		foreach (string otherName in otherNames)
		{
			result = new MemberAccessExpressionBuilder(result, otherName);
		}
		return result;
	}

	/// <summary>
	/// 返回访问指定元素的表达式构造器。
	/// </summary>
	/// <param name="expression">要访问的元素。</param>
	/// <returns>元素访问表达式构造器。</returns>
	public ElementAccessExpressionBuilder AccessElement(ExpressionBuilder expression)
	{
		return new ElementAccessExpressionBuilder(this).Argument(expression);
	}

	/// <summary>
	/// 返回调用当前方法的表达式。
	/// </summary>
	/// <param name="arguments">要调用的方法参数。</param>
	/// <returns>方法调用表达式的构造器。</returns>
	public InvocationExpressionBuilder Invoke(params ExpressionBuilder[] arguments)
	{
		InvocationExpressionBuilder builder = new(this);
		foreach (ExpressionBuilder arg in arguments)
		{
			builder.Argument(arg);
		}
		return builder;
	}

	/// <summary>
	/// 返回当前表达式的语句。
	/// </summary>
	/// <returns>当前表达式的语句。</returns>
	public ExpressionStatementBuilder AsStatement()
	{
		return new ExpressionStatementBuilder(this);
	}

	/// <summary>
	/// 允许从表达式节点隐式转换为 <see cref="ExpressionBuilder"/> 对象。
	/// </summary>
	/// <param name="expression">要转换的 <see cref="ExpressionSyntax"/> 对象。</param>
	/// <returns>转换到的 <see cref="ExpressionBuilder"/> 对象。</returns>
	public static implicit operator ExpressionBuilder(ExpressionSyntax expression)
	{
		return new ExpressionWrapperBuilder(expression);
	}

	/// <summary>
	/// 允许从字符串字面量隐式转换为 <see cref="ExpressionBuilder"/> 对象。
	/// </summary>
	/// <param name="value">要转换的字符串字面量。</param>
	/// <returns>转换到的 <see cref="ExpressionBuilder"/> 对象。</returns>
	public static implicit operator ExpressionBuilder(string? value)
	{
		if (value == null)
		{
			return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
		}
		else
		{
			return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
		}
	}

	/// <summary>
	/// 允许从 <see cref="bool"/> 字面量隐式转换为 <see cref="ExpressionBuilder"/> 对象。
	/// </summary>
	/// <param name="value">要转换的 <see cref="bool"/> 字面量。</param>
	/// <returns>转换到的 <see cref="ExpressionBuilder"/> 对象。</returns>
	public static implicit operator ExpressionBuilder(bool value)
	{
		if (value)
		{
			return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
		}
		else
		{
			return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
		}
	}

	/// <summary>
	/// 允许从 <see cref="char"/> 字面量隐式转换为 <see cref="ExpressionBuilder"/> 对象。
	/// </summary>
	/// <param name="value">要转换的 <see cref="char"/> 字面量。</param>
	/// <returns>转换到的 <see cref="ExpressionBuilder"/> 对象。</returns>
	public static implicit operator ExpressionBuilder(char value)
	{
		if (value == char.MinValue)
		{
			return "char.MinValue".AsName();
		}
		return SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(value));
	}

	/// <summary>
	/// 允许从 <see cref="int"/> 字面量隐式转换为 <see cref="ExpressionBuilder"/> 对象。
	/// </summary>
	/// <param name="value">要转换的 <see cref="int"/> 字面量。</param>
	/// <returns>转换到的 <see cref="ExpressionBuilder"/> 对象。</returns>
	public static implicit operator ExpressionBuilder(int value)
	{
		if (value == short.MinValue)
		{
			return "short.MinValue".AsName();
		}
		return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
	}

	/// <summary>
	/// 允许从 <see cref="uint"/> 字面量隐式转换为 <see cref="ExpressionBuilder"/> 对象。
	/// </summary>
	/// <param name="value">要转换的 <see cref="uint"/> 字面量。</param>
	/// <returns>转换到的 <see cref="ExpressionBuilder"/> 对象。</returns>
	public static implicit operator ExpressionBuilder(uint value)
	{
		if (value == uint.MaxValue)
		{
			return "uint.MaxValue".AsName();
		}
		return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
	}

	/// <summary>
	/// 表达式节点的构造器包装。
	/// </summary>
	private sealed class ExpressionWrapperBuilder : ExpressionBuilder
	{
		/// <summary>
		/// 被包装的表达式节点。
		/// </summary>
		private readonly ExpressionSyntax expression;

		/// <summary>
		/// 使用指定的表达式节点初始化 <see cref="ExpressionWrapperBuilder"/> 类的新实例。
		/// </summary>
		/// <param name="expression">被包装的表达式节点。</param>
		public ExpressionWrapperBuilder(ExpressionSyntax expression)
		{
			this.expression = expression;
		}

		/// <summary>
		/// 构造表达式语法节点。
		/// </summary>
		/// <param name="format">语法的格式信息。</param>
		/// <returns>表达式语法节点。</returns>
		public override ExpressionSyntax GetSyntax(SyntaxFormat format)
		{
			return expression;
		}
	}
}

/// <summary>
/// 提供表达式的构造器的辅助功能。
/// </summary>
internal static class ExpressionBuilderUtil
{
	/// <summary>
	/// 返回指定 <see cref="int[]"/> 的字面量数组表达式。
	/// </summary>
	/// <param name="value">字面量的值。</param>
	/// <param name="wrap">换行情况。</param>
	/// <returns>指定 <see cref="int[]"/> 的字面量数组表达式。</returns>
	public static ExpressionBuilder AsLiteral(this IEnumerable<int> value, int wrap = 0)
	{
		var builder = ExpressionBuilder.CreateArray().InitializerWrap(wrap);
		bool isEmpty = true;
		foreach (int item in value)
		{
			builder.Initializer(item);
			isEmpty = false;
		}
		if (isEmpty)
		{
			return "Array".AsName().Qualifier("System").AccessMember("Empty<int>").Invoke();
		}
		else
		{
			return builder;
		}
	}

	/// <summary>
	/// 返回指定 <see cref="uint[]"/> 的字面量数组表达式。
	/// </summary>
	/// <param name="value">字面量的值。</param>
	/// <param name="wrap">换行情况。</param>
	/// <returns>指定 <see cref="uint[]"/> 的字面量数组表达式。</returns>
	public static ExpressionBuilder AsLiteral(this IEnumerable<uint> value, int wrap = 0)
	{
		var builder = ExpressionBuilder.CreateArray().InitializerWrap(wrap);
		bool isEmpty = true;
		foreach (uint item in value)
		{
			builder.Initializer(item);
			isEmpty = false;
		}
		if (isEmpty)
		{
			return "Array".AsName().Qualifier("System").AccessMember("Empty<uint>").Invoke();
		}
		else
		{
			return builder;
		}
	}
}

