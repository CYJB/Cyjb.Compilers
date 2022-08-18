using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析的终结符特性信息。
/// </summary>
internal class LexerSymbolAttrInfo
{
	/// <summary>
	/// 从指定的 <see cref="CustomAttributeData"/> 解析终结符特性信息。
	/// </summary>
	/// <param name="data">特性数据。</param>
	/// <param name="kindType">终结符类型。</param>
	/// <param name="info">解析得到的 <see cref="LexerSymbolAttrInfo"/>。</param>
	/// <returns>如果解析成功，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	/// <exception cref="CompileException">未能正确解析特性。</exception>
	public static bool TryGetInfo(CustomAttributeData data, Type kindType, [NotNullWhen(true)] out LexerSymbolAttrInfo? info)
	{
		string? regex = data.ConstructorArguments[0].Value as string;
		if (string.IsNullOrEmpty(regex))
		{
			info = null;
			return false;
		}
		RegexOptions regexOptions = GenericConvert.ChangeType<object?, RegexOptions>(data.ConstructorArguments[1].Value);
		info = new(regex, regexOptions);
		foreach (CustomAttributeNamedArgument arg in data.NamedArguments)
		{
			switch (arg.MemberName)
			{
				case "Kind":
					if (!kindType.IsExplicitFrom(arg.TypedValue.ArgumentType))
					{
						throw new CompileException(Resources.InvalidLexerSymbol(data, Resources.InvalidSymbolKind));
					}
					info.Kind = GenericConvert.ChangeType(arg.TypedValue.Value, arg.TypedValue.ArgumentType);
					break;
			}
		}
		return true;
	}

	/// <summary>
	/// 使用指定的正则表达式初始化终结符信息。
	/// </summary>
	/// <param name="regex">正则表达式。</param>
	/// <param name="regexOptions">正则表达式的选项。</param>
	private LexerSymbolAttrInfo(string regex, RegexOptions regexOptions)
	{
		Regex = regex;
		RegexOptions = regexOptions;
	}

	/// <summary>
	/// 获取正则表达式的内容。
	/// </summary>
	public string Regex { get; }
	/// <summary>
	/// 获取正则表达式的选项。
	/// </summary>
	public RegexOptions RegexOptions { get; }
	/// <summary>
	/// 获取或设置正则表达式的类型。
	/// </summary>
	public object? Kind { get; set; }
	/// <summary>
	/// 获取或设置关联到的方法信息。
	/// </summary>
	public MethodInfo? Method { get; set; }
}
