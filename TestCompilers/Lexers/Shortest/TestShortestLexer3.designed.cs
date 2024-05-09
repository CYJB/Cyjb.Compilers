﻿//------------------------------------------------------------------------------
// <auto-generated>
// 此代码由工具生成。
//
// 对此文件的更改可能会导致不正确的行为，并且如果
// 重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

internal partial class TestShortestLexer3 
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
			// 0: ab+
			new TerminalData<TestKind>(TestKind.A, action: (TestShortestLexer3 c) => c.TestAction(), useShortest: true)
		};
		// 字符类信息
		// 0: [a]
		// 1: [b]
		// 字符类索引
		uint[] indexes = Array.Empty<uint>();
		// 字符类列表
		int[] classes = new[]
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 0, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1
		};
		// 状态转移
		//    0  1 -> Symbols
		// 0  1   
		// 1     2
		// 2     2 -> 0
		// 状态列表
		int[] states = new[]
		{
			0, -1, 0, 0, 0, -1, 0, 0,
			1, -1, 1, 12, 0
		};
		// 状态转移
		int[] trans = new[]
		{
			0, 1, 1, 2, 2, 2
		};
		// 词法分析器的数据
		LexerData<TestKind> lexerData = new(null,
			terminals,
			new CharClassMap(indexes, classes, null),
			states,
			trans,
			TrailingType.None,
			false,
			true,
			typeof(TestShortestLexer3));
		return new LexerFactory<TestKind, TestShortestLexer3>(lexerData);
	}
}



