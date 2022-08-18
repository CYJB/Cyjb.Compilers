Cyjb.Compilers
====

提供编译相关功能，基于 .NET 6。

本项目包括一些与编译相关的功能，目前包含词法分析器和 LALR 语法分析器。

## 定义词法分析器

可以通过运行时定义正则表达式来创建词法分析器，例如下面简单的构造一个数学算式的词法分析器：

```CSharp
enum Calc { Id, Add, Sub, Mul, Div, Pow, LBrace, RBrace }
Lexer<Calc> lexer = new Lexer<Calc>();
// 终结符的定义。
lexer.DefineSymbol("[0-9]+").Kind(Calc.Id).Action(c => c.Accept(int.Parse(c.Text)));
lexer.DefineSymbol("\\+").Kind(Calc.Add);
lexer.DefineSymbol("\\-").Kind(Calc.Sub);
lexer.DefineSymbol("\\*").Kind(Calc.Mul);
lexer.DefineSymbol("\\/").Kind(Calc.Div);
lexer.DefineSymbol("\\^").Kind(Calc.Pow);
lexer.DefineSymbol("\\(").Kind(Calc.LBrace);
lexer.DefineSymbol("\\)").Kind(Calc.RBrace);
// 吃掉所有空白。
lexer.DefineSymbol("\\s");
ILexerFactory<Calc> factory = lexer.GetFactory();

// 要分析的源文件。
string source = "1 + 20 * 3 / 4*(5+6)";
TokenReader<Calc> reader = factory.CreateReader(source);
// 构造词法分析器。
foreach (Token<Calc> token in reader)
{
	Console.WriteLine(token);
}
```

还可以通过设计时定义词法分析控制器来创建词法分析器，例如下面构造一个与上面相同的的词法分析器：

```CSharp
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

通过 nuget 引入 [Cyjb.Compilers.Design](https://www.nuget.org/packages/Cyjb.Compilers.Design/) 时，并指定 `GeneratePathProperty="true"`。

```xml
<ItemGroup>
	<PackageReference Include="Cyjb.Compilers.Design" Version="1.0.0" GeneratePathProperty="true" />
</ItemGroup>
```

添加与词法分析器同名的 tt 文件，内容如下：


```t4
<#@ include file="$(PkgCyjb_Compilers_Design)\content\CompilerTemplate.t4" #>
```

运行 T4 模板后即可生成同名的 `.lexer.cs` 文件，包含了词法分析器的实现。

具体用法可以参考 TestCompilers/Lexers 目录下的示例。

详细的类库文档，请参见 [Wiki](https://github.com/CYJB/Cyjb.Compilers/wiki)。

欢迎访问我的[博客](http://www.cnblogs.com/cyjb/)获取更多信息。

C# 词法分析器系列博文

 - [C# 词法分析器（一）词法分析介绍](http://www.cnblogs.com/cyjb/archive/p/LexerIntroduce.html)
 - [C# 词法分析器（二）输入缓冲和代码定位](http://www.cnblogs.com/cyjb/archive/p/LexerInputBuffer.html)
 - [C# 词法分析器（三）正则表达式](http://www.cnblogs.com/cyjb/archive/p/LexerRegex.html)
 - [C# 词法分析器（四）构造 NFA](http://www.cnblogs.com/cyjb/archive/p/LexerNfa.html)
 - [C# 词法分析器（五）转换 DFA](http://www.cnblogs.com/cyjb/archive/p/LexerDfa.html)
 - [C# 词法分析器（六）构造词法分析器](http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html)
 - [C# 词法分析器（七）总结](http://www.cnblogs.com/cyjb/p/LexerSummary.html)
