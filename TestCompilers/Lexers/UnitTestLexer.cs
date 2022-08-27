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

		TestCalc(lexer.GetFactory());
	}

	/// <summary>
	/// 对设计时计算器词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestInDesignCalc()
	{
		TestCalc(TestCalcController.Factory);
	}

	/// <summary>
	/// 测试计算器词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestCalc(ILexerFactory<Calc> factory)
	{
		string source = "1 + 20 * 3 / 4*(5+6)";
		Tokenlizer<Calc> tokenlizer = factory.CreateTokenlizer(source);
		Assert.AreEqual(new Token<Calc>(Calc.Id, "1", new TextSpan(0, 1), 1), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Add, "+", new TextSpan(2, 3)), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "20", new TextSpan(4, 6), 20), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Mul, "*", new TextSpan(7, 8)), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "3", new TextSpan(9, 10), 3), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Div, "/", new TextSpan(11, 12)), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "4", new TextSpan(13, 14), 4), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Mul, "*", new TextSpan(14, 15)), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.LBrace, "(", new TextSpan(15, 16)), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "5", new TextSpan(16, 17), 5), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Add, "+", new TextSpan(17, 18)), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "6", new TextSpan(18, 19), 6), tokenlizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.RBrace, ")", new TextSpan(19, 20)), tokenlizer.Read());
		Assert.AreEqual(Token<Calc>.GetEndOfFile(20), tokenlizer.Read());
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

		TestString(lexer.GetFactory());
	}

	/// <summary>
	/// 对设计时字符串词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestInDesignString()
	{
		TestString(TestStrController.Factory);
	}

	/// <summary>
	/// 测试字符串词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestString(ILexerFactory<Str> factory)
	{
		string source = @"""abcd\n\r""""aabb\""ccd\u0045\x47""@""abcd\n\r""@""aabb\""""ccd\u0045\x47""";
		Tokenlizer<Str> tokenlizer = factory.CreateTokenlizer(source);
		Assert.AreEqual(new Token<Str>(Str.Str, @"""abcd\n\r""", new TextSpan(0, 10)), tokenlizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"""aabb\""ccd\u0045\x47""", new TextSpan(10, 31)), tokenlizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"@""abcd\n\r""", new TextSpan(31, 42)), tokenlizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"@""aabb\""""ccd\u0045\x47""", new TextSpan(42, 65)), tokenlizer.Read());
		Assert.AreEqual(Token<Str>.GetEndOfFile(65), tokenlizer.Read());
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
		TestEscapeString(lexer.GetFactory());
	}

	/// <summary>
	/// 对设计时转义字符串词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestInDesignEscapeString()
	{
		TestEscapeString(TestEscapeStrController.Factory);
	}

	/// <summary>
	/// 测试转义字符串词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestEscapeString(ILexerFactory<Str> factory)
	{
		string source = @"""abcd\n\r""""aabb\""ccd\u0045\x47""@""abcd\n\r""@""aabb\""""ccd\u0045\x47""";
		Tokenlizer<Str> tokenlizer = factory.CreateTokenlizer(source);
		Assert.AreEqual(new Token<Str>(Str.Str, "abcd\n\r", new TextSpan(0, 10)), tokenlizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, "aabb\"ccd\u0045\x47", new TextSpan(10, 31)), tokenlizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"abcd\n\r", new TextSpan(31, 42)), tokenlizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"aabb\""ccd\u0045\x47", new TextSpan(42, 65)), tokenlizer.Read());
		Assert.AreEqual(Token<Str>.GetEndOfFile(65), tokenlizer.Read());
	}

	/// <summary>
	/// 对产生式词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestDefineProduction()
	{
		Lexer<ProductionKind> lexer = new();
		// 终结符的定义。
		lexer.DefineSymbol(@"\d+|\w[\w\d]*").Kind(ProductionKind.Id);
		lexer.DefineSymbol(@"\(").Kind(ProductionKind.LBrace);
		lexer.DefineSymbol(@"\)").Kind(ProductionKind.RBrace);
		lexer.DefineSymbol(@"\+").Kind(ProductionKind.Plus);
		lexer.DefineSymbol(@"\*").Kind(ProductionKind.Star);
		lexer.DefineSymbol(@"\?").Kind(ProductionKind.Question);
		// 吃掉所有空白。
		lexer.DefineSymbol(@"\s");

		TestProuction(lexer.GetFactory());
	}

	/// <summary>
	/// 对设计时产生式词法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestInDesignProuction()
	{
		TestProuction(TestProductionController.Factory);
	}

	/// <summary>
	/// 测试产生式词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestProuction(ILexerFactory<ProductionKind> factory)
	{
		string source = "A B21 381 D  * E+(F G2)* ";
		Tokenlizer<ProductionKind> tokenlizer = factory.CreateTokenlizer(source);
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "A", new TextSpan(0, 1)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "B21", new TextSpan(2, 5)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "381", new TextSpan(6, 9)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "D", new TextSpan(10, 11)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Star, "*", new TextSpan(13, 14)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "E", new TextSpan(15, 16)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Plus, "+", new TextSpan(16, 17)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.LBrace, "(", new TextSpan(17, 18)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "F", new TextSpan(18, 19)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "G2", new TextSpan(20, 22)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.RBrace, ")", new TextSpan(22, 23)), tokenlizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Star, "*", new TextSpan(23, 24)), tokenlizer.Read());
		Assert.AreEqual(Token<ProductionKind>.GetEndOfFile(25), tokenlizer.Read());
	}
}
