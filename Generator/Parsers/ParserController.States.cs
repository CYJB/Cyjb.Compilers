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
		NameBuilder stateType = SyntaxBuilder.Name(typeof(ParserStateData<>)).TypeArgument(KindType);
		Dictionary<IReadOnlyDictionary<SymbolKind, ParserAction>, LocalDeclarationStatementBuilder> actions =
			new(DictionaryEqualityComparer<SymbolKind, ParserAction>.Default);
		Dictionary<IReadOnlySet<SymbolKind>, LocalDeclarationStatementBuilder> expectings =
			new(SetEqualityComparer<SymbolKind>.Default);
		int actionIndex = 1;
		int expectingIndex = 1;
		NameBuilder actionsType = SyntaxBuilder.Name(typeof(Dictionary<,>))
			.TypeArgument(KindType).TypeArgument<ParserAction>();
		NameBuilder expectingType = SyntaxBuilder.Name(typeof(HashSet<>)).TypeArgument(KindType);
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
			method.Statement(SyntaxBuilder.Name("states").AccessElement(SyntaxBuilder.Literal(i))
				.Assign(SyntaxKind.SimpleAssignmentExpression,
					SyntaxBuilder.CreateObject(stateType).ArgumentWrap(1)
						.Argument(actionDecl)
						.Argument(GetAction(state.DefaultAction))
						.Argument(expectingDecl)
						.Argument(GetProduction(data, i))
						.Argument(SyntaxBuilder.Literal(state.RecoverIndex))
						.Argument(SyntaxBuilder.Literal(state.FollowBaseIndex))
				)
				.Statement().Comment(comment));
		}
	}

	/// <summary>
	/// 返回创建指定动作字典的表达式。
	/// </summary>
	/// <param name="actions">动作字典。</param>
	/// <returns>创建指定动作字典的表达式。</returns>
	private static ExpressionBuilder GetActions(IReadOnlyDictionary<SymbolKind, ParserAction> actions)
	{
		var builder = SyntaxBuilder.CreateObject().InitializerWrap(1);
		foreach (var (key, value) in actions)
		{
			var pair = SyntaxBuilder.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression)
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
				.AccessMember("Shift").Invoke(SyntaxBuilder.Literal(action.Index)),
			ParserActionType.Reduce => SyntaxBuilder.Type<ParserAction>()
				.AccessMember("Reduce").Invoke(SyntaxBuilder.Literal(action.Index)),
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
		var builder = SyntaxBuilder.CreateObject().InitializerWrap(1);
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
		return SyntaxBuilder.Name("productions").AccessElement(SyntaxBuilder.Literal(index));
	}
}

