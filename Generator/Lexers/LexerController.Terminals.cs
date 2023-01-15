using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.Lexers;

internal sealed partial class LexerController
{
	/// <summary>
	/// 返回指定词法分析器数据的终结符数据。
	/// </summary>
	/// <param name="data">词法分析器数据。</param>
	/// <param name="terminalMerge">终结符的合并信息。</param>
	/// <param name="symbols">符号信息列表。</param>
	/// <returns>终结符数据。</returns>
	private ExpressionBuilder TerminalsValue(LexerData<SymbolKind> data,
		Dictionary<int, List<int>> terminalMerge, List<LexerSymbolAttrInfo> symbols)
	{
		TypeBuilder terminalType = SyntaxBuilder.Name(typeof(TerminalData<>)).TypeArgument(KindType);
		var builder = SyntaxBuilder.CreateArray().InitializerWrap(1);
		for (int i = 0; i < data.Terminals.Length; i++)
		{
			TerminalData<SymbolKind> terminal = data.Terminals[i];
			var terminalBuilder = SyntaxBuilder.CreateObject(terminalType);
			builder.Initializer(terminalBuilder);
			AddSymbolComment(terminalBuilder, i, terminalMerge, symbols);
			bool argContinues = true;
			if (terminal.Kind == null)
			{
				argContinues = false;
			}
			else
			{
				terminalBuilder.Argument(terminal.Kind.Value.Syntax);
			}
			if (terminal.Value is ExpressionSyntax value)
			{
				if (argContinues)
				{
					terminalBuilder.Argument(value);
				}
				else
				{
					terminalBuilder.Argument(value, "value");
				}
			}
			else
			{
				argContinues = false;
			}
			if (terminal.Action == null)
			{
				argContinues = false;
			}
			else
			{
				var action = SyntaxBuilder.Lambda()
					.Parameter("c", SyntaxBuilder.Name(Name))
					.Body(SyntaxBuilder.Name("c").AccessMember(actionMap[terminal.Action]).Invoke());
				if (argContinues)
				{
					terminalBuilder.Argument(action);
				}
				else
				{
					terminalBuilder.Argument(action, "action");
				}
			}
			if (terminal.Trailing == null)
			{
				argContinues = false;
			}
			else
			{
				var trailing = SyntaxBuilder.Literal(terminal.Trailing.Value);
				if (argContinues)
				{
					terminalBuilder.Argument(trailing);
				}
				else
				{
					terminalBuilder.Argument(trailing, "trailing");
				}
			}
			if (terminal.UseShortest)
			{
				if (argContinues)
				{
					terminalBuilder.Argument(SyntaxBuilder.Literal(true));
				}
				else
				{
					terminalBuilder.Argument(SyntaxBuilder.Literal(true), "useShortest");
				}
			}
		}
		return builder;
	}

	/// <summary>
	/// 添加符号注释。
	/// </summary>
	/// <param name="builder">对象创建表达式的构造器。</param>
	/// <param name="index">符号索引。</param>
	/// <param name="terminalMerge">终结符的合并信息。</param>
	/// <param name="symbols">符号信息列表。</param>
	private static void AddSymbolComment(ObjectCreationExpressionBuilder builder, int index,
		Dictionary<int, List<int>> terminalMerge, List<LexerSymbolAttrInfo> symbols)
	{
		bool firstSymbol = true;
		string idxText = $"{index}: ";
		foreach (int j in terminalMerge[index])
		{
			if (firstSymbol)
			{
				builder.Comment(idxText + symbols[j].Regex);
				firstSymbol = false;
			}
			else
			{
				builder.Comment(new string(' ', idxText.Length) + symbols[j].Regex);
			}
		}
	}
}
