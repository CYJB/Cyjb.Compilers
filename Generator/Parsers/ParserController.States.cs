using Cyjb.CodeAnalysis.CSharp;
using Cyjb.Collections;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.Compilers.Parsers;

internal sealed partial class ParserController
{
	/// <summary>
	/// 添加语法分析状态数据。
	/// </summary>
	/// <param name="data">语法分析器数据</param>
	/// <param name="method">方法声明。</param>
	private void FillStates(ParserData<SymbolKind> data, MethodDeclarationBuilder method)
	{
		NameBuilder stateType = typeof(ParserStateData<>).AsName().TypeArgument(KindType);
		Dictionary<IReadOnlyDictionary<SymbolKind, ParserAction>, LocalDeclarationStatementBuilder> actions =
			new(DictionaryEqualityComparer<SymbolKind, ParserAction>.Default);
		Dictionary<IReadOnlySet<SymbolKind>, LocalDeclarationStatementBuilder> expectings =
			new(SetEqualityComparer<SymbolKind>.Default);
		int actionIndex = 1;
		int expectingIndex = 1;
		NameBuilder actionsType = typeof(Dictionary<,>).AsName()
			.TypeArgument(KindType).TypeArgument<ParserAction>();
		NameBuilder expectingType = typeof(HashSet<>).AsName().TypeArgument(KindType);
		for (int i = 0; i < data.States.Length; i++)
		{
			ParserStateData<SymbolKind> state = data.States[i];
			string? comment = parser.GetStateDescription(i);
			if (!actions.TryGetValue(state.Actions, out LocalDeclarationStatementBuilder? actionDecl))
			{
				string name = $"action_{actionIndex++}";
				actionDecl = SyntaxBuilder.DeclareLocal(actionsType, name)
					.Value(GetActions(state.Actions)).Comment(comment);
				actions[state.Actions] = actionDecl;
				method.Statement(actionDecl);
				comment = null;
			}
			if (!expectings.TryGetValue(state.Expecting, out LocalDeclarationStatementBuilder? expectingDecl))
			{
				string name = $"expecting_{expectingIndex++}";
				expectingDecl = SyntaxBuilder.DeclareLocal(expectingType, name)
					.Value(GetExpecting(state.Expecting)).Comment(comment);
				expectings[state.Expecting] = expectingDecl;
				method.Statement(expectingDecl);
				comment = null;
			}
			method.Statement("states".AsName().AccessElement(i)
				.Assign(ExpressionBuilder.CreateObject(stateType).ArgumentWrap(1)
					.Argument(actionDecl)
					.Argument(GetAction(state.DefaultAction))
					.Argument(expectingDecl)
					.Argument(GetProduction(data, i))
					.Argument(state.RecoverIndex)
				)
				.AsStatement().Comment(comment));
		}
	}

	/// <summary>
	/// 返回创建指定动作字典的表达式。
	/// </summary>
	/// <param name="actions">动作字典。</param>
	/// <returns>创建指定动作字典的表达式。</returns>
	private static ExpressionBuilder GetActions(IReadOnlyDictionary<SymbolKind, ParserAction> actions)
	{
		var builder = ExpressionBuilder.CreateObject().InitializerWrap(1);
		foreach (var (key, value) in actions)
		{
			var pair = ExpressionBuilder.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression)
				.Add(key.Syntax).Add(GetAction(value));
			builder.Initializer(pair);
		}
		return builder;
	}

	/// <summary>
	/// 返回创建指定动作的表达式。
	/// </summary>
	/// <param name="action">动作。</param>
	/// <returns>创建指定动作的表达式。</returns>
	private static ExpressionBuilder GetAction(ParserAction action)
	{
		return action.Type switch
		{
			ParserActionType.Accept => SyntaxBuilder.Type<ParserAction>().AccessMember("Accept"),
			ParserActionType.Shift => SyntaxBuilder.Type<ParserAction>()
				.AccessMember("Shift").Invoke(action.Index),
			ParserActionType.Reduce => SyntaxBuilder.Type<ParserAction>()
				.AccessMember("Reduce").Invoke(action.Index),
			_ => SyntaxBuilder.Type<ParserAction>().AccessMember("Error"),
		};
	}

	/// <summary>
	/// 返回创建指定预期词法单元集合的表达式。
	/// </summary>
	/// <param name="expecting">预期词法单元集合。</param>
	/// <returns>创建指定预期词法单元集合的表达式。</returns>
	private static ExpressionBuilder GetExpecting(IReadOnlySet<SymbolKind> expecting)
	{
		var builder = ExpressionBuilder.CreateObject().InitializerWrap(1);
		foreach (SymbolKind kind in expecting)
		{
			builder.Initializer(kind.Syntax);
		}
		return builder;
	}

	/// <summary>
	/// 返回指定状态恢复产生式的表达式。
	/// </summary>
	/// <param name="data">语法分析器数据</param>
	/// <param name="stateIndex">状态索引。</param>
	/// <returns>指定状态恢复产生式的表达式。</returns>
	private static ExpressionBuilder GetProduction(ParserData<SymbolKind> data, int stateIndex)
	{
		int index = Array.IndexOf(data.Productions, data.States[stateIndex].RecoverProduction);
		return "productions".AsName().AccessElement(index);
	}
}

