using System.Text.RegularExpressions;
using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示 <see cref="LexerController{T}"/> 的子类定义。
/// </summary>
internal sealed partial class LexerController
{
	/// <summary>
	/// <see cref="LexerContextAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel ContextAttrModel = AttributeModel.FromType(typeof(LexerContextAttribute));
	/// <summary>
	/// <see cref="LexerInclusiveContextAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel InclusiveContextAttrModel = AttributeModel.FromType(typeof(LexerInclusiveContextAttribute));
	/// <summary>
	/// <see cref="LexerRegexAttribute"/> 的模型。
	/// </summary>
	private static readonly AttributeModel RegexAttrModel = AttributeModel.FromType(typeof(LexerRegexAttribute));

	/// <summary>
	/// 模板上下文。
	/// </summary>
	private readonly TransformationContext context;
	/// <summary>
	/// 控制器节点。
	/// </summary>
	private readonly ClassDeclarationSyntax controllerSyntax;
	/// <summary>
	/// 控制器的类型。
	/// </summary>
	private readonly TypeBuilder controllerType;
	/// <summary>
	/// 标识符类型。
	/// </summary>
	private readonly string kindType;
	/// <summary>
	/// 词法分析是否用到了 Reject 动作。
	/// </summary>
	private bool rejectable = false;
	/// <summary>
	/// 词法分析器。
	/// </summary>
	private readonly Lexer<int> lexer = new();
	/// <summary>
	/// 终结符信息。
	/// </summary>
	private readonly List<LexerSymbolAttrInfo> symbolInfos = new();
	/// <summary>
	/// 动作的映射。
	/// </summary>
	private readonly Dictionary<Delegate, string> actionMap = new();

	/// <summary>
	/// 使用指定的控制器类型和标识符类型初始化。
	/// </summary>
	/// <param name="context">模板上下文。</param>
	/// <param name="controllerSyntax">控制器节点。</param>
	/// <param name="kindType">标识符类型。</param>
	public LexerController(TransformationContext context, ClassDeclarationSyntax controllerSyntax, TypeSyntax kindType)
	{
		this.context = context;
		this.controllerSyntax = controllerSyntax;
		controllerType = SyntaxFactory.IdentifierName(controllerSyntax.Identifier.WithoutTrivia());
		this.kindType = kindType.ToString();
	}

	/// <summary>
	/// 获取控制器节点。
	/// </summary>
	public ClassDeclarationSyntax ControllerSyntax => controllerSyntax;

	/// <summary>
	/// 解析词法分析相关成员。
	/// </summary>
	public void Parse()
	{
		ParseClassAttributes();
		ParseActions();
		// 添加终结符定义
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
					this.context.AddError(Resources.IncompleteLexerContext, info.Syntax);
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
			var builder = lexer.DefineSymbol(regex, info.RegexOptions);
			if (contexts.Length > 0)
			{
				builder.Context(contexts);
			}
			if (info.Kind != null)
			{
				// Kind 本身是 ExpressionSyntax，这里临时符号索引代替，后续生成代码时再替换。
				builder.Kind(i);
			}
			if (info.MethodName != null)
			{
				void action(LexerController<int> c)
				{
					c.Text = info.MethodName;
				}
				actionMap[action] = info.MethodName;
				builder.Action(action);
			}
		}
	}

	/// <summary>
	/// 解析类特性。
	/// </summary>
	/// <returns>如果解析成功，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	private void ParseClassAttributes()
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
								context.AddError(Resources.InvalidLexerContext(label), labelExp);
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
								context.AddError(Resources.InvalidLexerContext(label), labelExp);
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
								context.AddError(Resources.InvalidLexerSymbol(attr, Resources.EmptyRegexName), attr);
								break;
							}
							string? regex = args["regex"]!.GetStringLiteral();
							if (regex.IsNullOrEmpty())
							{
								context.AddError(Resources.InvalidLexerSymbol(attr, Resources.EmptyRegex), attr);
								break;
							}
							RegexOptions regexOptions = RegexOptions.None;
							ExpressionSyntax? exp = args["options"];
							if (exp != null)
							{
								regexOptions = exp.GetEnumValue<RegexOptions>();
							}
							lexer.DefineRegex(name, regex, regexOptions);
							break;
						}
					case "LexerSymbolAttribute":
						{
							if (LexerSymbolAttrInfo.TryParse(context, attr, out var info))
							{
								symbolInfos.Add(info);
							}
							break;
						}
				}
			}
			catch (CSharpException ex)
			{
				context.AddError(ex.ToString(), ex.Location);
			}
		}
	}

	/// <summary>
	/// 解析终结符动作。
	/// </summary>
	private void ParseActions()
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
				if (!LexerSymbolAttrInfo.TryParse(context, attr, out var info))
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
						context.AddError(Resources.InvalidLexerSymbolAction(methodName), method);
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
	/// 生成词法分析器。
	/// </summary>
	/// <returns>词法分析器的类型定义。</returns>
	public ClassDeclarationSyntax Generate()
	{
		LexerData<int> data = lexer.GetData();
		SyntaxFormat format = new SyntaxFormat(controllerSyntax).IncDepth();
		// 工厂方法
		TypeBuilder factoryInterfaceType = $"ILexerFactory<{kindType}>";
		TypeBuilder factoryType = $"LexerFactory<{kindType}, {controllerSyntax.Identifier}>";
		var factoryMethod = SyntaxBuilder.MethodDeclaration(factoryInterfaceType, "CreateLexerFactory")
			.Comment("创建词法分析器的工厂。")
			.Attribute(SyntaxBuilder.Attribute("global::System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
			.Modifier(SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword)
			.Statement(SyntaxBuilder
				.LocalDeclarationStatement($"Dictionary<string, ContextData<{kindType}>>", "contexts")
				.Comment("上下文数据")
				.Value(ContextsValue(data))
			)
			.Statement(SyntaxBuilder.LocalDeclarationStatement($"TerminalData<{kindType}>[]", "terminals")
				.Comment("终结符数据")
				.Value(TerminalsValue(data, symbolInfos))
			)
			.Statement(SyntaxBuilder.LocalDeclarationStatement("int[]", "indexes")
				.Comment("字符类信息")
				.Comment(lexer.GetCharClassDescription())
				.Comment("字符类索引")
				.Value(CharClassIndexes(data))
			)
			.Statement(SyntaxBuilder.LocalDeclarationStatement("int[]", "classes")
				.Comment("字符类列表")
				.Value(CharClassClasses(data))
			);
		if (data.CharClasses.Categories != null)
		{
			factoryMethod.Statement(SyntaxBuilder
				.LocalDeclarationStatement("Dictionary<UnicodeCategory, int>", "categories")
				.Comment("字符类 Unicode 类别")
				.Value(CharClassCategories(data))
			);
		}
		factoryMethod
			.Statement(SyntaxBuilder.LocalDeclarationStatement("DfaStateData[]", "states")
				.Comment("状态转移")
				.Comment(lexer.GetStateDescription())
				.Comment("状态列表")
				.Value(StatesValue(data))
			)
			.Statement(SyntaxBuilder.LocalDeclarationStatement("int[]", "next")
				.Comment("后继状态列表")
				.Value(NextValue(data))
			)
			.Statement(SyntaxBuilder.LocalDeclarationStatement("int[]", "check")
				.Comment("状态检查列表")
				.Value(CheckValue(data))
			);

		var charClassBuilder = SyntaxBuilder.ObjectCreationExpression().Type("CharClassMap")
			.Argument(SyntaxBuilder.IdentifierName("indexes"))
			.Argument(SyntaxBuilder.IdentifierName("classes"));
		if (data.CharClasses.Categories != null)
		{
			charClassBuilder.Argument(SyntaxBuilder.IdentifierName("categories"));
		}
		factoryMethod.Statement(SyntaxBuilder.LocalDeclarationStatement($"LexerData<{kindType}>", "lexerData")
			.Comment("词法分析器的数据")
			.Value(SyntaxBuilder.ObjectCreationExpression().ArgumentWrap(1)
				.Argument(SyntaxBuilder.IdentifierName("contexts"))
				.Argument(SyntaxBuilder.IdentifierName("terminals"))
				.Argument(charClassBuilder)
				.Argument(SyntaxBuilder.IdentifierName("states"))
				.Argument(SyntaxBuilder.IdentifierName("next"))
				.Argument(SyntaxBuilder.IdentifierName("check"))
				.Argument(SyntaxBuilder.IdentifierName("TrailingType").Access(data.TrailingType.ToString()))
				.Argument(SyntaxBuilder.LiteralExpression(data.ContainsBeginningOfLine))
				.Argument(SyntaxBuilder.LiteralExpression(rejectable))
				.Argument(SyntaxBuilder.TypeOfExpression(controllerType))
			))
			.Statement(SyntaxBuilder.ReturnStatement(
				SyntaxBuilder.ObjectCreationExpression().Type(factoryType)
					.Argument(SyntaxBuilder.IdentifierName("lexerData"))));

		// 成员声明
		List<MemberDeclarationSyntax> members = new()
		{
			SyntaxBuilder.FieldDeclaration(factoryInterfaceType, "Factory")
				.Modifier(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword)
				.Comment("词法分析器的工厂。")
				.Value(SyntaxBuilder.IdentifierName("CreateLexerFactory").Invoke())
				.GetSyntax(format)
				.AddTrailingTrivia(format.EndOfLine),
			factoryMethod.GetSyntax(format),
		};

		// 将 BaseList 的 TrailingTrivia 添加到 openBraceToken 之前，避免丢失换行符。
		SyntaxToken openBraceToken = controllerSyntax.OpenBraceToken.InsertLeadingTrivia(0,
			controllerSyntax.BaseList!.GetTrailingTrivia());

		return SyntaxFactory.ClassDeclaration(
			SyntaxFactory.List<AttributeListSyntax>(),
			controllerSyntax.Modifiers,
			controllerSyntax.Keyword,
			controllerSyntax.Identifier,
			controllerSyntax.TypeParameterList,
			null,
			SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
			openBraceToken,
			SyntaxFactory.List(members),
			controllerSyntax.CloseBraceToken,
			controllerSyntax.SemicolonToken);
	}
}
