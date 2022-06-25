using System;
using System.Globalization;
using System.Text;
using Cyjb.Compilers.Lexers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

/// <summary>
/// <see cref="Lexer"/> 类的单元测试。
/// </summary>
[TestClass]
public class UnitTestLexer
{
	/// <summary>
	/// 对计算器词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestDefineCalc()
	{
		Lexer<Calc> lexer = new();
		// 终结符的定义。
		lexer.DefineSymbol("[0-9]+").Kind(Calc.Id).Action(c =>
		{
			c.Value = int.Parse(c.Text);
			c.Accept();
		});
		lexer.DefineSymbol("\\+").Kind(Calc.Add);
		lexer.DefineSymbol("\\-").Kind(Calc.Sub);
		lexer.DefineSymbol("\\*").Kind(Calc.Mul);
		lexer.DefineSymbol("\\/").Kind(Calc.Div);
		lexer.DefineSymbol("\\^").Kind(Calc.Pow);
		lexer.DefineSymbol("\\(").Kind(Calc.LBrace);
		lexer.DefineSymbol("\\)").Kind(Calc.RBrace);
		// 吃掉所有空白。
		lexer.DefineSymbol("\\s");
		LexerFactory<Calc> factory = lexer.GetFactory();

		// 测试分析源码
		string source = "1 + 20 * 3 / 4*(5+6)";
		TokenReader<Calc> reader = factory.CreateReader(source);
		Assert.AreEqual(new Token<Calc>(Calc.Id, "1", new TextSpan(0, 1), 1), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Add, "+", new TextSpan(2, 3)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "20", new TextSpan(4, 6), 20), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Mul, "*", new TextSpan(7, 8)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "3", new TextSpan(9, 10), 3), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Div, "/", new TextSpan(11, 12)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "4", new TextSpan(13, 14), 4), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Mul, "*", new TextSpan(14, 15)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.LBrace, "(", new TextSpan(15, 16)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "5", new TextSpan(16, 17), 5), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Add, "+", new TextSpan(17, 18)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "6", new TextSpan(18, 19), 6), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.RBrace, ")", new TextSpan(19, 20)), reader.Read());
		Assert.AreEqual(Token<Calc>.GetEndOfFile(20), reader.Read());
	}

	/// <summary>
	/// 对设计时计算器词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestInDesignCalc()
	{
		LexerFactory<Calc, TestCalcController> factory = Lexer.GetFactory<Calc, TestCalcController>();

		// 测试分析源码
		string source = "1 + 20 * 3 / 4*(5+6)";
		TokenReader<Calc> reader = factory.CreateReader(source);
		Assert.AreEqual(new Token<Calc>(Calc.Id, "1", new TextSpan(0, 1), 1), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Add, "+", new TextSpan(2, 3)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "20", new TextSpan(4, 6), 20), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Mul, "*", new TextSpan(7, 8)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "3", new TextSpan(9, 10), 3), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Div, "/", new TextSpan(11, 12)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "4", new TextSpan(13, 14), 4), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Mul, "*", new TextSpan(14, 15)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.LBrace, "(", new TextSpan(15, 16)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "5", new TextSpan(16, 17), 5), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Add, "+", new TextSpan(17, 18)), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "6", new TextSpan(18, 19), 6), reader.Read());
		Assert.AreEqual(new Token<Calc>(Calc.RBrace, ")", new TextSpan(19, 20)), reader.Read());
		Assert.AreEqual(Token<Calc>.GetEndOfFile(20), reader.Read());
	}

	/// <summary>
	/// 对字符串词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestDefineString()
	{
		Lexer<Str> lexer = new();
		// 终结符的定义。
		lexer.DefineRegex("regular_char", @"[^""\\\n\r\u0085\u2028\u2029]|(\\.)");
		lexer.DefineRegex("regular_literal", @"\""{regular_char}*\""");
		lexer.DefineRegex("verbatim_char", @"[^""]|\""\""");
		lexer.DefineRegex("verbatim_literal", @"@\""{verbatim_char}*\""");
		lexer.DefineSymbol("{regular_literal}|{verbatim_literal}").Kind(Str.Str);
		LexerFactory<Str> factory = lexer.GetFactory();

		// 测试分析源码
		string source = @"""abcd\n\r""""aabb\""ccd\u0045\x47""@""abcd\n\r""@""aabb\""""ccd\u0045\x47""";
		TokenReader<Str> reader = factory.CreateReader(source);
		Assert.AreEqual(new Token<Str>(Str.Str, @"""abcd\n\r""", new TextSpan(0, 10)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"""aabb\""ccd\u0045\x47""", new TextSpan(10, 31)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"@""abcd\n\r""", new TextSpan(31, 42)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"@""aabb\""""ccd\u0045\x47""", new TextSpan(42, 65)), reader.Read());
		Assert.AreEqual(Token<Str>.GetEndOfFile(65), reader.Read());
	}

	/// <summary>
	/// 对设计时字符串词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestInDesignString()
	{
		LexerFactory<Str, TestStrController> factory = Lexer.GetFactory<Str, TestStrController>();

		// 测试分析源码
		string source = @"""abcd\n\r""""aabb\""ccd\u0045\x47""@""abcd\n\r""@""aabb\""""ccd\u0045\x47""";
		TokenReader<Str> reader = factory.CreateReader(source);
		Assert.AreEqual(new Token<Str>(Str.Str, @"""abcd\n\r""", new TextSpan(0, 10)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"""aabb\""ccd\u0045\x47""", new TextSpan(10, 31)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"@""abcd\n\r""", new TextSpan(31, 42)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"@""aabb\""""ccd\u0045\x47""", new TextSpan(42, 65)), reader.Read());
		Assert.AreEqual(Token<Str>.GetEndOfFile(65), reader.Read());
	}

	private class EscapeStrController : LexerController<Str>
	{
		/// <summary>
		/// 当前起始索引。
		/// </summary>
		public int CurStart;
		/// <summary>
		/// 处理后的文本。
		/// </summary>
		public StringBuilder DecodedText = new();
	}

	/// <summary>
	/// 对转义字符串词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestDefineEscapeString()
	{
		Lexer<Str, EscapeStrController> lexer = new();
		const string ctxStr = "str";
		const string ctxVstr = "vstr";
		lexer.DefineContext(ctxStr);
		lexer.DefineContext(ctxVstr);
		// 终结符的定义。
		lexer.DefineSymbol(@"\""").Action(c =>
		{
			c.PushContext(ctxStr);
			c.CurStart = c.Start;
			c.DecodedText.Clear();
		});
		lexer.DefineSymbol(@"@\""").Action(c =>
		{
			c.PushContext(ctxVstr);
			c.CurStart = c.Start;
			c.DecodedText.Clear();
		});
		lexer.DefineSymbol(@"\""").Context(ctxStr).Action(c =>
		{
			c.PopContext();
			c.Start = c.CurStart;
			c.Text = c.DecodedText.ToString();
			c.Accept(Str.Str);
		});
		lexer.DefineSymbol(@"\\u[0-9]{4}").Context(ctxStr).Action(c =>
		{
			c.DecodedText.Append((char)int.Parse(c.Text.AsSpan()[2..], NumberStyles.HexNumber));
		});
		lexer.DefineSymbol(@"\\x[0-9]{2}").Context(ctxStr).Action(c =>
		{
			c.DecodedText.Append((char)int.Parse(c.Text.AsSpan()[2..], NumberStyles.HexNumber));
		});
		lexer.DefineSymbol(@"\\n").Context(ctxStr).Action(c =>
		{
			c.DecodedText.Append('\n');
		});
		lexer.DefineSymbol(@"\\\""").Context(ctxStr).Action(c =>
		{
			c.DecodedText.Append('\"');
		});
		lexer.DefineSymbol(@"\\r").Context(ctxStr).Action(c =>
		{
			c.DecodedText.Append('\r');
		});
		lexer.DefineSymbol(@".").Context(ctxStr).Action(c =>
		{
			c.DecodedText.Append(c.Text);
		});
		lexer.DefineSymbol(@"\""").Context(ctxVstr).Action(c =>
		{
			c.PopContext();
			c.Start = c.CurStart;
			c.Text = c.DecodedText.ToString();
			c.Accept(Str.Str);
		});
		lexer.DefineSymbol(@"\""\""").Context(ctxVstr).Action(c =>
		{
			c.DecodedText.Append('"');
		});
		lexer.DefineSymbol(@".").Context(ctxVstr).Action(c =>
		{
			c.DecodedText.Append(c.Text);
		});
		LexerFactory<Str, EscapeStrController> factory = lexer.GetFactory();

		// 测试分析源码
		string source = @"""abcd\n\r""""aabb\""ccd\u0045\x47""@""abcd\n\r""@""aabb\""""ccd\u0045\x47""";
		TokenReader<Str> reader = factory.CreateReader(source);
		Assert.AreEqual(new Token<Str>(Str.Str, "abcd\n\r", new TextSpan(0, 10)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, "aabb\"ccd\u0045\x47", new TextSpan(10, 31)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"abcd\n\r", new TextSpan(31, 42)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"aabb\""ccd\u0045\x47", new TextSpan(42, 65)), reader.Read());
		Assert.AreEqual(Token<Str>.GetEndOfFile(65), reader.Read());
	}

	/// <summary>
	/// 对设计时转义字符串词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestInDesignEscapeString()
	{
		LexerFactory<Str, TestEscapeStrController> factory = Lexer.GetFactory<Str, TestEscapeStrController>();

		// 测试分析源码
		string source = @"""abcd\n\r""""aabb\""ccd\u0045\x47""@""abcd\n\r""@""aabb\""""ccd\u0045\x47""";
		TokenReader<Str> reader = factory.CreateReader(source);
		Assert.AreEqual(new Token<Str>(Str.Str, "abcd\n\r", new TextSpan(0, 10)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, "aabb\"ccd\u0045\x47", new TextSpan(10, 31)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"abcd\n\r", new TextSpan(31, 42)), reader.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"aabb\""ccd\u0045\x47", new TextSpan(42, 65)), reader.Read());
		Assert.AreEqual(Token<Str>.GetEndOfFile(65), reader.Read());
	}
}
