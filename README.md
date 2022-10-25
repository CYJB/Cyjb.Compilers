Cyjb.Compilers
====

提供编译相关功能，基于 .NET 6。

本项目包括一些与编译相关的功能，目前包含词法分析器和 LALR 语法分析器，以及相应的运行时。

- Compilers 包含了构造词法和语法分析器的功能。
- Design 提供了通过设计时 T4 模板生成词法分析器和语法分析器的能力。
- Generator 是用于 T4 模板的代码生成器，其产物会由 Design 工具嵌入使用。
- Runtime 提供了词法和语法分析的运行时。

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

