using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 提供 <see cref="AttributeSyntax"/> 相关的扩展方法。 
/// </summary>
internal static class AttributeSyntaxUtil
{
	/// <summary>
	/// 返回当前特性列表中的所有特性。
	/// </summary>
	/// <param name="attrList">特性列表。</param>
	/// <returns>特性列表中包含的每个特性。</returns>
	public static IEnumerable<AttributeSyntax> GetAttributes(this SyntaxList<AttributeListSyntax> attrList)
	{
		foreach (AttributeListSyntax list in attrList)
		{
			foreach (AttributeSyntax attr in list.Attributes)
			{
				yield return attr;
			}
		}
	}

	/// <summary>
	/// 返回当前特性的完整名称（包含 Attribute 后缀）。
	/// </summary>
	/// <param name="attr">要检查的特性。</param>
	/// <returns>特性的完整名称。</returns>
	public static string GetFullName(this AttributeSyntax attr)
	{
		if (attr.Name is NameSyntax nameSyntax)
		{
			string name = nameSyntax.GetSimpleName().ToString();
			if (!name.EndsWith("Attribute"))
			{
				name += "Attribute";
			}
			return name;
		}
		else
		{
			// 在正确的语法里，特性好像不太走到这里。
			return attr.Name.ToString();
		}
	}

	/// <summary>
	/// 返回当前特性的参数列表。
	/// </summary>
	/// <param name="attr">要解析的特性。</param>
	/// <param name="model">特性的模型。</param>
	/// <returns>特性参数。</returns>
	/// <exception cref="CSharpException">未能正确解析特性参数。</exception>
	public static AttributeArguments GetArguments(this AttributeSyntax attr, AttributeModel model)
	{
		Dictionary<string, ExpressionSyntax> args = new();
		List<ExpressionSyntax> paramsArgs = new();
		if (attr.ArgumentList != null)
		{
			bool[] parsedCtorArg = new bool[model.ConstructorParameters.Length];
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
					int index = model.IndexOfConstructorArgument(name);
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
					if (!model.NamedParameter.Contains(name))
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
					if (ctorIndex == parsedCtorArg.Length - 1 && model.HasParamsParameter)
					{
						paramsArgs.Add(arg.Expression);
					}
					else
					{
						args[model.ConstructorParameters[ctorIndex].Name] = arg.Expression;
					}
					ctorIndex++;
				}
				else if (model.HasParamsParameter)
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
		foreach (AttributeParameterInfo info in model.ConstructorParameters)
		{
			if (info.IsOptional || info.IsParamArray)
			{
				continue;
			}
			if (!args.ContainsKey(info.Name))
			{
				throw CSharpException.NoCorrespondingArgument(info.Name, model.Signature, attr.GetLocation());
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
}
