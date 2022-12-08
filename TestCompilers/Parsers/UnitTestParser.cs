using System;
using System.Collections;
using System.Linq;
using Cyjb.Compilers.Parsers;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Parsers;

/// <summary>
/// <see cref="Parser"/> 类的单元测试。
/// </summary>
[TestClass]
public partial class UnitTestParser
{
	/// <summary>
	/// 对计算器语法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestDefineCalc()
	{
		Parser<Calc> parser = new();
		// 定义产生式
		parser.DefineProduction(Calc.E, Calc.Id).Action(c => c[0].Value);
		parser.DefineProduction(Calc.E, Calc.E, Calc.Add, Calc.E)
			.Action(c => (double)c[0].Value! + (double)c[2].Value!);
		parser.DefineProduction(Calc.E, Calc.E, Calc.Sub, Calc.E)
			.Action(c => (double)c[0].Value! - (double)c[2].Value!);
		parser.DefineProduction(Calc.E, Calc.E, Calc.Mul, Calc.E)
			.Action(c => (double)c[0].Value! * (double)c[2].Value!);
		parser.DefineProduction(Calc.E, Calc.E, Calc.Div, Calc.E)
			.Action(c =>
			{
				return (double)c[0].Value! / (double)c[2].Value!;
			});
		parser.DefineProduction(Calc.E, Calc.E, Calc.Pow, Calc.E)
			.Action(c => Math.Pow((double)c[0].Value!, (double)c[2].Value!));
		parser.DefineProduction(Calc.E, Calc.LBrace, Calc.E, Calc.RBrace)
			.Action(c => c[1].Value);
		// 定义运算符优先级。
		parser.DefineAssociativity(AssociativeType.Left, Calc.Add, Calc.Sub);
		parser.DefineAssociativity(AssociativeType.Left, Calc.Mul, Calc.Div);
		parser.DefineAssociativity(AssociativeType.Right, Calc.Pow);
		parser.DefineAssociativity(AssociativeType.NonAssociate, Calc.Id);

		TestCalc(parser.GetFactory());
	}

	/// <summary>
	/// 对设计时计算器语法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestInDesignCalc()
	{
		TestCalc(TestCalcParser.Factory);
	}

	/// <summary>
	/// 测试计算器语法分析器。
	/// </summary>
	/// <param name="factory">语法分析器的工厂。</param>
	private static void TestCalc(IParserFactory<Calc> factory)
	{
		ITokenizer<Calc> tokenizer = new EnumerableTokenizer<Calc>(new Token<Calc>[]
		{
			new Token<Calc>(Calc.Id, "1", 1.0),
			new Token<Calc>(Calc.Add, "+"),
			new Token<Calc>(Calc.Id, "20", 20.0),
			new Token<Calc>(Calc.Mul, "*"),
			new Token<Calc>(Calc.Id, "3", 3.0),
			new Token<Calc>(Calc.Div, "/"),
			new Token<Calc>(Calc.Id, "4", 4.0),
			new Token<Calc>(Calc.Mul, "*"),
			new Token<Calc>(Calc.LBrace, "("),
			new Token<Calc>(Calc.Id, "5", 5.0),
			new Token<Calc>(Calc.Add, "+"),
			new Token<Calc>(Calc.Id, "6", 6.0),
			new Token<Calc>(Calc.RBrace, ")"),
		});
		ITokenParser<Calc> parser = factory.CreateParser(tokenizer);
		Assert.AreEqual(ParseStatus.Ready, parser.Status);
		Assert.AreEqual(166.0, parser.Parse().Value);
		Assert.AreEqual(ParseStatus.Finished, parser.Status);
	}

	/// <summary>
	/// 对产生式语法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestDefineProduction()
	{
		Parser<ProductionKind> parser = new();
		// 定义产生式
		parser.DefineProduction(ProductionKind.AltExp, ProductionKind.Exp)
			.Action(c => c[0].Value);
		parser.DefineProduction(ProductionKind.AltExp, ProductionKind.AltExp, ProductionKind.Or, ProductionKind.Exp)
			.Action(c => $"({c[0].Value})|({c[2].Value})");
		parser.DefineProduction(ProductionKind.Exp, ProductionKind.Repeat, SymbolOption.OneOrMore)
			.Action(c => string.Join(" ", ((IList)c[0].Value!).Cast<string>()));
		parser.DefineProduction(ProductionKind.Exp, ProductionKind.Exp, ProductionKind.Repeat)
			.Action(c => $"{c[0].Value} {c[1].Value}");
		parser.DefineProduction(ProductionKind.Repeat, ProductionKind.Item)
			.Action(c => c[0].Value);
		parser.DefineProduction(ProductionKind.Repeat, ProductionKind.Item, ProductionKind.Plus)
			.Action(c => $"{c[0].Value}+");
		parser.DefineProduction(ProductionKind.Repeat, ProductionKind.Item, ProductionKind.Star)
			.Action(c => $"{c[0].Value}*");
		parser.DefineProduction(ProductionKind.Repeat, ProductionKind.Item, ProductionKind.Question)
			.Action(c => $"{c[0].Value}?");
		parser.DefineProduction(ProductionKind.Item, ProductionKind.Id)
			.Action(c => c[0].Text);
		parser.DefineProduction(ProductionKind.Item,
			ProductionKind.LBrace, ProductionKind.AltExp, ProductionKind.RBrace)
			.Action(c => $"({c[1].Value})");

		TestProuction(parser.GetFactory());
	}

	/// <summary>
	/// 对设计时产生式语法分析进行测试。
	/// </summary>
	[TestMethod]
	public void TestInDesignProuction()
	{
		TestProuction(TestProductionParser.Factory);
	}

	/// <summary>
	/// 测试产生式语法分析器。
	/// </summary>
	/// <param name="factory">语法分析器的工厂。</param>
	private static void TestProuction(IParserFactory<ProductionKind> factory)
	{
		ITokenizer<ProductionKind> tokenizer = new EnumerableTokenizer<ProductionKind>(new Token<ProductionKind>[]
		{
			new Token<ProductionKind>(ProductionKind.Id, "A"),
			new Token<ProductionKind>(ProductionKind.Id, "B"),
			new Token<ProductionKind>(ProductionKind.Star, "*"),
			new Token<ProductionKind>(ProductionKind.Id, "C"),
			new Token<ProductionKind>(ProductionKind.Id, "D"),
			new Token<ProductionKind>(ProductionKind.LBrace, "("),
			new Token<ProductionKind>(ProductionKind.Id, "E"),
			new Token<ProductionKind>(ProductionKind.Id, "F"),
			new Token<ProductionKind>(ProductionKind.Or, "|"),
			new Token<ProductionKind>(ProductionKind.Id, "E2"),
			new Token<ProductionKind>(ProductionKind.Id, "F2"),
			new Token<ProductionKind>(ProductionKind.RBrace, ")"),
			new Token<ProductionKind>(ProductionKind.Plus, "+"),
			new Token<ProductionKind>(ProductionKind.Id, "G"),
			new Token<ProductionKind>(ProductionKind.Question, "?"),
		});
		ITokenParser<ProductionKind> parser = factory.CreateParser(tokenizer);
		Assert.AreEqual(ParseStatus.Ready, parser.Status);
		Assert.AreEqual("A B* C D ((E F)|(E2 F2))+ G?", parser.Parse().Value);
		Assert.AreEqual(ParseStatus.Finished, parser.Status);
	}

	/// <summary>
	/// 对空产生式的 Span 进行测试。
	/// </summary>
	[TestMethod]
	public void TestSpanForEmptyProductionBody()
	{
		Parser<TestKind> parser = new();
		// 定义产生式
		parser.DefineProduction(TestKind.A);
		parser.DefineProduction(TestKind.A, TestKind.A, TestKind.B);
		var factory = parser.GetFactory();

		ITokenizer<TestKind> tokenizer = new EnumerableTokenizer<TestKind>(new Token<TestKind>[]
		{
			new Token<TestKind>(TestKind.B, "1", new TextSpan(10, 11)),
		});
		ITokenParser<TestKind> tokenParser = factory.CreateParser(tokenizer);
		Assert.AreEqual(new TextSpan(10, 11), tokenParser.Parse().Span);
	}
}
