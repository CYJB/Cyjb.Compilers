using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 提供对 <see cref="ParameterSyntax"/> 的扩展方法。 
/// </summary>
internal static class ParameterSyntaxUtil
{
	/// <summary>
	/// 返回当前 <see cref="ParameterSyntax"/> 是否是 params 参数。
	/// </summary>
	/// <param name="param">要检查的 <see cref="ParameterSyntax"/> 对象。</param>
	/// <returns>如果 <paramref name="param"/> 是 params 参数，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public static bool IsParamsArray(this ParameterSyntax param)
	{
		foreach (var modifier in param.Modifiers)
		{
			if (modifier.IsKind(SyntaxKind.ParamsKeyword))
			{
				return true;
			}
		}
		return false;
	}
}
