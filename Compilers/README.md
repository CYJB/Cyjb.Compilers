Cyjb.Compilers
====

[![](https://img.shields.io/nuget/v/Cyjb.Compilers.svg)](https://www.nuget.org/packages/Cyjb.Compilers)

提供编译相关功能，基于 .NET 6。

本项目包括一些与编译相关的功能，目前包含词法分析器和 LALR 语法分析器。

## 支持的正则表达式用法

| 正则表达式 | 描述 |
|---|---|
| `x` | 单个字符 x |
| `.` | 除了换行以外的任意单个字符 |
| `[xyz]` | 一个字符类，表示 'x'，'y'，'z' 中的任意一个字符 |
| `[a-z]` | 一个字符类，表示 'a' 到 'z' 之间的任意一个字符（包含 'a' 和 'z'） |
| `[^a-z]` | 一个字符类，表示除了 [a-z] 之外的任意一个字符 |
| `[a-z-[b-f]]` | 一个字符类，表示 [a-z] 范围减去 [b-f] 范围的字符，等价于 [ag-z] |
| `r*` | 将任意正则表达式 r 重复 0 次或多次 |
| `r+` | 将 r 重复 1 次或多次 |
| `r?` | 将 r 重复 0 次或 1 次，即“可选”的 r |
| `r{m,n}` | 将 r 重复 m 次至 n 次（包含 m 和 n） |
| `r{m,}` | 将 r 重复 m 次或多次（大于等于 m 次） |
| `r{m}` | 将 r 重复恰好 m 次 |
| `{name}` | 展开预先定义的正则表达式 “name”，可以通过预先定义一些正则表达式，以实现简化正则表达式 |
| `"[xyz]\"foo"` | 原义字符串，表示字符串“[xyz]"foo”，用法与 C# 中定义字符串基本相同 |
| `\X` | 表示 X 字符转义，如果 X 是 'a','b','t','r','v','f','n' 或 'e'，表示相应的 ASCII 字符；如果 X 是 'w','W','s','S','d' 或 'D'，则表示相应的字符类；否则表示字符 X |
| `\nnn` | 表示使用八进制形式指定的字符，nnn 最多由三位数字组成 |
| `\xnn` | 表示使用十六进制形式指定的字符，nn 恰好由两位数字组成 |
| `\cX` | 表示 X 指定的 ASCII 控制字符 |
| `\unnnn` | 表示使用十六进制形式指定的 Unicode 字符，nnnn 恰好由四位数字组成 |
| `\p{name}` | 表示 name 指定的 Unicode 通用类别或命名块中的单个字符 |
| `\P{name}` | 表示除了 name 指定的 Unicode 通用类别或命名块之外的单个字符 |
| `(r)` | 表示 r 本身 |
| `(?r-s:pattern)` | 应用或禁用子正则表达式中指定的选项。选项可以是字符 'i','s' 或 'x'。<br/><br/>'i' 表示不区分大小写；'-i' 表示区分大小写。<br/>'s' 表示允许 '.' 匹配换行符；'-s' 表示不允许 '.' 匹配换行符。<br/>'x' 表示忽略模式中的空白和注释，除非使用 '\' 字符转义或者在字符类中，或者使用双引号（""） 括起来；'-x' 表示不忽略空白。 |
| `(?#comment)` | 表示注释，注释中不允许出现右括号 ')' |
| `rs` | r 与 s 的连接 |
| `r|s` | r 与 s 的并 |
| `r/s` | 仅当 r 后面跟着 s 时，才匹配 r。这里 '/' 表示向前看，s 并不会被匹配 |
| `{EOF}` | 在**向前看表达式内**匹配文件结束 |
| `^r` | 行首限定符，仅当 r 在一行的开头时才匹配 |
| `r$` | 行尾限定符，仅当 r 在一行的结尾时才匹配。这里的行尾可以是 '\n'、'\r\n' 或者 '\r' |
| `<s>r` | 仅当当前是上下文 s 时才匹配 r |
| `<s1,s2>r` | 仅当当前是上下文 s1 或 s2 时才匹配 r |
| `<*>r` | 在任意上下文中匹配 r |
| `<<EOF>>` | 表示在文件的结尾 |
| `<s1,s2><<EOF>>` | 表示在上下文 s1 或 s2 时的文件的结尾 |

## 定义词法分析器

可以通过正则表达式来创建词法分析器，例如下面简单的构造一个数学算式的词法分析器：

```CSharp
enum Calc { Id, Add, Sub, Mul, Div, Pow, LBrace, RBrace }
Lexer<Calc> lexer = new Lexer<Calc>();
// 终结符的定义。
lexer.DefineSymbol("[0-9]+").Kind(Calc.Id).Action(c => c.Accept(double.Parse(c.Text)));
lexer.DefineSymbol("\\+").Kind(Calc.Add);
lexer.DefineSymbol("\\-").Kind(Calc.Sub);
lexer.DefineSymbol("\\*").Kind(Calc.Mul);
lexer.DefineSymbol("\\/").Kind(Calc.Div);
lexer.DefineSymbol("\\^").Kind(Calc.Pow);
lexer.DefineSymbol("\\(").Kind(Calc.LBrace);
lexer.DefineSymbol("\\)").Kind(Calc.RBrace);
// 吃掉所有空白。
lexer.DefineSymbol("\\s");
ILexerFactory<Calc> lexerFactory = lexer.GetFactory();

// 要分析的源文件。
string source = "1 + 20 * 3 / 4*(5+6)";
ITokenizer<Calc> tokenizer = lexerFactory.CreateTokenizer(source);
// 构造词法分析器。
foreach (Token<Calc> token in tokenizer)
{
	Console.WriteLine(token);
}
```

词法分析器使用 `DefineSymbol` 方法定义终结符，使用的正则表达式的定义与 [C# 正则表达式](https://docs.microsoft.com/zh-cn/dotnet/standard/base-types/regular-expression-language-quick-reference)一致，但不包含定位点、捕获、Lookaround、反向引用、替换构造和替代功能。

正则表达式支持通过 `/` 指定向前看符号，支持指定匹配的上下文，并在执行动作时根据需要切换上下文。

如果前缀可以与多个正则表达式匹配，那么：

1. 总是选择最长的前缀。
2. 如果最长的可能前缀与多个正则表达式匹配，总是选择先定义的正则表达式。

支持使用 `DefineContext` 或 `DefineInclusiveContext` 方法定义上下文或包含型上下文，在声明符号时可以通过构造器的 `Context` 方法指定生效的上下文。

支持使用 `DefineRegex` 方法定义公共正则表达式，在声明符号时可以通过 `foo{RegexName}bar` 引用公共正则表达式。

支持在调用 `GetFactory` 方法生成工厂时，传入 `rejectable` 参数开启 Reject 动作的支持。

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
public partial class CalcLexer : LexerController<Calc>
{
	/// <summary>
	/// 数字的终结符定义。
	/// </summary>
	[LexerSymbol("[0-9]+", Kind = Calc.Id)]
	public void DigitAction()
	{
		Accept(double.Parse(Text));
	}
}
```

设计时词法分析器的使用方法请参见 [Cyjb.Compilers.Design](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/README.md)。

具体用法可以参考 [TestCompilers/Lexers](https://github.com/CYJB/Cyjb.Compilers/tree/master/TestCompilers/Lexers) 目录下的示例。

## 定义语法分析器

可以通过语法产生式来创建词法分析器，例如下面简单的构造一个数学算式的语法分析器：

```CSharp
// 非终结符的定义。
Parser<Calc> parser = new();
// 定义产生式
parser.DefineProduction(Calc.E, Calc.Id).Action(c => c[0].Value);
parser.DefineProduction(Calc.E, Calc.E, Calc.Add, Calc.E)
	.Action(c => (double) c[0].Value! + (double) c[2].Value!);
parser.DefineProduction(Calc.E, Calc.E, Calc.Sub, Calc.E)
	.Action(c => (double) c[0].Value! - (double) c[2].Value!);
parser.DefineProduction(Calc.E, Calc.E, Calc.Mul, Calc.E)
	.Action(c => (double) c[0].Value! * (double) c[2].Value!);
parser.DefineProduction(Calc.E, Calc.E, Calc.Div, Calc.E)
	.Action(c => (double) c[0].Value! / (double) c[2].Value!);
parser.DefineProduction(Calc.E, Calc.E, Calc.Pow, Calc.E)
	.Action(c => Math.Pow((double) c[0].Value!, (double) c[2].Value!));
parser.DefineProduction(Calc.E, Calc.LBrace, Calc.E, Calc.RBrace)
	.Action(c => c[1].Value);
// 定义运算符优先级。
parser.DefineAssociativity(AssociativeType.Left, Calc.Add, Calc.Sub);
parser.DefineAssociativity(AssociativeType.Left, Calc.Mul, Calc.Div);
parser.DefineAssociativity(AssociativeType.Right, Calc.Pow);
parser.DefineAssociativity(AssociativeType.NonAssociate, Calc.Id);
IParserFactory<Calc> parserFactory = parser.GetFactory();
// 解析词法单元序列。
string source = "1 + 20 * 3 / 4*(5+6)";
ITokenParser<Calc> parser = parserFactory.CreateParser(lexerFactory.CreateTokenizer(source));
Console.WriteLine(parser.Parse().Value);
// 输出 166.0
```

语法分析器使用 LALR 语法分析实现，使用 `DefineProduction` 方法定义产生式，并且可以通过 [SymbolOption](https://github.com/CYJB/Cyjb.Compilers/blob/master/Runtime/Parsers/SymbolOption.cs) 的 `Optional`、`ZeroOrMore` 和 `OneOrMore` 支持简单的子产生式，其功能类似于正则表达式中的 `A?`、`A*` 和 `A+`。

支持使用 `DefineAssociativity` 方法定义符号的结合性。

默认使用首个出现的非终结符作为起始符号，也支持使用 `AddStart` 方法指定起始符号。起始符号可以指定多个，并通过在语法分析器的 `Parse` 方法的参数来选择希望使用的起始符号。

还可以使用 [ParseOption](https://github.com/CYJB/Cyjb.Compilers/blob/master/Runtime/Parsers/ParseOption.cs) 指定起始符号的扫描方式。

还可以通过设计时定义语法分析控制器来创建语法分析器，例如下面构造一个与上面相同的的语法分析器：

```CSharp
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

设计时语法分析器的使用方法请参见 [Cyjb.Compilers.Design](https://github.com/CYJB/Cyjb.Compilers/blob/master/Design/README.md)。

具体用法可以参考 [TestCompilers/Parsers](https://github.com/CYJB/Cyjb.Compilers/tree/master/TestCompilers/Parsers) 目录下的示例。

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

C# 语法法分析器系列博文

- [C# 语法分析器（一）语法分析介绍](https://www.cnblogs.com/cyjb/p/ParserIntroduce.html)
- [C# 语法分析器（二）LR(0) 语法分析](https://www.cnblogs.com/cyjb/p/ParserLR_0.html)
- [C# 语法分析器（三）LALR 语法分析](https://www.cnblogs.com/cyjb/p/ParserLALR.html)
- [C# 语法分析器（四）二义性文法](https://www.cnblogs.com/cyjb/p/ParserAmbiguous.html)
- [C# 语法分析器（五）错误恢复](https://www.cnblogs.com/cyjb/p/ParserErrorRecovery.html)
- [C# 语法分析器（六）构造语法分析器](https://www.cnblogs.com/cyjb/p/ParserCreate.html)

