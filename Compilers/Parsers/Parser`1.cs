namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 提供构造语法分析器的功能。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <remarks>
/// <para>泛型参数 <typeparamref name="T"/> 一般是一个枚举类型，用于标识词法单元。
/// 其中包含了所有终结符和非终结符的定义。关于语法分析的相关信息，请参考我的系列博文
/// <see href="http://www.cnblogs.com/cyjb/archive/p/ParserIntroduce.html">
/// 《C# 语法分析器（一）语法分析介绍》</see>。</para></remarks>
/// <example>
/// 下面简单的构造一个数学算式的语法分析器：
/// <code>
/// enum Calc { Id, Add, Sub, Mul, Div, Pow, LBrace, RBrace, E }
/// // 非终结符的定义。
/// Parser&lt;Calc&gt; parser = new();
/// // 定义产生式
/// parser.DefineProduction(Calc.E, Calc.Id).Action(c => c[0].Value);
/// parser.DefineProduction(Calc.E, Calc.E, Calc.Add, Calc.E)
///		.Action(c => (double) c[0].Value! + (double) c[2].Value!);
/// parser.DefineProduction(Calc.E, Calc.E, Calc.Sub, Calc.E)
/// 	.Action(c => (double) c[0].Value! - (double) c[2].Value!);
/// parser.DefineProduction(Calc.E, Calc.E, Calc.Mul, Calc.E)
/// 	.Action(c => (double) c[0].Value! * (double) c[2].Value!);
/// parser.DefineProduction(Calc.E, Calc.E, Calc.Div, Calc.E)
/// 	.Action(c => (double) c[0].Value! / (double) c[2].Value!);
/// parser.DefineProduction(Calc.E, Calc.E, Calc.Pow, Calc.E)
/// 	.Action(c => Math.Pow((double) c[0].Value!, (double) c[2].Value!));
/// parser.DefineProduction(Calc.E, Calc.LBrace, Calc.E, Calc.RBrace)
/// 	.Action(c => c[1].Value);
/// // 定义运算符优先级。
/// parser.DefineAssociativity(AssociativeType.Left, Calc.Add, Calc.Sub);
/// parser.DefineAssociativity(AssociativeType.Left, Calc.Mul, Calc.Div);
/// parser.DefineAssociativity(AssociativeType.Right, Calc.Pow);
/// parser.DefineAssociativity(AssociativeType.NonAssociate, Calc.Id);
/// IParserFactory&lt;Calc&gt; factory = parser.GetFactory();
/// // 解析词法单元序列。
/// ITokenizer&lt;Calc&gt; tokenizer = /* 创建词法分析器 */;
/// ITokenParser&lt;Calc&gt; parser = factory.CreateParser(tokenizer);
/// Console.WriteLine(parser.Parse().Value);
/// // 输出 166.0
/// </code>
/// </example>
/// <seealso cref="ParserData{T}"/>
/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/ParserIntroduce.html">
/// 《C# 语法分析器（一）语法分析介绍》</seealso>
public sealed class Parser<T> : Parser<T, ParserController<T>>
	where T : struct
{
}
