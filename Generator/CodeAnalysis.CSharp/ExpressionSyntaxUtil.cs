using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 提供 <see cref="ExpressionSyntax"/> 相关的扩展方法。 
/// </summary>
internal static class ExpressionSyntaxUtil
{
	/// <summary>
	/// 检查当前表达式是否是数组初始化表达式，并返回其初始化列表。
	/// </summary>
	/// <param name="exp">要检查的表达式。</param>
	/// <param name="initializer">初始化列表。</param>
	/// <returns>如果 <paramref name="exp"/> 是 <see cref="ArrayCreationExpressionSyntax"/> 或
	/// <see cref="ImplicitArrayCreationExpressionSyntax"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool TryGetArrayCreationInitializer(this ExpressionSyntax exp,
		out InitializerExpressionSyntax? initializer)
	{
		if (exp is ArrayCreationExpressionSyntax arrayCreation)
		{
			initializer = arrayCreation.Initializer;
			return true;
		}
		else if (exp is ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
		{
			initializer = implicitArrayCreation.Initializer;
			return true;
		}
		else
		{
			initializer = null;
			return false;
		}
	}

	/// <summary>
	/// 返回当前表达式的字符串字面量值。
	/// </summary>
	/// <param name="exp">要检查的表达式。</param>
	/// <returns>字符串的值。</returns>
	/// <exception cref="CSharpException">当前表达式不是字符串字面量。</exception>
	public static string? GetStringLiteral(this ExpressionSyntax exp)
	{
		if (TryGetStringLiteral(exp, out string? value))
		{
			return value;
		}
		throw GetNoExplicitConv(exp, typeof(string));
	}

	/// <summary>
	/// 返回无法转换到指定类型的异常。
	/// </summary>
	/// <param name="exp">无法正常转换的表达式。</param>
	/// <param name="target">要转换到的目标类型。</param>
	private static CSharpException GetNoExplicitConv(ExpressionSyntax exp, Type target)
	{
		object fromType = exp;
		if (exp is LiteralExpressionSyntax literalExp)
		{
			if (exp.IsKind(SyntaxKind.NullLiteralExpression))
			{
				fromType = "null";
			}
			else
			{
				fromType = literalExp.Token.Value!.GetType();
			}
		}
		return CSharpException.NoExplicitConv(fromType, target, exp.GetLocation());
	}

	/// <summary>
	/// 检查当前表达式是否是字符串字面量表达式，并返回其字符串值。
	/// </summary>
	/// <param name="exp">要检查的表达式。</param>
	/// <param name="value">字符串的值。</param>
	/// <returns>如果 <paramref name="exp"/> 是 <see cref="SyntaxKind.StringLiteralExpression"/>，
	/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool TryGetStringLiteral(this ExpressionSyntax exp, [NotNullWhen(true)] out string? value)
	{
		if (exp.IsKind(SyntaxKind.StringLiteralExpression))
		{
			value = (exp as LiteralExpressionSyntax)!.Token.ValueText;
			return true;
		}
		else
		{
			value = null;
			return exp.IsKind(SyntaxKind.NullLiteralExpression);
		}
	}

	/// <summary>
	/// 返回当前表达式的数字字面量表达式值。
	/// </summary>
	/// <typeparam name="T">数字的类型。</typeparam>
	/// <param name="exp">要检查的表达式。</param>
	/// <returns>数字的值。</returns>
	/// <exception cref="CSharpException">当前表达式不是数字字面量。</exception>
	public static T GetNumericLiteral<T>(this ExpressionSyntax exp)
		where T : struct
	{
		if (TryGetNumericLiteral(exp, out T? value))
		{
			return value.Value;
		}
		throw GetNoExplicitConv(exp, typeof(T));
	}

	/// <summary>
	/// 检查当前表达式是否是数字字面量表达式，并返回其值。
	/// </summary>
	/// <typeparam name="T">数字的类型。</typeparam>
	/// <param name="exp">要检查的表达式。</param>
	/// <param name="value">数字的值。</param>
	/// <returns>如果 <paramref name="exp"/> 是 <see cref="SyntaxKind.NumericLiteralExpression"/>，
	/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool TryGetNumericLiteral<T>(this ExpressionSyntax exp, [NotNullWhen(true)] out T? value)
		where T : struct
	{
		if (exp.IsKind(SyntaxKind.NumericLiteralExpression))
		{
			try
			{
				value = (T?)(exp as LiteralExpressionSyntax)!.Token.Value!;
				return true;
			}
			catch { }
		}
		value = null;
		return false;
	}

	/// <summary>
	/// 检查并返回指定枚举类型的值。
	/// </summary>
	/// <typeparam name="TEnum">枚举的类型。</typeparam>
	/// <param name="exp">要检查的表达式。</param>
	/// <returns>枚举的值。</returns>
	/// <exception cref="CSharpException">未能解析枚举的值。</exception>
	public static TEnum GetEnumValue<TEnum>(this ExpressionSyntax exp)
		where TEnum : struct
	{
		if (exp.IsKind(SyntaxKind.BitwiseOrExpression))
		{
			BinaryExpressionSyntax binaryExp = (BinaryExpressionSyntax)exp;
			TEnum left = binaryExp.Left.GetEnumValue<TEnum>();
			TEnum right = binaryExp.Right.GetEnumValue<TEnum>();
			ulong leftValue = GenericConvert.ChangeType<TEnum, ulong>(left);
			ulong rightValue = GenericConvert.ChangeType<TEnum, ulong>(right);
			return GenericConvert.ChangeType<ulong, TEnum>(leftValue | rightValue);
		}
		return exp.GetSingleEnumValue<TEnum>();
	}

	/// <summary>
	/// 检查并返回指定枚举类型的单一值。
	/// </summary>
	/// <typeparam name="TEnum">枚举的类型。</typeparam>
	/// <param name="exp">要检查的表达式。</param>
	/// <returns>枚举的值。</returns>
	/// <exception cref="CSharpException">未能解析枚举的值。</exception>
	private static TEnum GetSingleEnumValue<TEnum>(this ExpressionSyntax exp)
		where TEnum : struct
	{
		ExpressionSyntax valueExp = exp;
		if (exp is MemberAccessExpressionSyntax memberAccess)
		{
			string? enumName;
			valueExp = memberAccess.Expression;
			if (memberAccess.Expression is MemberAccessExpressionSyntax innerMemberAccess)
			{
				enumName = innerMemberAccess.Name.Identifier.Text;
			}
			else if (memberAccess.Expression is IdentifierNameSyntax identifier)
			{
				enumName = identifier.Identifier.Text;
			}
			else
			{
				enumName = null;
			}
			if (enumName == typeof(TEnum).Name)
			{
				string memberName = memberAccess.Name.Identifier.Text;
				if (Enum.TryParse(memberName, out TEnum value))
				{
					return value;
				}
				throw CSharpException.NoSuchMember(enumName, memberName, memberAccess.Name.GetLocation());
			}
		}
		else if (exp.TryGetNumericLiteral(out ulong? value))
		{
			return GenericConvert.ChangeType<ulong?, TEnum>(value);
		}
		throw GetNoExplicitConv(valueExp, typeof(TEnum));
	}
}
