Cyjb.Compilers.Design
====

允许通过设计时 [T4 模板](https://docs.microsoft.com/zh-cn/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022)生成词法分析器和语法分析器的实现。

使用方式：

1. 通过 nuget 依赖运行时 [Cyjb.Compilers.Runtim](https://www.nuget.org/packages/Cyjb.Compilers.Runtime)。

2. 通过 nuget 依赖生成器 [Cyjb.Compilers.Design](https://www.nuget.org/packages/Cyjb.Compilers.Design)，注意请如下指定引用配置，可以正常编译项目并避免产生运行时引用。

```xml
<ItemGroup>
	<PackageReference Include="Cyjb.Compilers.Design" Version="1.0.0">
		<GeneratePathProperty>True</GeneratePathProperty>
		<PrivateAssets>all</PrivateAssets>
		<IncludeAssets>compile; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
	</PackageReference>
</ItemGroup>
```

3. 编写词法分析器或语法分析器的控制器类，例如：

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
public partial class TestCalcController : LexerController<Calc>
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

4. 添加与词法分析器同名的 tt 文件，内容如下：


```t4
<#@ include file="$(PkgCyjb_Compilers_Design)\content\CompilerTemplate.t4" #>
```

运行 T4 模板后即可生成同名的 `.lexer.cs` 文件，包含了词法分析器的实现。

欢迎访问我的[博客](http://www.cnblogs.com/cyjb/)获取更多信息。

C# 词法分析器系列博文

 - [C# 词法分析器（一）词法分析介绍](http://www.cnblogs.com/cyjb/archive/p/LexerIntroduce.html)
 - [C# 词法分析器（二）输入缓冲和代码定位](http://www.cnblogs.com/cyjb/archive/p/LexerInputBuffer.html)
 - [C# 词法分析器（三）正则表达式](http://www.cnblogs.com/cyjb/archive/p/LexerRegex.html)
 - [C# 词法分析器（四）构造 NFA](http://www.cnblogs.com/cyjb/archive/p/LexerNfa.html)
 - [C# 词法分析器（五）转换 DFA](http://www.cnblogs.com/cyjb/archive/p/LexerDfa.html)
 - [C# 词法分析器（六）构造词法分析器](http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html)
 - [C# 词法分析器（七）总结](http://www.cnblogs.com/cyjb/p/LexerSummary.html)
