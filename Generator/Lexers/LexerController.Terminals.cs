using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.Lexers;

internal sealed partial class LexerController
{
	/// <summary>
	/// 返回指定词法分析器数据的终结符数据。
	/// </summary>
	/// <param name="data">词法分析器数据。</param>
	/// <param name="symbols">符号信息列表。</param>
	/// <returns>终结符数据。</returns>
	private ExpressionBuilder TerminalsValue(LexerData<SymbolKind> data, List<LexerSymbolAttrInfo> symbols)
	{
		TypeBuilder terminalType = SyntaxBuilder.Name(typeof(TerminalData<>)).TypeArgument(KindType);
		var builder = SyntaxBuilder.CreateArray().InitializerWrap(1);
		for (int i = 0; i < symbols.Count; i++)
		{
			TerminalData<SymbolKind> terminal = data.Terminals[i];
			var terminalBuilder = SyntaxBuilder.CreateObject(terminalType);
			builder.Initializer(terminalBuilder);
			terminalBuilder.Comment($"{i}: {symbols[i].Regex}");
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
			if (terminal.Trailing != null)
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
		}
		return builder;
	}
}
