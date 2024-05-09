using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
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
	/// 生成的 LexerCore 类名。
	/// </summary>
	private const string LexerCoreClassName = "LexerCore";

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
						ParseContextAttribute(attr, false);
						break;
					case "LexerInclusiveContextAttribute":
						ParseContextAttribute(attr, true);
						break;
					case "LexerRegexAttribute":
						ParseRegexAttribute(attr);
						break;
					case "LexerSymbolAttribute":
						ParseSymbolAttribute(attr, null);
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
				switch (attr.GetFullName())
				{
					case "LexerRegexAttribute":
						ParseRegexAttribute(attr);
						break;
					case "LexerSymbolAttribute":
						ParseSymbolAttribute(attr, method);
						break;
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
		NameBuilder factoryInterfaceType = typeof(ILexerFactory<>).AsName().TypeArgument(KindType);
		// 工厂成员声明
		yield return SyntaxBuilder.DeclareField(factoryInterfaceType, "Factory")
				.Modifier(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword)
				.Comment("词法分析器的工厂。")
				.Attribute(typeof(CompilerGeneratedAttribute))
				.Value("CreateLexerFactory".AsName().Invoke())
				.GetSyntax(Format)
				.AddTrailingTrivia(Format.EndOfLine);

		// 工厂方法
		var factoryMethod = SyntaxBuilder.DeclareMethod(factoryInterfaceType, "CreateLexerFactory")
			.Comment("创建词法分析器的工厂。")
			.Attribute(typeof(CompilerGeneratedAttribute))
			.Modifier(SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword);
		LexerData<SymbolKind> data = lexer.GetData(rejectable);
		// 如果只包含默认上下文，那么不需要创建 contexts 变量。
		var contexts = GetContextDeclaration(data.Contexts);
		TypeBuilder terminalsType = typeof(TerminalData<>).AsName().TypeArgument(KindType).Array();
		var terminals = SyntaxBuilder.DeclareLocal(terminalsType, "terminals")
			.Comment("终结符数据")
			.Value(TerminalsValue(data.Terminals, lexer.TerminalMerge, symbolInfos));
		var indexes = SyntaxBuilder.DeclareLocal<uint[]>("indexes")
				.Comment("字符类信息")
				.Comment(lexer.GetCharClassDescription())
				.Comment("字符类索引")
				.Value(data.CharClasses.Indexes.AsLiteral(8));
		var classes = SyntaxBuilder.DeclareLocal<int[]>("classes")
			.Comment("字符类列表")
			.Value(data.CharClasses.CharClasses.AsLiteral(24));
		var categories = DeclareCharClassCategories(data);
		var charClassBuilder = ExpressionBuilder.CreateObject<CharClassMap>()
			.Argument(indexes).Argument(classes).Argument(categories);
		var states = SyntaxBuilder.DeclareLocal<int[]>("states")
			.Comment("状态转移")
			.Comment(lexer.GetStateDescription())
			.Comment("状态列表")
			.Value(data.States.AsLiteral(8));
		var trans = SyntaxBuilder.DeclareLocal<int[]>("trans")
			.Comment("状态转移")
			.Value(data.Trans.AsLiteral(12));

		TypeBuilder lexerDataType = typeof(LexerData<>).AsName().TypeArgument(KindType);
		var lexerData = SyntaxBuilder.DeclareLocal(lexerDataType, "lexerData")
			.Comment("词法分析器的数据")
			.Value(ExpressionBuilder.CreateObject().ArgumentWrap(1)
				.Argument(contexts)
				.Argument(terminals)
				.Argument(charClassBuilder)
				.Argument(states)
				.Argument(trans)
				.Argument("TrailingType".AsName().AccessMember(data.TrailingType.ToString()))
				.Argument(data.ContainsBeginningOfLine)
				.Argument(rejectable)
				.Argument(ExpressionBuilder.TypeOf(ControllerType)));

		NameBuilder factoryType = typeof(LexerFactory<,>).AsName()
			.TypeArgument(KindType).TypeArgument(ControllerType);

		factoryMethod
			.Statement(contexts)
			.Statement(terminals)
			.Statement(indexes)
			.Statement(classes)
			.Statement(categories)
			.Statement(states)
			.Statement(trans)
			.Statement(lexerData)
			.Statement(SyntaxBuilder.Return(
				ExpressionBuilder.CreateObject(factoryType).Argument(lexerData)));
		yield return factoryMethod.GetSyntax(Format);
	}

	/// <summary>
	/// 解析上下文特性。
	/// </summary>
	/// <param name="attr">要解析的特性。</param>
	/// <param name="inclusive">是否是包含型。</param>
	private void ParseContextAttribute(AttributeSyntax attr, bool inclusive)
	{
		AttributeArguments args = attr.GetArguments(inclusive ? InclusiveContextAttrModel : ContextAttrModel);
		ExpressionSyntax labelExp = args["label"]!;
		string? label = labelExp.GetStringLiteral();
		if (label.IsNullOrEmpty())
		{
			Context.AddError(Resources.InvalidLexerContext(label), labelExp);
			return;
		}
		if (inclusive)
		{
			lexer.DefineInclusiveContext(label);
		}
		else
		{
			lexer.DefineContext(label);
		}
	}

	/// <summary>
	/// 解析正则表达式特性。
	/// </summary>
	/// <param name="attr">要解析的特性。</param>
	private void ParseRegexAttribute(AttributeSyntax attr)
	{
		AttributeArguments args = attr.GetArguments(RegexAttrModel);
		string? name = args["name"]!.GetStringLiteral();
		if (name.IsNullOrEmpty())
		{
			Context.AddError(Resources.InvalidLexerSymbol(attr, Resources.EmptyRegexName), attr);
			return;
		}
		string? regex = args["regex"]!.GetStringLiteral();
		if (regex.IsNullOrEmpty())
		{
			Context.AddError(Resources.InvalidLexerSymbol(attr, Resources.EmptyRegex), attr);
			return;
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
	}

	/// <summary>
	/// 解析符号特性。
	/// </summary>
	/// <param name="attr">要解析的特性。</param>
	private void ParseSymbolAttribute(AttributeSyntax attr, MethodDeclarationSyntax? method)
	{
		if (!LexerSymbolAttrInfo.TryParse(Context, attr, out var info))
		{
			return;
		}
		if (method != null)
		{
			// 检查是否是可选或 params 参数
			string methodName = method.Identifier.Text;
			foreach (ParameterSyntax param in method.ParameterList.Parameters)
			{
				if (param.Default == null && !param.IsParamsArray())
				{
					Context.AddError(Resources.InvalidLexerSymbolAction(methodName), method);
					return;
				}
			}
			info.MethodName = methodName;
		}
		symbolInfos.Add(info);
	}

	/// <summary>
	/// 添加终结符定义。
	/// </summary>
	private void AddSymbols()
	{
		Dictionary<string, Action<LexerController<SymbolKind>>> methodMap = new();
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
					// 相同的方法使用相同的 Action，支持合并终结符。
					if (!methodMap.TryGetValue(info.MethodName, out Action<LexerController<SymbolKind>>? action))
					{
						action = (LexerController<SymbolKind> c) =>
						{
							c.Text = info.MethodName;
						};
						methodMap[info.MethodName] = action;
						actionMap[action] = info.MethodName;
					}
					builder.Action(action);
				}
			}
			catch (RegexParseException ex)
			{
				Context.AddError(ex.Message, info.Syntax);
			}
		}
	}

	/// <summary>
	/// 返回上下文数据的声明。
	/// </summary>
	/// <param name="Contexts">上下文数据。</param>
	/// <returns>上下文数据的声明。</returns>
	private LocalDeclarationStatementBuilder? GetContextDeclaration(IReadOnlyDictionary<string, ContextData> contexts)
	{
		if (contexts.Count == 1 && contexts.Values.All((context) => context.EofAction == null))
		{
			// 只有默认上下文且没有 Eof 动作，那么不需要生成上下文数据。
			return null;
		}
		var contextsValue = ExpressionBuilder.CreateObject().InitializerWrap(1);
		// 按照索引顺序生成上下文
		foreach (var pair in contexts.OrderBy(pair => pair.Value.Index))
		{
			var contextBuilder = ExpressionBuilder.CreateObject<ContextData>()
				.Argument(pair.Value.Index)
				.Argument(GetContextKey(pair.Value.Label));
			if (pair.Value.EofAction != null)
			{
				contextBuilder.Argument(ExpressionBuilder.Lambda()
					.Parameter("c", Name)
					.Body("c".AsName().AccessMember(actionMap[pair.Value.EofAction]).Invoke())
				);
			}
			contextsValue.Initializer(ExpressionBuilder.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression)
				.Add(GetContextKey(pair.Key))
				.Add(contextBuilder));
		}
		return SyntaxBuilder.DeclareLocal<Dictionary<string, ContextData>>("contexts")
			.Comment("上下文数据")
			.Value(contextsValue);
	}
}
