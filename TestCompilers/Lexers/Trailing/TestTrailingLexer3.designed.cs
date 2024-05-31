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

internal partial class TestTrailingLexer3 
{
	/// <summary>
	/// 词法分析器的工厂。
	/// </summary>
	[CompilerGeneratedAttribute]
	public static readonly ILexerFactory<TestKind> Factory = CreateLexerFactory();

	/// <summary>
	/// 创建词法分析器的工厂。
	/// </summary>
	[CompilerGeneratedAttribute]
	private static ILexerFactory<TestKind> CreateLexerFactory()
	{
		// 终结符数据
		TerminalData<TestKind>[] terminals = new[]
		{
			// 0: a+/b*c
			new TerminalData<TestKind>(TestKind.A, trailing: 0),
			// 1: .
			new TerminalData<TestKind>(TestKind.B)
		};
		// 字符类信息
		// 0: [a]
		// 1: [b]
		// 2: [c]
		// 3: [\0-`d-\uFFFE]
		// 字符类索引
		uint[] indexes = new[]
		{
			10551294U
		};
		// 字符类列表
		int[] classes = new[]
		{
			3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			3, 0, 1, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			3, 3, 3, 3, 3, 3, 3, 3, 3
		};
		// 字符类 Unicode 类别
		Dictionary<UnicodeCategory, int> categories = new()
		{
			 { UnicodeCategory.Control, 3 }
		};
		// 状态转移
		//    0  1  2  3 -> Symbols
		// 0  1  2  2  2
		// 1  3  4  5    -> 1, -1
		// 2             -> 1
		// 3  3  4  5    -> -1
		// 4     4  5   
		// 5             -> 0
		// 状态列表
		int[] states = new[]
		{
			0, -1, 0, 0, 4, -1, 2, 24,
			short.MinValue, -1, 1, 24, 7, -1, 1, 25,
			9, -1, 0, 0, short.MinValue, -1, 1, 26,
			1, -1, 0
		};
		// 状态转移
		int[] trans = new[]
		{
			0, 1, 0, 2, 0, 2, 0, 2, 1, 3, 1, 4,
			1, 5, 3, 3, 3, 4, 3, 5, 4, 4, 4, 5
		};
		// 词法分析器的数据
		LexerData<TestKind> lexerData = new(null,
			terminals,
			new CharClassMap(indexes, classes, categories),
			states,
			trans,
			TrailingType.Variable,
			false,
			false,
			typeof(TestTrailingLexer3));
		return new LexerFactory<TestKind, TestTrailingLexer3>(lexerData);
	}
}


