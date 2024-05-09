using System.Runtime.CompilerServices;
using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 <see cref="ParserController{T}"/> 的子类定义。
/// </summary>
internal sealed partial class ParserController : Controller
{
	/// <summary>
	/// <see cref="ParserNonAssociateAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel ParserNonAssociateAttrModel = AttributeModel.From<ParserNonAssociateAttribute>();
	/// <summary>
	/// <see cref="ParserLeftAssociateAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel ParserLeftAssociateAttrModel = AttributeModel.From<ParserLeftAssociateAttribute>();
	/// <summary>
	/// <see cref="ParserRightAssociateAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel ParserRightAssociateAttrModel = AttributeModel.From<ParserRightAssociateAttribute>();
	/// <summary>
	/// <see cref="ParserStartAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel ParserStartAttrModel = AttributeModel.From<ParserStartAttribute>();
	/// <summary>
	/// <see cref="ParserProductionAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel ParserProductionAttrModel = AttributeModel.From<ParserProductionAttribute>();

	/// <summary>
	/// 语法分析器。
	/// </summary>
	private readonly Parser<SymbolKind> parser = new();
	/// <summary>
	/// 动作的映射。
	/// </summary>
	private readonly Dictionary<Delegate, string> actionMap = new();

	/// <summary>
	/// 使用指定的控制器名称和标识符类型初始化 <see cref="ParserController"/> 类的新实例。
	/// </summary>
	/// <param name="context">模板上下文。</param>
	/// <param name="syntax">控制器语法节点。</param>
	/// <param name="kindType">标识符类型。</param>
	public ParserController(TransformationContext context, ClassDeclarationSyntax syntax, string kindType)
		: base(context, syntax, kindType)
	{ }

	/// <summary>
	/// 解析控制器语法节点。
	/// </summary>
	/// <param name="controllerSyntax">控制器语法节点。</param>
	public override void Parse(ClassDeclarationSyntax controllerSyntax)
	{
		ParseClassAttributes(controllerSyntax);
		ParseActions(controllerSyntax);
	}

	/// <summary>
	/// 解析类特性。
	/// </summary>
	/// <param name="controllerSyntax">控制器语法节点。</param>
	/// <returns>如果解析成功，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	private void ParseClassAttributes(ClassDeclarationSyntax controllerSyntax)
	{
		foreach (AttributeSyntax attr in controllerSyntax.AttributeLists.GetAttributes())
		{
			try
			{
				switch (attr.GetFullName())
				{
					case "ParserNonAssociateAttribute":
						DefineAssociativity(AssociativeType.NonAssociate,
							attr.GetArguments(ParserNonAssociateAttrModel).ParamsArgument);
						break;
					case "ParserLeftAssociateAttribute":
						DefineAssociativity(AssociativeType.Left,
							attr.GetArguments(ParserLeftAssociateAttrModel).ParamsArgument);
						break;
					case "ParserRightAssociateAttribute":
						DefineAssociativity(AssociativeType.Right,
							attr.GetArguments(ParserRightAssociateAttrModel).ParamsArgument);
						break;
					case "ParserStartAttribute":
						{
							AttributeArguments args = attr.GetArguments(ParserStartAttrModel);
							ExpressionSyntax? exp = args["kind"];
							if (exp == null)
							{
								Context.AddError(Resources.NoParserStart, attr);
							}
							else
							{
								SymbolKind kind = SymbolKind.GetKind(exp);
								exp = args["option"];
								ParseOptions option = ParseOptions.ScanToEOF;
								if (exp != null)
								{
									option = exp.GetEnumValue<ParseOptions>();
								}
								parser.AddStart(kind, option);
							}
						}
						break;
					case "ParserProductionAttribute":
						BuildProduction(attr);
						break;
				}
			}
			catch (CSharpException ex)
			{
				Context.AddError(ex.ToString(), ex.Location);
			}
		}
	}

	/// <summary>
	/// 解析产生式动作。
	/// </summary>
	/// <param name="controllerSyntax">控制器语法节点。</param>
	private void ParseActions(ClassDeclarationSyntax controllerSyntax)
	{
		foreach (MemberDeclarationSyntax member in controllerSyntax.Members)
		{
			if (member is not MethodDeclarationSyntax method)
			{
				continue;
			}
			foreach (AttributeSyntax attr in member.AttributeLists.GetAttributes())
			{
				if (attr.GetFullName() != "ParserProductionAttribute")
				{
					continue;
				}
				var builder = BuildProduction(attr);
				if (builder != null)
				{
					string methodName = method.Identifier.Text;
					object? action(ParserController<SymbolKind> c)
					{
						return methodName;
					}
					actionMap[action] = methodName;
					builder.Action(action);
				}
			}
		}
	}

	/// <summary>
	/// 定义指定结合性的终结符集合。
	/// </summary>
	/// <param name="type">终结符集合的结合性。</param>
	/// <param name="symbols">具有相同结合性的终结符的集合。</param>
	private void DefineAssociativity(AssociativeType type, ExpressionSyntax[] symbols)
	{
		if (symbols.Length == 0)
		{
			return;
		}
		parser.DefineAssociativity(type, symbols.Select((syntax) => SymbolKind.GetKind(syntax)).ToArray());
	}

	/// <summary>
	/// 通过指定的 <see cref="ParserProductionAttribute"/> 构造产生式。
	/// </summary>
	/// <param name="attr">特性的语法节点。</param>
	/// <returns>产生式构造器。</returns>
	private ProductionBuilder<SymbolKind, ParserController<SymbolKind>>? BuildProduction(AttributeSyntax attr)
	{
		AttributeArguments args = attr.GetArguments(ParserProductionAttrModel);
		ExpressionSyntax? exp = args["kind"];
		if (exp == null)
		{
			Context.AddError(Resources.NoParserProductionHead, attr);
			return null;
		}
		else
		{
			SymbolKind kind = SymbolKind.GetKind(exp);
			Variant<SymbolKind, SymbolOptions>[] body = args.ParamsArgument.Select((arg) =>
			{
				if (arg.TryGetEnumValue(out SymbolOptions option))
				{
					return new Variant<SymbolKind, SymbolOptions>(option);
				}
				else
				{
					return new Variant<SymbolKind, SymbolOptions>(SymbolKind.GetKind(arg));
				}
			}).ToArray();
			var builder = parser.DefineProduction(kind, body);
			exp = args["Prec"];
			if (exp != null)
			{
				builder.Prec(SymbolKind.GetKind(exp));
			}
			return builder;
		}
	}

	/// <summary>
	/// 生成控制器的成员。
	/// </summary>
	/// <returns>控制器的成员。</returns>
	public override IEnumerable<MemberDeclarationSyntax> Generate()
	{
		ParserData<SymbolKind> data = parser.GetData();
		TypeBuilder factoryInterfaceType = typeof(IParserFactory<>).AsName()
			.TypeArgument(KindType);
		// 工厂成员声明
		yield return SyntaxBuilder.DeclareField(factoryInterfaceType, "Factory")
				.Modifier(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword)
				.Comment("语法分析器的工厂。")
				.Attribute(typeof(CompilerGeneratedAttribute))
				.Value("CreateParserFactory".AsName().Invoke())
				.GetSyntax(Format)
				.AddTrailingTrivia(Format.EndOfLine);

		// 工厂方法
		var factoryMethod = SyntaxBuilder.DeclareMethod(factoryInterfaceType, "CreateParserFactory")
			.Comment("创建语法分析器的工厂。")
			.Attribute(typeof(CompilerGeneratedAttribute))
			.Modifier(SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword);
		// 如果只包含一个起始符号，那么不需要创建 startStates 变量。
		bool hasStartState = data.StartStates?.Count > 1;
		LocalDeclarationStatementBuilder? startStates = null;
		if (hasStartState)
		{
			TypeBuilder startStatesType = typeof(Dictionary<,>).AsName()
				.TypeArgument(KindType).TypeArgument(typeof(int));
			startStates = SyntaxBuilder.DeclareLocal(startStatesType, "startStates")
				.Comment("上下文数据")
				.Value(KindMap(data.StartStates!));
			factoryMethod.Statement(startStates);
		}
		// 符号声明，索引小于 -1 的是临时符号。
		factoryMethod.Statement(SyntaxBuilder.DeclareLocal(KindType, "endOfFile")
			.Value(((ExpressionBuilder)(-1)).Parenthesized().Cast(KindType))
			.Comment("临时符号"));
		foreach (SymbolKind symbol in GetTempKinds(data))
		{
			StatementBuilder builder = SyntaxBuilder.DeclareLocal(KindType, symbol.ToString())
				.Value(((ExpressionBuilder)symbol.Index).Parenthesized().Cast(KindType));
			factoryMethod.Statement(builder);
		}
		var productions = DeclareProductions(data);
		NameBuilder stateType = typeof(ParserStateData<>).AsName().TypeArgument(KindType);
		var states = SyntaxBuilder.DeclareLocal(stateType.Clone().Array(), "states")
			.Comment("状态数据")
			.Value(ExpressionBuilder.CreateArray(stateType).Rank(data.States.Length));
		factoryMethod
			.Statement(productions)
			.Statement(states);
		FillStates(data, factoryMethod);

		var gotoMap = SyntaxBuilder.DeclareLocal<int[]>("gotoMap")
			.Comment("GOTO 表的起始索引")
			.Value(data.GotoMap.AsLiteral(24));
		var gotoTrans = SyntaxBuilder.DeclareLocal<int[]>("gotoTrans")
			.Comment("GOTO 表的转移")
			.Value(data.GotoTrans.AsLiteral(24));

		var parserDataType = typeof(ParserData<>).AsName().TypeArgument(KindType);
		var parserData = SyntaxBuilder.DeclareLocal(parserDataType, "parserData")
			.Comment("语法分析器的数据")
			.Value(ExpressionBuilder.CreateObject().ArgumentWrap(1)
				.Argument(productions)
				.Argument(startStates)
				.Argument(states)
				.Argument(gotoMap)
				.Argument(gotoTrans)
			);

		var factoryType = typeof(ParserFactory<,>).AsName()
			.TypeArgument(KindType).TypeArgument(ControllerType);

		yield return factoryMethod
			.Statement(gotoMap)
			.Statement(gotoTrans)
			.Statement(parserData)
			.Statement(SyntaxBuilder.Return(
				ExpressionBuilder.CreateObject(factoryType).Argument(parserData)))
			.GetSyntax(Format);
	}

	/// <summary>
	/// 获取所有临时符号定义。
	/// </summary>
	/// <param name="data">语法分析数据。</param>
	/// <returns>所有临时符号定义。</returns>
	private static IEnumerable<SymbolKind> GetTempKinds(ParserData<SymbolKind> data)
	{
		HashSet<SymbolKind> kinds = new();
		foreach (ProductionData<SymbolKind> production in data.Productions)
		{
			kinds.Add(production.Head);
			kinds.UnionWith(production.Body);
		}
		return kinds.Where((kind) => kind.Index < -1).OrderBy((kind) => -kind.Index);
	}
}

