Cyjb.Compilers.Design
====

[![](https://img.shields.io/nuget/v/Cyjb.Compilers.Design.svg)](https://www.nuget.org/packages/Cyjb.Compilers.Design)

允许通过设计时 [T4 模板](https://docs.microsoft.com/zh-cn/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022)生成词法分析器和语法分析器的实现。

## 使用方式

1. 通过 nuget 依赖运行时 [Cyjb.Compilers.Runtime](https://www.nuget.org/packages/Cyjb.Compilers.Runtime)。

2. 通过 nuget 依赖生成器 [Cyjb.Compilers.Design](https://www.nuget.org/packages/Cyjb.Compilers.Design)，注意请如下指定引用配置，可以正常编译项目并避免产生运行时引用。

```xml
<ItemGroup>
	<PackageReference Include="Cyjb.Compilers.Design" Version="1.0.5">
		<GeneratePathProperty>True</GeneratePathProperty>
		<PrivateAssets>all</PrivateAssets>
		<IncludeAssets>compile; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
	</PackageReference>
</ItemGroup>
```

3. 编写词法分析器的控制器类，例如：

```CSharp
using Cyjb.Compilers.Lexers;

enum Calc { Id, Add, Sub, Mul, Div, Pow, LBrace, RBrace }

[LexerSymbol("\\+", Kind = Calc.Add)]
[LexerSymbol("\\-", Kind = Calc.Sub)]
[LexerSymbol("\\*", Kind = Calc.Mul)]
[LexerSymbol("\\/", Kind = Calc.Div)]
[LexerSymbol("\\^", Kind = Calc.Pow)]
[LexerSymbol("\\(", Kind = Calc.LBrace)]
[LexerSymbol("\\)", Kind = Calc.RBrace)]
[LexerSymbol("\\s")]
// 必须是部分类，且继承自 LexerController<Calc>
public partial class CalcLexer : LexerController<Calc>
{
	/// <summary>
	/// 数字的终结符定义。
	/// </summary>
	[LexerSymbol("[0-9]+", Kind = Calc.Id)]
	public void DigitAction()
	{
		Value = int.Parse(Text);
		Accept();
	}
}
```

或者语法分析器的控制器类，例如：

```CSharp
using Cyjb.Compilers.Parsers;

[ParserLeftAssociate(Calc.Add, Calc.Sub)]
[ParserLeftAssociate(Calc.Mul, Calc.Div)]
[ParserRightAssociate(Calc.Pow)]
[ParserNonAssociate(Calc.Id)]
// 必须是部分类，且继承自 ParserController<Calc>
public partial class CalcParser : ParserController<Calc>
{
	[ParserProduction(Calc.E, Calc.Id)]
	private object? IdAction()
	{
		return this[0].Value;
	}

	[ParserProduction(Calc.E, Calc.E, Calc.Add, Calc.E)]
	[ParserProduction(Calc.E, Calc.E, Calc.Sub, Calc.E)]
	[ParserProduction(Calc.E, Calc.E, Calc.Mul, Calc.E)]
	[ParserProduction(Calc.E, Calc.E, Calc.Div, Calc.E)]
	[ParserProduction(Calc.E, Calc.E, Calc.Pow, Calc.E)]
	private object? BinaryAction()
	{
		double left = (double)this[0].Value!;
		double right = (double)this[2].Value!;
		return this[1].Kind switch
		{
			Calc.Add => left + right,
			Calc.Sub => left - right,
			Calc.Mul => left * right,
			Calc.Div => left / right,
			Calc.Pow => Math.Pow(left, right),
			_ => throw CommonExceptions.Unreachable(),
		};
	}

	[ParserProduction(Calc.E, Calc.LBrace, Calc.E, Calc.RBrace)]
	private object? BraceAction()
	{
		return this[1].Value;
	}
}
```

4. 添加与词法分析器同名的 tt 文件，内容如下：

```t4
<#@ include file="$(PkgCyjb_Compilers_Design)\content\CompilerTemplate.t4" #>
```

运行 T4 模板后即可生成同名的 `.designed.cs` 文件，包含了词法或语法分析器的实现。

通过 `CalcLexer.Factory` 或 `CalcParser.Factory` 即可访问创建词法/语法分析器的工厂类。

## 词法分析功能

词法分析器使用 [LexerSymbol](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/Lexers/LexerSymbolAttribute.cs) 特性声明终结符，使用的正则表达式的定义与 [C# 正则表达式](https://docs.microsoft.com/zh-cn/dotnet/standard/base-types/regular-expression-language-quick-reference)一致，但不包含定位点、捕获、Lookaround、反向引用、替换构造和替代功能。

正则表达式支持通过 `/` 指定向前看符号，支持指定匹配的上下文，并在执行动作时根据需要切换上下文。

如果前缀可以与多个正则表达式匹配，那么：

1. 总是选择最长的前缀。
2. 如果最长的可能前缀与多个正则表达式匹配，总是选择先定义的正则表达式。

支持使用 [LexerContext](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/Lexers/LexerContextAttribute.cs) 或 [LexerInclusiveContext](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/Lexers/LexerInclusiveContextAttribute.cs) 特性声明上下文或包含型上下文，在声明符号时可以通过 `<ContextName>foo` 指定生效的上下文。

支持使用 [LexerRegex](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/Lexers/LexerRegexAttribute.cs) 特性声明公共正则表达式，在声明符号时可以通过 `foo{RegexName}bar` 引用公共正则表达式。

支持使用 [LexerRejectable](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/Lexers/LexerRejectableAttribute.cs) 特性开启 Reject 动作的支持。

## 语法分析功能

语法分析器使用 LALR 语法分析实现，使用 [ParserProduction](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/Parsers/ParserProductionAttribute.cs) 特性声明产生式，并且可以通过 [SymbolOption](https://github.com/CYJB/Cyjb.Compilers/blob/master/Runtime/Parsers/SymbolOption.cs) 的 `Optional`、`ZeroOrMore` 和 `OneOrMore` 支持简单的子产生式，其功能类似于正则表达式中的 `A?`、`A*` 和 `A+`。

支持使用 [ParserNonAssociate](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/Parsers/ParserNonAssociateAttribute.cs)、[ParserLeftAssociate](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/Parsers/ParserLeftAssociateAttribute.cs) 和 [ParserRightAssociate](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/Parsers/ParserRightAssociateAttribute.cs) 声明符号的结合性。

默认使用首个出现的非终结符作为起始符号，也支持使用 [ParserStart](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/Parsers/ParserStartAttribute.cs) 指定起始符号。起始符号可以指定多个，并通过在语法分析器的 `Parse` 方法的参数来选择希望使用的起始符号。

还可以使用 [ParseOption](https://github.com/CYJB/Cyjb.Compilers/blob/master/Runtime/Parsers/ParseOption.cs) 指定起始符号的扫描方式。

欢迎访问我的[博客](http://www.cnblogs.com/cyjb/)获取更多信息。

C# 词法分析器系列博文

 - [C# 词法分析器（一）词法分析介绍](http://www.cnblogs.com/cyjb/archive/p/LexerIntroduce.html)
 - [C# 词法分析器（二）输入缓冲和代码定位](http://www.cnblogs.com/cyjb/archive/p/LexerInputBuffer.html)
 - [C# 词法分析器（三）正则表达式](http://www.cnblogs.com/cyjb/archive/p/LexerRegex.html)
 - [C# 词法分析器（四）构造 NFA](http://www.cnblogs.com/cyjb/archive/p/LexerNfa.html)
 - [C# 词法分析器（五）转换 DFA](http://www.cnblogs.com/cyjb/archive/p/LexerDfa.html)
 - [C# 词法分析器（六）构造词法分析器](http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html)
 - [C# 词法分析器（七）总结](http://www.cnblogs.com/cyjb/p/LexerSummary.html)

 C# 语法法分析器系列博文

- [C# 语法分析器（一）语法分析介绍](https://www.cnblogs.com/cyjb/p/ParserIntroduce.html)
- [C# 语法分析器（二）LR(0) 语法分析](https://www.cnblogs.com/cyjb/p/ParserLR_0.html)
- [C# 语法分析器（三）LALR 语法分析](https://www.cnblogs.com/cyjb/p/ParserLALR.html)
- [C# 语法分析器（四）二义性文法](https://www.cnblogs.com/cyjb/p/ParserAmbiguous.html)
- [C# 语法分析器（五）错误恢复](https://www.cnblogs.com/cyjb/p/ParserErrorRecovery.html)
- [C# 语法分析器（六）构造语法分析器](https://www.cnblogs.com/cyjb/p/ParserCreate.html)

