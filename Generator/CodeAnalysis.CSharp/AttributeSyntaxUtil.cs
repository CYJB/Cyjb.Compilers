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
		return model.GetArguments(attr);
	}
}
