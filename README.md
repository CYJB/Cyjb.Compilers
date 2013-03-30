Cyjb.Compiler
====

My personal C# library of compiler.

我的个人 C# 编译类库。欢迎访问我的[博客](http://www.cnblogs.com/cyjb/)。

本项目包括一些与编译相关的类，包括：

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
