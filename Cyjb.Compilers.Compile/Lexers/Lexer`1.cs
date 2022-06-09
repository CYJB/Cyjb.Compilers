namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析规则。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <remarks>
/// <para>泛型参数 <typeparamref name="T"/> 一般是一个枚举类型，用于标识词法单元。</para>
/// <para>对于词法分析中的冲突，总是选择最长的词素。如果最长的词素可以与多个模式匹配，
/// 则选择最先被定义的模式。关于词法分析的相关信息，请参考我的系列博文
/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerIntroduce.html">
/// 《C# 词法分析器（一）词法分析介绍》</see>，词法分析器的使用指南请参见
/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerSummary.html">
/// 《C# 词法分析器（七）总结》</see>。</para></remarks>
/// <example>
/// 下面简单的构造一个数学算式的词法分析器：
/// <code>
/// enum Calc { Id, Add, Sub, Mul, Div, Pow, LBrace, RBrace }
/// Lexer&lt;Calc&gt; lexer = new Lexer&lt;Calc&gt;();
/// // 终结符的定义。
/// lexer.DefineSymbol("[0-9]+").Kind(Calc.Id).Action(c => c.Accept(int.Parse(c.Text)));
/// lexer.DefineSymbol("\\+").Kind(Calc.Add);
/// lexer.DefineSymbol("\\-").Kind(Calc.Sub);
/// lexer.DefineSymbol("\\*").Kind(Calc.Mul);
/// lexer.DefineSymbol("\\/").Kind(Calc.Div);
/// lexer.DefineSymbol("\\^").Kind(Calc.Pow);
/// lexer.DefineSymbol("\\(").Kind(Calc.LBrace);
/// lexer.DefineSymbol("\\)").Kind(Calc.RBrace);
/// // 吃掉所有空白。
/// lexer.DefineSymbol("\\s");
/// LexerFactory&lt;Calc&gt; factory = lexer.GetFactory();
/// // 要分析的源文件。
/// string source = "1 + 20 * 3 / 4*(5+6)";
/// TokenReader&lt;Calc&gt; reader = factory.CreateReader(source);
/// // 构造词法分析器。
/// foreach (Token&lt;Calc&gt; token in reader)
/// {
/// 	Console.WriteLine(token);
/// }
/// </code>
/// </example>
/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerIntroduce.html">
/// 《C# 词法分析器（一）词法分析介绍》</seealso>
/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerSummary.html">
/// 《C# 词法分析器（七）总结》</seealso>
public sealed class Lexer<T> : Lexer<T, LexerController<T>>
	where T : struct
{
	/// <summary>
	/// 返回词法分析的工厂。
	/// </summary>
	/// <param name="rejectable">是否用到了 Reject 动作。</param>
	/// <returns>词法分析的数据。</returns>
	public new LexerFactory<T> GetFactory(bool rejectable = false)
	{
		LexerData<T> data = GetData(rejectable);
		return new LexerFactory<T>(data);
	}
}
