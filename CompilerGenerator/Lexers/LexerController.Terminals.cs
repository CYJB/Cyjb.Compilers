using Cyjb.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Lexers;

internal sealed partial class LexerController
{
	/// <summary>
	/// 返回指定词法分析器数据的终结符数据。
	/// </summary>
	/// <param name="data">词法分析器数据</param>
	/// <returns>终结符数据。</returns>
	public ExpressionBuilder TerminalsValue(LexerData<int> data)
	{
		TypeBuilder terminalType = $"TerminalData<{kindType}>";
		var builder = SyntaxBuilder.ArrayCreationExpression().InitializerWrap(1);
		foreach (TerminalData<int> terminal in data.Terminals)
		{
			var terminalBuilder = SyntaxBuilder.ObjectCreationExpression().Type(terminalType);
			builder.Initializer(terminalBuilder);
			bool argContinues = true;
			if (terminal.Kind == null)
			{
				argContinues = false;
			}
			else
			{
				terminalBuilder.Argument(symbolInfos[terminal.Kind.Value].Kind!);
			}
			if (terminal.Action == null)
			{
				argContinues = false;
			}
			else
			{
				var action = SyntaxBuilder.LambdaExpression()
					.Parameter("c", controllerType)
					.Body(SyntaxBuilder.IdentifierName("c").Access(actionMap[terminal.Action]).Invoke());
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
				var trailing = SyntaxBuilder.LiteralExpression(terminal.Trailing.Value);
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
