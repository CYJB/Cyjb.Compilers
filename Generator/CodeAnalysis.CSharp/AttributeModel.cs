using System.Reflection;
using System.Text;
using Cyjb.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 表示特性的模型。
/// </summary>
internal sealed class AttributeModel
{
	/// <summary>
	/// 公共实例成员的绑定标志位。
	/// </summary>
	private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

	/// <summary>
	/// 通过指定的特性类型构造其模型。
	/// </summary>
	/// <typeparam name="T">特性类型。</typeparam>
	/// <remarks>只支持单个构造函数。</remarks>
	/// <returns>特性的模型。</returns>
	public static AttributeModel From<T>()
	{
		Type attributeType = typeof(T);
		StringBuilder signature = new($"{attributeType.Name}.{attributeType.Name}(");
		// 选择参数最多的构造函数。
		ParameterInfo[] parameters = attributeType.GetConstructors()
			.OrderByDescending((ctor) => ctor.GetParametersNoCopy().Length)
			.First()
			.GetParametersNoCopy();
		AttrParamInfo[] ctorParams = new AttrParamInfo[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ctorParams[i] = new AttrParamInfo(parameters[i]);
			if (i > 0)
			{
				signature.Append(", ");
			}
			signature.Append(parameters[i].ParameterType.Name);
		}
		signature.Append(')');
		HashSet<string> namedArguments = new();
		foreach (FieldInfo field in attributeType.GetFields(PublicInstance))
		{
			if (field.Attributes != FieldAttributes.Literal)
			{
				namedArguments.Add(field.Name);
			}
		}
		foreach (PropertyInfo property in attributeType.GetProperties(PublicInstance))
		{
			if (property.CanWrite)
			{
				namedArguments.Add(property.Name);
			}
		}
		return new AttributeModel(signature.ToString(), ctorParams, namedArguments);
	}

	/// <summary>
	/// 构造函数的签名。
	/// </summary>
	private readonly string signature;
	/// <summary>
	/// 获取构造函数参数信息列表。
	/// </summary>
	private readonly AttrParamInfo[] constructorParams;
	/// <summary>
	/// 获取是否包含 params 参数。
	/// </summary>
	private readonly bool hasParamsParam;
	/// <summary>
	/// 获取命名参数列表。
	/// </summary>
	private readonly HashSet<string> namedParams;

	/// <summary>
	/// 使用指定的参数信息初始化 <see cref="AttributeModel"/> 类的新实例。
	/// </summary>
	/// <param name="signature">构造函数的签名。</param>
	/// <param name="ctorParams">构造函数参数名称列表。</param>
	/// <param name="namedParams">命名参数列表。</param>
	private AttributeModel(string signature, AttrParamInfo[] ctorParams, HashSet<string> namedParams)
	{
		this.signature = signature;
		constructorParams = ctorParams;
		hasParamsParam = ctorParams.Length > 0 && ctorParams[^1].IsParamArray;
		this.namedParams = namedParams;
	}

	/// <summary>
	/// 返回指定特性的参数列表。
	/// </summary>
	/// <param name="attr">要解析的特性。</param>
	/// <returns>特性参数。</returns>
	/// <exception cref="CSharpException">未能正确解析特性参数。</exception>
	public AttributeArguments GetArguments(AttributeSyntax attr)
	{
		Dictionary<string, ExpressionSyntax> args = new();
		List<ExpressionSyntax> paramsArgs = new();
		if (attr.ArgumentList != null)
		{
			bool[] parsedCtorArg = new bool[constructorParams.Length];
			HashSet<string> parsedNamedArg = new();
			int ctorIndex = 0;
			IdentifierNameSyntax? nameColon = null;
			bool hasNameEquals = false;
			foreach (AttributeArgumentSyntax arg in attr.ArgumentList.Arguments)
			{
				if (arg.NameColon != null)
				{
					nameColon = arg.NameColon.Name;
					string name = nameColon.Identifier.Text;
					int index = IndexOfConstructorArgument(name);
					if (index < 0)
					{
						// 不是有效的参数名称
						throw CSharpException.BadNamedArgument(attr.GetFullName(), name, nameColon.GetLocation());
					}
					else if (parsedCtorArg[index])
					{
						// 参数已被设置。
						throw CSharpException.NamedArgumentUsedInPositional(name, nameColon.GetLocation());
					}
					parsedCtorArg[index] = true;
					args[name] = arg.Expression;
				}
				else if (arg.NameEquals != null)
				{
					IdentifierNameSyntax nameSyntax = arg.NameEquals.Name;
					string name = nameSyntax.Identifier.Text;
					if (!namedParams.Contains(name))
					{
						throw CSharpException.InvalidExprTerm(name, nameSyntax.GetLocation());
					}
					if (!parsedNamedArg.Add(name))
					{
						throw CSharpException.DuplicateNamedAttributeArgument(name, nameSyntax.GetLocation());
					}
					args[name] = arg.Expression;
					hasNameEquals = true;
				}
				else if (nameColon != null)
				{
					throw CSharpException.BadNonTrailingNamedArgument(nameColon.Identifier.Text, nameColon.GetLocation());
				}
				else if (hasNameEquals)
				{
					throw CSharpException.NamedArgumentExpected(arg.GetLocation());
				}
				else if (ctorIndex < parsedCtorArg.Length)
				{
					// 构造函数参数个数范围内
					parsedCtorArg[ctorIndex] = true;
					if (ctorIndex == parsedCtorArg.Length - 1 && hasParamsParam)
					{
						paramsArgs.Add(arg.Expression);
					}
					else
					{
						args[constructorParams[ctorIndex].Name] = arg.Expression;
					}
					ctorIndex++;
				}
				else if (hasParamsParam)
				{
					// 构造函数参数个数范围外，添加到 params 参数
					paramsArgs.Add(arg.Expression);
				}
				else
				{
					// 构造函数参数个数范围外，且无 params 参数，抛错
					throw CSharpException.BadCtorArgCount(attr.GetFullName(),
						attr.ArgumentList.Arguments.Count, attr.GetLocation());
				}
			}
		}
		// 检查必选参数
		foreach (AttrParamInfo info in constructorParams)
		{
			if (info.IsOptional || info.IsParamArray)
			{
				continue;
			}
			if (!args.ContainsKey(info.Name))
			{
				throw CSharpException.NoCorrespondingArgument(info.Name, signature, attr.GetLocation());
			}
		}
		// 检查 params 参数。
		if (paramsArgs.Count == 0)
		{
			return new AttributeArguments(args, Array.Empty<ExpressionSyntax>());
		}
		else if (paramsArgs.Count == 1 && paramsArgs[0].TryGetArrayCreationInitializer(out var initializer))
		{
			if (initializer == null || initializer.Expressions.Count == 0)
			{
				return new AttributeArguments(args, Array.Empty<ExpressionSyntax>());
			}
			else
			{
				return new AttributeArguments(args, initializer.Expressions.ToArray());
			}
		}
		else
		{
			return new AttributeArguments(args, paramsArgs.ToArray());
		}
	}

	/// <summary>
	/// 返回指定构造函数参数名称的索引。
	/// </summary>
	/// <param name="name">要查找的参数名称。</param>
	/// <returns>参数的索引。</returns>
	private int IndexOfConstructorArgument(string name)
	{
		for (int i = 0; i < constructorParams.Length; i++)
		{
			if (constructorParams[i].Name == name)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// 特性的参数信息。
	/// </summary>
	private struct AttrParamInfo
	{
		/// <summary>
		/// 参数名称。
		/// </summary>
		public string Name;
		/// <summary>
		/// 是否是可选参数。
		/// </summary>
		public bool IsOptional;
		/// <summary>
		/// 是否是 params 参数。
		/// </summary>
		public bool IsParamArray;

		/// <summary>
		/// 使用指定的 <see cref="ParameterInfo"/> 初始化。
		/// </summary>
		/// <param name="param">参数信息。</param>
		public AttrParamInfo(ParameterInfo param)
		{
			Name = param.Name!;
			IsOptional = param.IsOptional;
			IsParamArray = param.IsParamArray();
		}
	}
}
