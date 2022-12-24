using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示 <see cref="LexerController{T}"/> 的子类定义。
/// </summary>
internal sealed partial class LexerController : Controller
{
	/// <summary>
	/// <see cref="LexerContextAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel ContextAttrModel = AttributeModel.From<LexerContextAttribute>();
	/// <summary>
	/// <see cref="LexerInclusiveContextAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel InclusiveContextAttrModel = AttributeModel.From<LexerInclusiveContextAttribute>();
	/// <summary>
	/// <see cref="LexerRegexAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel RegexAttrModel = AttributeModel.From<LexerRegexAttribute>();

	/// <summary>
	/// 词法分析是否用到了 Reject 动作。
	/// </summary>
	private bool rejectable = false;
	/// <summary>
	/// 词法分析器。
	/// </summary>
	private readonly Lexer<SymbolKind> lexer = new();
	/// <summary>
	/// 终结符信息。
	/// </summary>
	private readonly List<LexerSymbolAttrInfo> symbolInfos = new();
	/// <summary>
	/// 动作的映射。
	/// </summary>
	private readonly Dictionary<Delegate, string> actionMap = new();

	/// <summary>
	/// 使用指定的控制器名称和标识符类型初始化 <see cref="LexerController"/> 类的新实例。
	/// </summary>
	/// <param name="context">模板上下文。</param>
	/// <param name="syntax">控制器语法节点。</param>
	/// <param name="kindType">标识符类型。</param>
	public LexerController(TransformationContext context, ClassDeclarationSyntax syntax, string kindType)
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
					case "LexerRejectableAttribute":
						rejectable = true;
						break;
					case "LexerContextAttribute":
						{
							AttributeArguments args = attr.GetArguments(ContextAttrModel);
							ExpressionSyntax labelExp = args["label"]!;
							string? label = labelExp.GetStringLiteral();
							if (label.IsNullOrEmpty())
							{
								Context.AddError(Resources.InvalidLexerContext(label), labelExp);
							}
							else
							{
								lexer.DefineContext(label);
							}
							break;
						}
					case "LexerInclusiveContextAttribute":
						{
							AttributeArguments args = attr.GetArguments(InclusiveContextAttrModel);
							ExpressionSyntax labelExp = args["label"]!;
							string? label = labelExp.GetStringLiteral();
							if (label.IsNullOrEmpty())
							{
								Context.AddError(Resources.InvalidLexerContext(label), labelExp);
							}
							else
							{
								lexer.DefineInclusiveContext(label);
							}
							break;
						}
					case "LexerRegexAttribute":
						{
							AttributeArguments args = attr.GetArguments(RegexAttrModel);
							string? name = args["name"]!.GetStringLiteral();
							if (name.IsNullOrEmpty())
							{
								Context.AddError(Resources.InvalidLexerSymbol(attr, Resources.EmptyRegexName), attr);
								break;
							}
							string? regex = args["regex"]!.GetStringLiteral();
							if (regex.IsNullOrEmpty())
							{
								Context.AddError(Resources.InvalidLexerSymbol(attr, Resources.EmptyRegex), attr);
								break;
							}
							RegexOptions regexOptions = RegexOptions.None;
							ExpressionSyntax? exp = args["options"];
							if (exp != null)
							{
								regexOptions = exp.GetEnumValue<RegexOptions>();
							}
							try
							{
								lexer.DefineRegex(name, regex, regexOptions);
							}
							catch (RegexParseException ex)
							{
								Context.AddError(Resources.InvalidRegex(regex, ex.Message), attr);
							}
							break;
						}
					case "LexerSymbolAttribute":
						{
							if (LexerSymbolAttrInfo.TryParse(Context, attr, out var info))
							{
								symbolInfos.Add(info);
							}
							break;
						}
				}
			}
			catch (CSharpException ex)
			{
				Context.AddError(ex.ToString(), ex.Location);
			}
		}
	}

	/// <summary>
	/// 解析终结符动作。
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
				if (attr.GetFullName() != "LexerSymbolAttribute")
				{
					continue;
				}
				if (!LexerSymbolAttrInfo.TryParse(Context, attr, out var info))
				{
					continue;
				}
				// 检查是否是可选或 params 参数
				string methodName = method.Identifier.Text;
				bool isValidAction = true;
				foreach (ParameterSyntax param in method.ParameterList.Parameters)
				{
					if (param.Default == null && !param.IsParamsArray())
					{
						isValidAction = false;
						Context.AddError(Resources.InvalidLexerSymbolAction(methodName), method);
						break;
					}
				}
				if (isValidAction)
				{
					info.MethodName = methodName;
					symbolInfos.Add(info);
				}
			}
		}
	}

	/// <summary>
	/// 生成控制器的成员。
	/// </summary>
	/// <returns>控制器的成员。</returns>
	public override IEnumerable<MemberDeclarationSyntax> Generate()
	{
		AddSymbols();
		LexerData<SymbolKind> data = lexer.GetData(rejectable);
		NameBuilder factoryInterfaceType = SyntaxBuilder.Name(typeof(ILexerFactory<>))
			.TypeArgument(KindType);
		// 工厂成员声明
		yield return SyntaxBuilder.DeclareField(factoryInterfaceType, "Factory")
				.Modifier(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword)
				.Comment("词法分析器的工厂。")
				.Value(SyntaxBuilder.Name("CreateLexerFactory").Invoke())
				.GetSyntax(Format)
				.AddTrailingTrivia(Format.EndOfLine);

		// 工厂方法
		var factoryMethod = SyntaxBuilder.DeclareMethod(factoryInterfaceType, "CreateLexerFactory")
			.Comment("创建词法分析器的工厂。")
			.Attribute(SyntaxBuilder.Attribute<CompilerGeneratedAttribute>())
			.Modifier(SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword);
		// 如果只包含默认上下文，那么不需要创建 contexts 变量。
		LocalDeclarationStatementBuilder? contexts = null;
		if (data.Contexts.Count > 1)
		{
			contexts = SyntaxBuilder.DeclareLocal<Dictionary<string, ContextData>>("contexts")
				.Comment("上下文数据")
				.Value(ContextsValue(data));
		}
		TypeBuilder terminalsType = SyntaxBuilder.Name(typeof(TerminalData<>)).TypeArgument(KindType).Array();
		var terminals = SyntaxBuilder.DeclareLocal(terminalsType, "terminals")
			.Comment("终结符数据")
			.Value(TerminalsValue(data, symbolInfos));
		var indexes = SyntaxBuilder.DeclareLocal<uint[]>("indexes")
				.Comment("字符类信息")
				.Comment(lexer.GetCharClassDescription())
				.Comment("字符类索引")
				.Value(SyntaxBuilder.Literal(data.CharClasses.Indexes, 8));
		var classes = SyntaxBuilder.DeclareLocal<int[]>("classes")
			.Comment("字符类列表")
			.Value(SyntaxBuilder.Literal(data.CharClasses.CharClasses, 24));
		var categories = DeclareCharClassCategories(data);
		var charClassBuilder = SyntaxBuilder.CreateObject<CharClassMap>()
			.Argument(indexes).Argument(classes).Argument(categories);
		var states = SyntaxBuilder.DeclareLocal<DfaStateData[]>("states")
			.Comment("状态转移")
			.Comment(lexer.GetStateDescription())
			.Comment("状态列表")
			.Value(StatesValue(data));
		var next = SyntaxBuilder.DeclareLocal<int[]>("next")
			.Comment("后继状态列表")
			.Value(SyntaxBuilder.Literal(data.Next, 24));
		var check = SyntaxBuilder.DeclareLocal<int[]>("check")
			.Comment("状态检查列表")
			.Value(SyntaxBuilder.Literal(data.Check, 24));

		TypeBuilder lexerDataType = SyntaxBuilder.Name(typeof(LexerData<>)).TypeArgument(KindType);
		var lexerData = SyntaxBuilder.DeclareLocal(lexerDataType, "lexerData")
			.Comment("词法分析器的数据")
			.Value(SyntaxBuilder.CreateObject().ArgumentWrap(1)
				.Argument(contexts)
				.Argument(terminals)
				.Argument(charClassBuilder)
				.Argument(states)
				.Argument(next)
				.Argument(check)
				.Argument(SyntaxBuilder.Name("TrailingType").AccessMember(data.TrailingType.ToString()))
				.Argument(SyntaxBuilder.Literal(data.ContainsBeginningOfLine))
				.Argument(SyntaxBuilder.Literal(rejectable))
				.Argument(SyntaxBuilder.TypeOf(ControllerType)));

		NameBuilder factoryType = SyntaxBuilder.Name(typeof(LexerFactory<,>))
			.TypeArgument(KindType).TypeArgument(ControllerType);

		yield return factoryMethod
			.Statement(contexts)
			.Statement(terminals)
			.Statement(indexes)
			.Statement(classes)
			.Statement(categories)
			.Statement(states)
			.Statement(next)
			.Statement(check)
			.Statement(lexerData)
			.Statement(SyntaxBuilder.Return(
				SyntaxBuilder.CreateObject(factoryType).Argument(lexerData)))
			.GetSyntax(Format);
	}

	/// <summary>
	/// 添加终结符定义。
	/// </summary>
	private void AddSymbols()
	{
		for (int i = 0; i < symbolInfos.Count; i++)
		{
			LexerSymbolAttrInfo info = symbolInfos[i];
			string regex = info.Regex;
			// 解析 symbol 开头的 Context
			string[] contexts = Array.Empty<string>();
			if (regex[0] == '<' && !regex.StartsWith("<<EOF>>"))
			{
				// 处理上下文。
				int idx = regex.IndexOf('>');
				if (idx == -1)
				{
					Context.AddError(Resources.IncompleteLexerContext, info.Syntax);
					continue;
				}
				string context = regex[1..idx];
				regex = regex[(idx + 1)..];
				if (context == "*")
				{
					contexts = lexer.Contexts;
				}
				else
				{
					contexts = new string(context).Split(',');
				}
			}
			try
			{
				var builder = lexer.DefineSymbol(regex, info.RegexOptions);
				if (contexts.Length > 0)
				{
					builder.Context(contexts);
				}
				if (info.Kind != null)
				{
					// Kind 本身是 ExpressionSyntax，这里临时符号索引代替，后续生成代码时再替换。
					builder.Kind(info.Kind.Value);
				}
				builder.Value(info.Value);
				if (info.UseShortest)
				{
					builder.UseShortest();
				}
				if (info.MethodName != null)
				{
					void action(LexerController<SymbolKind> c)
					{
						c.Text = info.MethodName;
					}
					actionMap[action] = info.MethodName;
					builder.Action(action);
				}
			}
			catch (RegexParseException ex)
			{
				Context.AddError(ex.Message, info.Syntax);
			}
		}
	}
}
