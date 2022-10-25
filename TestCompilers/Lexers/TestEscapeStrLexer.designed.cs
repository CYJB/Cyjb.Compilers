﻿//------------------------------------------------------------------------------
// <auto-generated>
// 此代码由工具生成。
//
// 对此文件的更改可能会导致不正确的行为，并且如果
// 重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

public partial class TestEscapeStrLexer 
{
	/// <summary>
	/// 词法分析器的工厂。
	/// </summary>
	public static readonly ILexerFactory<Str> Factory = CreateLexerFactory();

	/// <summary>
	/// 创建词法分析器的工厂。
	/// </summary>
	[CompilerGeneratedAttribute]
	private static ILexerFactory<Str> CreateLexerFactory()
	{
		// 上下文数据
		Dictionary<string, ContextData> contexts = new()
		{
			 { ContextData.Initial, new ContextData(0, ContextData.Initial) },
			 { "str", new ContextData(1, "str") },
			 { "vstr", new ContextData(2, "vstr") }
		};
		// 终结符数据
		TerminalData<Str>[] terminals = new[]
		{
			// 0: \"
			new TerminalData<Str>(action: (TestEscapeStrLexer c) => c.BeginStrAction()),
			// 1: @\"
			new TerminalData<Str>(action: (TestEscapeStrLexer c) => c.BeginVstrAction()),
			// 2: <str, vstr>\"
			new TerminalData<Str>(Str.Str, action: (TestEscapeStrLexer c) => c.EndAction()),
			// 3: <str>\\u[0-9]{4}
			new TerminalData<Str>(action: (TestEscapeStrLexer c) => c.HexEscapeAction()),
			// 4: <str>\\x[0-9]{2}
			new TerminalData<Str>(action: (TestEscapeStrLexer c) => c.HexEscapeAction()),
			// 5: <str>\\n
			new TerminalData<Str>(action: (TestEscapeStrLexer c) => c.EscapeLFAction()),
			// 6: <str>\\\"
			new TerminalData<Str>(action: (TestEscapeStrLexer c) => c.EscapeQuoteAction()),
			// 7: <str>\\r
			new TerminalData<Str>(action: (TestEscapeStrLexer c) => c.EscapeCRAction()),
			// 8: <*>.
			new TerminalData<Str>(action: (TestEscapeStrLexer c) => c.CopyAction()),
			// 9: <vstr>\"\"
			new TerminalData<Str>(action: (TestEscapeStrLexer c) => c.VstrQuoteAction())
		};
		// 字符类信息
		// 0: ["]
		// 1: [@]
		// 2: [\\]
		// 3: [u]
		// 4: [0-9]
		// 5: [x]
		// 6: [n]
		// 7: [r]
		// 8: [\0-\t\v\f\u000E-!#-/:-?A-[]-mo-qstvwy-\uFFFF]
		// 字符类索引
		int[] indexes = new[]
		{
			10551295
		};
		// 字符类列表
		int[] classes = new[]
		{
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, -1, 8, 8, -1, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 0, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 8, 8, 8, 8, 8, 8, 1, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 2, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 6, 8, 8, 8, 7, 8, 8, 3, 8, 8,
			5, 8, 8, 8, 8, 8, 8, 8, 8
		};
		// 字符类 Unicode 类别
		Dictionary<UnicodeCategory, int> categories = new()
		{
			 { UnicodeCategory.Control, 8 }
		};
		// 状态转移
		//      0   1  2  3   4   5   6   7  8 -> Symbols
		//  0  19  20  4  4   4   4   4   4  4
		//  1   6   4  7  4   4   4   4   4  4
		//  2   3   4  4  4   4   4   4   4  4
		//  3   5                              -> 2 conflict 8
		//  4                                  -> 8
		//  5                                  -> 9
		//  6                                  -> 2 conflict 8
		//  7   8         9      10  11  12    -> 8
		//  8                                  -> 6
		//  9                15               
		// 10                13               
		// 11                                  -> 5
		// 12                                  -> 7
		// 13                14               
		// 14                                  -> 4
		// 15                16               
		// 16                17               
		// 17                18               
		// 18                                  -> 3
		// 19                                  -> 0 conflict 8
		// 20  21                              -> 8
		// 21                                  -> 1
		// 状态列表
		DfaStateData[] states = new[]
		{
			new DfaStateData(0, 2),
			new DfaStateData(2, 2),
			new DfaStateData(5, -1),
			new DfaStateData(3, -1, 2),
			new DfaStateData(int.MinValue, -1, 8),
			new DfaStateData(int.MinValue, -1, 9),
			new DfaStateData(int.MinValue, -1, 2),
			new DfaStateData(14, -1, 8),
			new DfaStateData(int.MinValue, -1, 6),
			new DfaStateData(11, -1),
			new DfaStateData(12, -1),
			new DfaStateData(int.MinValue, -1, 5),
			new DfaStateData(int.MinValue, -1, 7),
			new DfaStateData(14, -1),
			new DfaStateData(int.MinValue, -1, 4),
			new DfaStateData(18, -1),
			new DfaStateData(19, -1),
			new DfaStateData(20, -1),
			new DfaStateData(int.MinValue, -1, 3),
			new DfaStateData(int.MinValue, -1, 0),
			new DfaStateData(25, -1, 8),
			new DfaStateData(int.MinValue, -1, 1)
		};
		// 后继状态列表
		int[] next = new[]
		{
			19, 20, 6, 5, 7, 3, 4, 4, 4, 4, 4, 4, 4, 4, 8, 15, 13, 9, 14, 10, 11, 12, 16, 17,
			18, 21
		};
		// 状态检查列表
		int[] check = new[]
		{
			0, 0, 1, 3, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7, 9, 10, 7, 13, 7, 7, 7, 15, 16,
			17, 20
		};
		// 词法分析器的数据
		LexerData<Str> lexerData = new(contexts,
			terminals,
			new CharClassMap(indexes, classes, categories),
			states,
			next,
			check,
			TrailingType.None,
			false,
			false,
			typeof(TestEscapeStrLexer));
		return new LexerFactory<Str, TestEscapeStrLexer>(lexerData);
	}
}


