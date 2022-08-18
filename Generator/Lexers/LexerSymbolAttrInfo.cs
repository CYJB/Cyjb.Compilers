using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析的终结符特性信息。
/// </summary>
internal class LexerSymbolAttrInfo
{
	/// <summary>
	/// <see cref="LexerSymbolAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel SymbolAttrModel = AttributeModel.FromType(typeof(LexerSymbolAttribute));

	/// <summary>
	/// 从指定的特性节点和终结符类型解析终结符信息。
	/// </summary>
	/// <param name="context">模板上下文。</param>
	/// <param name="attr">特性节点。</param>
	/// <param name="info">解析得到的 <see cref="LexerSymbolAttrInfo"/>。</param>
	/// <returns>解析是否成功。</returns>
	public static bool TryParse(TransformationContext context, AttributeSyntax attr, [NotNullWhen(true)] out LexerSymbolAttrInfo? info)
	{
		try
		{
			AttributeArguments args = attr.GetArguments(SymbolAttrModel);
			string? regex = args["regex"]!.GetStringLiteral();
			if (regex.IsNullOrEmpty())
			{
				context.AddError(Resources.InvalidLexerSymbol(attr, Resources.EmptyRegex), attr);
				info = null;
				return false;
			}
			info = new(attr, regex)
			{
				Kind = args["Kind"]
			};
			ExpressionSyntax? exp = args["options"];
			if (exp != null)
			{
				info.RegexOptions = exp.GetEnumValue<RegexOptions>();
			}
			return true;
		}
		catch (CSharpException ex)
		{
			context.AddError(ex.ToString(), ex.Location);
		}
		info = null;
		return false;
	}

	/// <summary>
	/// 使用指定的正则表达式初始化终结符信息。
	/// </summary>
	/// <param name="syntax">获取相关的特性信息。</param>
	/// <param name="regex">正则表达式。</param>
	private LexerSymbolAttrInfo(AttributeSyntax syntax, string regex)
	{
		Syntax = syntax;
		Regex = regex;
	}

	/// <summary>
	/// 获取相关的特性信息。
	/// </summary>
	public AttributeSyntax Syntax { get; }
	/// <summary>
	/// 获取正则表达式的内容。
	/// </summary>
	public string Regex { get; }
	/// <summary>
	/// 获取或设置正则表达式的选项。
	/// </summary>
	public RegexOptions RegexOptions { get; set; }
	/// <summary>
	/// 获取或设置正则表达式的类型。
	/// </summary>
	public ExpressionSyntax? Kind { get; set; }
	/// <summary>
	/// 获取或设置关联到的方法名称。
	/// </summary>
	public string? MethodName { get; set; }
}
