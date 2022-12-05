using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Cyjb.Compilers.Lexers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

/// <summary>
/// <see cref="Lexer"/> 类的单元测试。
/// </summary>
[TestClass]
public partial class UnitTestLexer
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
		TestCalc(TestCalcLexer.Factory);
	}

	/// <summary>
	/// 测试计算器词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestCalc(ILexerFactory<Calc> factory)
	{
		string source = "1 + 20 * 3 / 4*(5+6)";
		ITokenizer<Calc> tokenizer = factory.CreateTokenizer(source);
		Assert.AreEqual(ParseStatus.Ready, tokenizer.Status);
		Assert.AreEqual(new Token<Calc>(Calc.Id, "1", new TextSpan(0, 1), 1), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Add, "+", new TextSpan(2, 3)), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "20", new TextSpan(4, 6), 20), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Mul, "*", new TextSpan(7, 8)), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "3", new TextSpan(9, 10), 3), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Div, "/", new TextSpan(11, 12)), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "4", new TextSpan(13, 14), 4), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Mul, "*", new TextSpan(14, 15)), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.LBrace, "(", new TextSpan(15, 16)), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "5", new TextSpan(16, 17), 5), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Add, "+", new TextSpan(17, 18)), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.Id, "6", new TextSpan(18, 19), 6), tokenizer.Read());
		Assert.AreEqual(new Token<Calc>(Calc.RBrace, ")", new TextSpan(19, 20)), tokenizer.Read());
		Assert.AreEqual(Token<Calc>.GetEndOfFile(20), tokenizer.Read());
		Assert.AreEqual(ParseStatus.Finished, tokenizer.Status);

		ITokenizer<Calc> errorTokenizer = factory.CreateTokenizer("1ss");
		int errorIndex = 0;
		errorTokenizer.TokenizeError += (tokenizer, error) =>
		{
			if (errorIndex == 0)
			{
				Assert.AreEqual(new TokenizeError("s", new TextSpan(1, 2), null), error);
			}
			else
			{
				Assert.AreEqual(new TokenizeError("s", new TextSpan(2, 3), null), error);
			}
			errorIndex++;
		};
		Assert.AreEqual(new Token<Calc>(Calc.Id, "1", new TextSpan(0, 1), 1), errorTokenizer.Read());
		Assert.AreEqual(Token<Calc>.GetEndOfFile(3), errorTokenizer.Read());
		Assert.AreEqual(2, errorIndex);
	}

	/// <summary>
	/// 对词法单元的值进行测试。
	/// </summary>
	[TestMethod]
	public void TestDefineSymbolValue()
	{
		Lexer<Calc> lexer = new();
		lexer.DefineSymbol("[0-9]+").Kind(Calc.Id).Value(101).Action(c =>
		{
			Assert.AreEqual(101, c.Value);
			c.Accept((int)c.Value! + 10);
		});

		TestSymbolValue(lexer.GetFactory());
	}

	/// <summary>
	/// 对设计时词法单元的值进行测试。
	/// </summary>
	[TestMethod]
	public void TestInDesignSymbolValue()
	{
		TestSymbolValue(TestSymbolValueLexer.Factory);
	}

	/// <summary>
	/// 测试词法单元的值。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestSymbolValue(ILexerFactory<Calc> factory)
	{
		string source = "20";
		ITokenizer<Calc> tokenizer = factory.CreateTokenizer(source);
		Assert.AreEqual(ParseStatus.Ready, tokenizer.Status);
		Assert.AreEqual(new Token<Calc>(Calc.Id, "20", new TextSpan(0, 2), 111), tokenizer.Read());
		Assert.AreEqual(Token<Calc>.GetEndOfFile(2), tokenizer.Read());
		Assert.AreEqual(ParseStatus.Finished, tokenizer.Status);
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
		TestString(TestStrLexer.Factory);
	}

	/// <summary>
	/// 测试字符串词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestString(ILexerFactory<Str> factory)
	{
		string source = @"""abcd\n\r""""aabb\""ccd\u0045\x47""@""abcd\n\r""@""aabb\""""ccd\u0045\x47""";
		ITokenizer<Str> tokenizer = factory.CreateTokenizer(source);
		Assert.AreEqual(new Token<Str>(Str.Str, @"""abcd\n\r""", new TextSpan(0, 10)), tokenizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"""aabb\""ccd\u0045\x47""", new TextSpan(10, 31)), tokenizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"@""abcd\n\r""", new TextSpan(31, 42)), tokenizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"@""aabb\""""ccd\u0045\x47""", new TextSpan(42, 65)), tokenizer.Read());
		Assert.AreEqual(Token<Str>.GetEndOfFile(65), tokenizer.Read());
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
		TestEscapeString(TestEscapeStrLexer.Factory);
	}

	/// <summary>
	/// 测试转义字符串词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestEscapeString(ILexerFactory<Str> factory)
	{
		string source = @"""abcd\n\r""""aabb\""ccd\u0045\x47""@""abcd\n\r""@""aabb\""""ccd\u0045\x47""";
		ITokenizer<Str> tokenizer = factory.CreateTokenizer(source);
		Assert.AreEqual(new Token<Str>(Str.Str, "abcd\n\r", new TextSpan(0, 10)), tokenizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, "aabb\"ccd\u0045\x47", new TextSpan(10, 31)), tokenizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"abcd\n\r", new TextSpan(31, 42)), tokenizer.Read());
		Assert.AreEqual(new Token<Str>(Str.Str, @"aabb\""ccd\u0045\x47", new TextSpan(42, 65)), tokenizer.Read());
		Assert.AreEqual(Token<Str>.GetEndOfFile(65), tokenizer.Read());
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
		TestProuction(TestProductionLexer.Factory);
	}

	/// <summary>
	/// 测试产生式词法分析器。
	/// </summary>
	/// <param name="factory">词法分析器的工厂。</param>
	private static void TestProuction(ILexerFactory<ProductionKind> factory)
	{
		string source = "A B21 381 D  * E+(F G2)* ";
		ITokenizer<ProductionKind> tokenizer = factory.CreateTokenizer(source);
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "A", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "B21", new TextSpan(2, 5)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "381", new TextSpan(6, 9)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "D", new TextSpan(10, 11)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Star, "*", new TextSpan(13, 14)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "E", new TextSpan(15, 16)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Plus, "+", new TextSpan(16, 17)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.LBrace, "(", new TextSpan(17, 18)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "F", new TextSpan(18, 19)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Id, "G2", new TextSpan(20, 22)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.RBrace, ")", new TextSpan(22, 23)), tokenizer.Read());
		Assert.AreEqual(new Token<ProductionKind>(ProductionKind.Star, "*", new TextSpan(23, 24)), tokenizer.Read());
		Assert.AreEqual(Token<ProductionKind>.GetEndOfFile(25), tokenizer.Read());
	}

	/// <summary>
	/// 测试向前看。
	/// </summary>
	[TestMethod]
	public void TestTrailing()
	{
		// 前面固定长度
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"ab/c").Kind(TestKind.A);
		lexer.DefineSymbol(@".", RegexOptions.Singleline).Kind(TestKind.B);
		var factory = lexer.GetFactory();

		var tokenizer = factory.CreateTokenizer("ab");
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(2), tokenizer.Read());

		tokenizer = factory.CreateTokenizer("abc");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "ab", new TextSpan(0, 2)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "c", new TextSpan(2, 3)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(3), tokenizer.Read());

		// 后面固定长度
		lexer = new();
		lexer.DefineSymbol(@"a+/b").Kind(TestKind.A);
		lexer.DefineSymbol(@".", RegexOptions.Singleline).Kind(TestKind.B);
		factory = lexer.GetFactory();

		tokenizer = factory.CreateTokenizer("ab");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(2), tokenizer.Read());

		tokenizer = factory.CreateTokenizer("ac");
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "c", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(2), tokenizer.Read());

		// 都不固定长度
		lexer = new();
		lexer.DefineSymbol(@"a+/b*c").Kind(TestKind.A);
		lexer.DefineSymbol(@".", RegexOptions.Singleline).Kind(TestKind.B);
		factory = lexer.GetFactory();

		tokenizer = factory.CreateTokenizer("aabbc");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "aa", new TextSpan(0, 2)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(2, 3)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(3, 4)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "c", new TextSpan(4, 5)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(5), tokenizer.Read());

		tokenizer = factory.CreateTokenizer("aabb");
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "a", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(2, 3)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(3, 4)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(4), tokenizer.Read());
	}

	/// <summary>
	/// 测试向前看行尾。
	/// </summary>
	[TestMethod]
	public void TestTrailingEOL()
	{
		Lexer<TestKind> lexer = new();
		lexer.DefineSymbol(@"a$").Kind(TestKind.A);
		lexer.DefineSymbol(@".", RegexOptions.Singleline).Kind(TestKind.B);
		var factory = lexer.GetFactory();

		var tokenizer = factory.CreateTokenizer("ab");
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "b", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(2), tokenizer.Read());

		// 匹配 \n。
		tokenizer = factory.CreateTokenizer("a\n");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "\n", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(2), tokenizer.Read());

		// 匹配 \r\n
		tokenizer = factory.CreateTokenizer("a\r\n");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "\r", new TextSpan(1, 2)), tokenizer.Read());
		Assert.AreEqual(new Token<TestKind>(TestKind.B, "\n", new TextSpan(2, 3)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(3), tokenizer.Read());

		// 匹配 EOF
		tokenizer = factory.CreateTokenizer("a");
		Assert.AreEqual(new Token<TestKind>(TestKind.A, "a", new TextSpan(0, 1)), tokenizer.Read());
		Assert.AreEqual(Token<TestKind>.GetEndOfFile(1), tokenizer.Read());
	}
}
