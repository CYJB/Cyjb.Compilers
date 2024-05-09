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

internal partial class TestShortestLexer2 
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
			// 0: a.+/b
			new TerminalData<TestKind>(TestKind.A, trailing: -1, useShortest: true)
		};
		// 字符类信息
		// 0: [a]
		// 1: [\0-\t\v\f\u000E-`c-\uFFFE]
		// 2: [b]
		// 字符类索引
		uint[] indexes = new[]
		{
			10551294U
		};
		// 字符类列表
		int[] classes = new[]
		{
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1, 1, 1, -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 0, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1
		};
		// 字符类 Unicode 类别
		Dictionary<UnicodeCategory, int> categories = new()
		{
			 { UnicodeCategory.Control, 1 }
		};
		// 状态转移
		//    0  1  2 -> Symbols
		// 0  1      
		// 1  2  2  2
		// 2  2  2  3 -> -1
		// 3  2  2  3 -> 0, -1
		// 状态列表
		int[] states = new[]
		{
			0, -1, 0, 0, 1, -1, 0, 0,
			4, -1, 1, 16, 7, -1, 2, 17,
			-1, 0, -1
		};
		// 状态转移
		int[] trans = new[]
		{
			0, 1, 1, 2, 1, 2, 1, 2, 2, 2, 2, 2,
			2, 3, 3, 2, 3, 2, 3, 3
		};
		// 词法分析器的数据
		LexerData<TestKind> lexerData = new(null,
			terminals,
			new CharClassMap(indexes, classes, categories),
			states,
			trans,
			TrailingType.Fixed,
			false,
			false,
			typeof(TestShortestLexer2));
		return new LexerFactory<TestKind, TestShortestLexer2>(lexerData);
	}
}



