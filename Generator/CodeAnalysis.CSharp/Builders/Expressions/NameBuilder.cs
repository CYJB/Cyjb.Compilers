using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 名称表达式的构造器。
/// </summary>
internal sealed class NameBuilder : ExpressionBuilder
{
	/// <summary>
	/// 待简化的名称。
	/// </summary>
	private static readonly Dictionary<Type, string> SimplifyNames = new()
	{
		{ typeof(bool), "bool" },
		{ typeof(double), "double" },
		{ typeof(float), "float" },
		{ typeof(sbyte), "sbyte" },
		{ typeof(short), "short" },
		{ typeof(int), "int" },
		{ typeof(long), "long" },
		{ typeof(object), "object" },
		{ typeof(byte), "byte" },
		{ typeof(char), "char" },
		{ typeof(ushort), "ushort" },
		{ typeof(uint), "uint" },
		{ typeof(ulong), "ulong" },
		{ typeof(string), "string" },
		{ typeof(void), "void" },
	};

	/// <summary>
	/// 标识符名称。
	/// </summary>
	private readonly string identifier;
	/// <summary>
	/// 类型参数。
	/// </summary>
	private readonly List<TypeBuilder> typeArguments = new();
	/// <summary>
	/// 限定符名称。
	/// </summary>
	private string? qualifier;
	/// <summary>
	/// 已生成的语法节点。
	/// </summary>
	private NameSyntax? syntax;

	/// <summary>
	/// 使用指定的标识符名称初始化 <see cref="NameBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="identifier">标识符名称。</param>
	public NameBuilder(string identifier)
	{
		this.identifier = identifier;
	}

	/// <summary>
	/// 使用指定的类型初始化化 <see cref="NameBuilder"/> 类的新实例。
	/// </summary>
	/// <param name="type">类型。</param>
	public NameBuilder(Type type)
	{
		// 化简部分基础类型。
		if (SimplifyNames.TryGetValue(type, out string? simplifyName))
		{
			identifier = simplifyName;
		}
		else
		{
			identifier = type.Name;
			qualifier = type.Namespace;
			if (type.IsGenericType)
			{
				identifier = type.GetGenericTypeDefinition().Name;
				int idx = identifier.IndexOf('`');
				if (idx >= 0)
				{
					identifier = identifier[..idx];
				}
				if (!type.IsGenericTypeDefinition)
				{
					typeArguments.AddRange(type.GenericTypeArguments.Select(
						type => new TypeBuilder(type)));
				}
			}
		}
	}

	/// <summary>
	/// 设置标识符的限定符。
	/// </summary>
	/// <param name="qualifier">限定符。</param>
	/// <returns>当前标识符名称表达式构造器。</returns>
	public NameBuilder Qualifier(string qualifier)
	{
		this.qualifier = qualifier;
		return this;
	}

	/// <summary>
	/// 添加指定的类型参数。
	/// </summary>
	/// <param name="type">类型参数。</param>
	/// <returns>当前标识符名称表达式构造器。</returns>
	public NameBuilder TypeArgument(TypeBuilder type)
	{
		typeArguments.Add(type);
		return this;
	}

	/// <summary>
	/// 添加指定的类型参数。
	/// </summary>
	/// <typeparam name="T">类型参数。</typeparam>
	/// <returns>当前标识符名称表达式构造器。</returns>
	public NameBuilder TypeArgument<T>()
	{
		typeArguments.Add(typeof(T));
		return this;
	}

	/// <summary>
	/// 添加指定的数组。
	/// </summary>
	/// <param name="rank">数组的秩。</param>
	/// <returns>类型构造器。</returns>
	public TypeBuilder Array(int rank = 1)
	{
		return new TypeBuilder(Syntax).Array(rank);
	}

	/// <summary>
	/// 获取名称语法节点。
	/// </summary>
	public NameSyntax Syntax
	{
		get
		{
			if (syntax == null)
			{
				SimpleNameSyntax simpleName;
				if (typeArguments.Count == 0)
				{
					simpleName = SyntaxFactory.IdentifierName(identifier);
				}
				else
				{
					TypeArgumentListSyntax typeArgs = SyntaxFactory.TypeArgumentList(
						SyntaxBuilder.SeparatedList(typeArguments.Select(arg => arg.Syntax), SyntaxFormat.Default)
					);
					simpleName = SyntaxFactory.GenericName(identifier).WithTypeArgumentList(typeArgs);
				}
				if (qualifier == null)
				{
					syntax = simpleName;
				}
				else
				{
					syntax = SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(qualifier), simpleName);
				}
			}
			return syntax;
		}
	}

	/// <summary>
	/// 构造标识符名称表达式。
	/// </summary>
	/// <param name="format">语法格式。</param>
	/// <returns>标识符名称表达式。</returns>
	public override NameSyntax GetSyntax(SyntaxFormat format)
	{
		return Syntax;
	}

	/// <summary>
	/// 返回当前名称构造器的副本。
	/// </summary>
	/// <returns>当前名称构造器的副本。</returns>
	public NameBuilder Clone()
	{
		NameBuilder clone = new(identifier)
		{
			qualifier = qualifier
		};
		clone.typeArguments.AddRange(typeArguments);
		return clone;
	}

	/// <summary>
	/// 允许从字符串隐式转换为 <see cref="NameBuilder"/> 对象。
	/// </summary>
	/// <param name="typeName">要转换的类型名称。</param>
	/// <returns>转换到的 <see cref="NameBuilder"/> 对象。</returns>
	public static implicit operator NameBuilder(string typeName)
	{
		return new NameBuilder(typeName);
	}

	/// <summary>
	/// 允许从 <see cref="Type"/> 隐式转换为 <see cref="NameBuilder"/> 对象。
	/// </summary>
	/// <param name="type">要转换的类型。</param>
	/// <returns>转换到的 <see cref="NameBuilder"/> 对象。</returns>
	public static implicit operator NameBuilder(Type type)
	{
		return new NameBuilder(type);
	}
}

/// <summary>
/// 提供名称表达式的构造器的辅助功能。
/// </summary>
internal static class NameBuilderUtil
{
	/// <summary>
	/// 创建名称构造器。
	/// </summary>
	/// <param name="name">名称。</param>
	/// <returns>名称构造器。</returns>
	public static NameBuilder AsName(this string name) => new(name);

	/// <summary>
	/// 创建名称构造器。
	/// </summary>
	/// <param name="type">要提取名称的类型。</param>
	/// <returns>名称构造器。</returns>
	public static NameBuilder AsName(this Type type) => new(type);
}

