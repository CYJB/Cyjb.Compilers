using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 提供对 <see cref="NameSyntax"/> 的扩展方法。 
/// </summary>
internal static class NameSyntaxUtil
{
	/// <summary>
	/// 返回当前 <see cref="NameSyntax"/> 的 <see cref="SimpleNameSyntax"/>。
	/// </summary>
	/// <param name="name">要检查的 <see cref="NameSyntax"/> 对象。</param>
	/// <returns><see cref="SimpleNameSyntax"/> 本身，或者限定符最右侧的 <see cref="SimpleNameSyntax"/>。</returns>
	public static SimpleNameSyntax GetSimpleName(this NameSyntax name)
	{
		if (name is SimpleNameSyntax simpleName)
		{
			return simpleName;
		}
		else if (name is AliasQualifiedNameSyntax aliasQualifiedName)
		{
			return aliasQualifiedName.Name;
		}
		else if (name is QualifiedNameSyntax qualifiedName)
		{
			return qualifiedName.Right;
		}
		else
		{
			throw CommonExceptions.Unreachable();
		}
	}
}
