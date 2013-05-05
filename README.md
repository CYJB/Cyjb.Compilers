Cyjb.Compiler
====

My personal C# library of compiler.

我的个人 C# 编译类库。欢迎访问我的[博客](http://www.cnblogs.com/cyjb/)。

本项目包括一些与编译相关的类，包括：

* Cyjb.Compiler 命名空间：包含定义词法和语法分析器的类。
	- Grammar 类：表示词法分析或语法分析中使用的语法规则。
	- Symbol 类：表示词法或语法分析中的终结符或非终结符。
	- TerminalSymbol 类：表示词法分析或语法分析的终结符。
* Cyjb.Compiler.Lexer 命名空间：包含定义词法分析器的类。
	- LexerContext 类：表示词法分析器的上下文。
	- LexerRule 类：表示词法分析器的规则。
	- ReaderController 类：表示词法单元读取器的控制器。
	- TokenReader 类：表示词法单元读取器的基类。
* Cyjb.Compiler.RegularExpressions 命名空间：包含定义与解析正则表达式的类。
	- AlternationExp 类：表示并联的正则表达式。
	- AnchorExp 类：表示定位点的正则表达式。
	- CharClassExp 类：表示字符类的正则表达式。
	- ConcatenationExp 类：表示连接的正则表达式。
	- EndOfFileExp 类：表示到达文件结尾的正则表达式。
	- LiteralExp 类：表示逐字字符串的正则表达式。
	- Regex 类：表示正则表达式。
	- RegexCharClass 类：表示正则表达式的字符类。
	- RegexOptions 类：提供用于设置正则表达式选项的枚举值。
	- RegexParser 类：表示正则表达式的分析器。
	- RepeatExp 类：表示重复多次的正则表达式。
