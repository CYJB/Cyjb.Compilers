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

internal partial class TestCalcRunnerLexer 
{
	/// <summary>
	/// 词法分析器的工厂。
	/// </summary>
	[CompilerGeneratedAttribute]
	public static readonly ILexerFactory<Calc> Factory = CreateLexerFactory();

	/// <summary>
	/// 创建词法分析器的工厂。
	/// </summary>
	[CompilerGeneratedAttribute]
	private static ILexerFactory<Calc> CreateLexerFactory()
	{
		// 上下文数据
		Dictionary<string, ContextData> contexts = new()
		{
			 { ContextData.Initial, new ContextData(0, ContextData.Initial, (TestCalcRunnerLexer c) => c.EofAction()) }
		};
		// 终结符数据
		TerminalData<Calc>[] terminals = new[]
		{
			// 0: \s
			new TerminalData<Calc>(),
			// 1: [0-9]+
			new TerminalData<Calc>(Calc.Id, action: (TestCalcRunnerLexer c) => c.DigitAction()),
			// 2: \+
			new TerminalData<Calc>(Calc.Add, action: (TestCalcRunnerLexer c) => c.OperatorAction()),
			// 3: \-
			new TerminalData<Calc>(Calc.Sub, action: (TestCalcRunnerLexer c) => c.OperatorAction()),
			// 4: \*
			new TerminalData<Calc>(Calc.Mul, action: (TestCalcRunnerLexer c) => c.OperatorAction()),
			// 5: \/
			new TerminalData<Calc>(Calc.Div, action: (TestCalcRunnerLexer c) => c.OperatorAction()),
			// 6: \^
			new TerminalData<Calc>(Calc.Pow, action: (TestCalcRunnerLexer c) => c.OperatorAction()),
			// 7: \(
			new TerminalData<Calc>(Calc.LBrace, action: (TestCalcRunnerLexer c) => c.OperatorAction()),
			// 8: \)
			new TerminalData<Calc>(Calc.RBrace, action: (TestCalcRunnerLexer c) => c.OperatorAction())
		};
		// 字符类信息
		// 0: [\t-\r\u0085\p{Zs}\p{Zl}\p{Zp}]
		// 1: [0-9]
		// 2: [+]
		// 3: [-]
		// 4: [*]
		// 5: [/]
		// 6: [^]
		// 7: [(]
		// 8: [)]
		// 字符类索引
		uint[] indexes = new[]
		{
			8716421U
		};
		// 字符类列表
		int[] classes = new[]
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 0, -1, -1, -1, -1, -1, -1, -1, 7, 8, 4, 2, -1, 3, -1, 5,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 6, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 0
		};
		// 字符类 Unicode 类别
		Dictionary<UnicodeCategory, int> categories = new()
		{
			 { UnicodeCategory.LineSeparator, 0 },
			 { UnicodeCategory.ParagraphSeparator, 0 },
			 { UnicodeCategory.SpaceSeparator, 0 }
		};
		// 状态转移
		//    0  1  2  3  4  5  6  7  8 -> Symbols
		// 0  1  2  3  4  5  6  7  8  9
		// 1                            -> 0
		// 2     2                      -> 1
		// 3                            -> 2
		// 4                            -> 3
		// 5                            -> 4
		// 6                            -> 5
		// 7                            -> 6
		// 8                            -> 7
		// 9                            -> 8
		// 状态列表
		int[] states = new[]
		{
			0, -1, 0, 0, short.MinValue, -1, 1, 40,
			8, -1, 1, 41, short.MinValue, -1, 1, 42,
			short.MinValue, -1, 1, 43, short.MinValue, -1, 1, 44,
			short.MinValue, -1, 1, 45, short.MinValue, -1, 1, 46,
			short.MinValue, -1, 1, 47, short.MinValue, -1, 1, 48,
			0, 1, 2, 3, 4, 5, 6, 7,
			8
		};
		// 状态转移
		int[] trans = new[]
		{
			0, 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 6,
			0, 7, 0, 8, 0, 9, 2, 2
		};
		// 词法分析器的数据
		LexerData<Calc> lexerData = new(contexts,
			terminals,
			new CharClassMap(indexes, classes, categories),
			states,
			trans,
			TrailingType.None,
			false,
			false,
			typeof(TestCalcRunnerLexer));
		return new LexerFactory<Calc, TestCalcRunnerLexer>(lexerData);
	}
}



