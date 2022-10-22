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
								ParseOption option = ParseOption.ScanToEOF;
								if (exp != null)
								{
									option = exp.GetEnumValue<ParseOption>();
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
			Variant<string, SymbolKind>[] body = args.ParamsArgument.Select((arg) =>
			{
				if (arg.TryGetStringLiteral(out string? text))
				{
					return new Variant<string, SymbolKind>(text);
				}
				else
				{
					return new Variant<string, SymbolKind>(SymbolKind.GetKind(arg));
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
		TypeBuilder factoryInterfaceType = SyntaxBuilder.Name("IParserFactory").TypeArgument(KindType);
		// 工厂成员声明
		yield return SyntaxBuilder.DeclareField(factoryInterfaceType, "Factory")
				.Modifier(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword)
				.Comment("语法分析器的工厂。")
				.Value(SyntaxBuilder.Name("CreateParserFactory").Invoke())
				.GetSyntax(Format)
				.AddTrailingTrivia(Format.EndOfLine);

		// 工厂方法
		var factoryMethod = SyntaxBuilder.DeclareMethod(factoryInterfaceType, "CreateParserFactory")
			.Comment("创建语法分析器的工厂。")
			.Attribute(SyntaxBuilder.Attribute("System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
			.Modifier(SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword);
		// 如果只包含一个起始符号，那么不需要创建 startStates 变量。
		bool hasStartState = data.StartStates?.Count > 1;
		if (hasStartState)
		{
			factoryMethod.Statement(SyntaxBuilder
				.DeclareLocal($"Dictionary<{KindType}, int>", "startStates")
				.Comment("上下文数据")
				.Value(KindMap(data.StartStates!))
			);
		}
		// 符号声明，索引小于 -1 的是临时符号。
		factoryMethod.Statement(SyntaxBuilder.DeclareLocal(KindType, "endOfFile")
			.Value(SyntaxBuilder.Literal(-1).Parenthesized().Cast(KindType))
			.Comment("临时符号"));
		foreach (SymbolKind symbol in GetTempKinds(data))
		{
			StatementBuilder builder = SyntaxBuilder.DeclareLocal(KindType, symbol.ToString())
				.Value(SyntaxBuilder.Literal(symbol.Index).Parenthesized().Cast(KindType));
			factoryMethod.Statement(builder);
		}
		factoryMethod
			.Statement(SyntaxBuilder.DeclareLocal($"ProductionData<{KindType}>[]", "productions")
				.Comment("产生式数据")
				.Value(Productions(data, actionMap)))
			.Statement(SyntaxBuilder.DeclareLocal($"ParserStateData<{KindType}>[]", "states")
				.Comment("状态数据")
				.Value(SyntaxBuilder.CreateArray()
					.Type(SyntaxBuilder.Type($"ParserStateData<{KindType}>"))
					.Rank(SyntaxBuilder.Literal(data.States.Length))));
		FillStates(data, factoryMethod);
		factoryMethod
			.Statement(SyntaxBuilder.DeclareLocal($"Dictionary<{KindType}, int>", "gotoMap")
				.Comment("转移数据")
				.Value(KindMap(data.GotoMap)))
			.Statement(SyntaxBuilder.DeclareLocal<int[]>("gotoNext")
				.Comment("转移的目标")
				.Value(SyntaxBuilder.LiteralArray(data.GotoNext, 24)))
			.Statement(SyntaxBuilder.DeclareLocal($"{KindType}[]", "gotoCheck")
				.Comment("转移的检查")
				.Value(GotoCheck(data.GotoCheck)))
			.Statement(SyntaxBuilder.DeclareLocal<int[]>("followNext")
				.Comment("后继状态的目标")
				.Value(SyntaxBuilder.LiteralArray(data.FollowNext, 24)))
			.Statement(SyntaxBuilder.DeclareLocal<int[]>("followCheck")
				.Comment("后继状态的检查")
				.Value(SyntaxBuilder.LiteralArray(data.FollowCheck, 24)))
			.Statement(SyntaxBuilder.DeclareLocal($"ParserData<{KindType}>", "parserData")
				.Comment("语法分析器的数据")
				.Value(SyntaxBuilder.CreateObject().ArgumentWrap(1)
					.Argument(SyntaxBuilder.Name("productions"))
					.Argument(hasStartState ? SyntaxBuilder.Name("startStates") : SyntaxBuilder.Literal(null))
					.Argument(SyntaxBuilder.Name("states"))
					.Argument(SyntaxBuilder.Name("gotoMap"))
					.Argument(SyntaxBuilder.Name("gotoNext"))
					.Argument(SyntaxBuilder.Name("gotoCheck"))
					.Argument(SyntaxBuilder.Name("followNext"))
					.Argument(SyntaxBuilder.Name("followCheck"))
				))
			.Statement(SyntaxBuilder.Return(
				SyntaxBuilder.CreateObject().Type($"ParserFactory<{KindType}, {Name}>")
					.Argument(SyntaxBuilder.Name("parserData"))));
		yield return factoryMethod.GetSyntax(Format);
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

