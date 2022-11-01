using System.Diagnostics;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 提供构造语法分析器的功能。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
/// <typeparam name="TController">语法分析控制器的类型。</typeparam>
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
public partial class Parser<T, TController>
	where T : struct
	where TController : ParserController<T>, new()
{
	/// <summary>
	/// 从 <see cref="int"/> 到 <c>T</c> 的转换器。
	/// </summary>
	private static readonly Converter<int, T> kindConverter = GenericConvert.GetConverter<int, T>()!;
	/// <summary>
	/// 下一个临时非终结符的索引。
	/// </summary>
	/// <remarks><c>-1</c> 已被占用，表示 ERROR。</remarks>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int tempSymbolIndex = -2;
	/// <summary>
	/// 符号字典。
	/// </summary>
	private readonly Dictionary<T, Symbol<T>> symbols = new();
	/// <summary>
	/// 符号名称字典。
	/// </summary>
	private readonly Dictionary<string, Symbol<T>> symbolNames = new();
	/// <summary>
	/// 起始符号集合。
	/// </summary>
	private readonly List<StartSymbol<T>> startSymbols = new();
	/// <summary>
	/// 产生式列表。
	/// </summary>
	private readonly List<Production<T>> productions = new();
	/// <summary>
	/// 终结符的结合性信息。
	/// </summary>
	private readonly Dictionary<T, Associativity> associativities = new();
	/// <summary>
	/// 定义的终结符的优先级。
	/// </summary>
	private int priority = 0;

	/// <summary>
	/// 将指定非终结符标标记为开始符号。
	/// </summary>
	/// <param name="kind">要标记的开始符号。</param>
	/// <param name="option">指定开始符号的分析选项。</param>
	public void AddStart(T kind, ParseOption option = ParseOption.ScanToEOF)
	{
		Symbol<T> start = GetOrCreateSymbol(kind, SymbolType.NonTerminal);
		start.StartType = SymbolStartType.Start;
		// 添加增广文法
		Symbol<T> augmentedStartSymbol = CreateTempSymbol($"{kind}'");
		// 扫描到匹配时，需要使用低优先级的 Accept 动作，确保能够被其它动作覆盖。
		augmentedStartSymbol.StartType = option == ParseOption.ScanToMatch ?
			SymbolStartType.AugmentedStartLowPriority : SymbolStartType.AugmentedStartHighPriority;
		startSymbols.Add(new StartSymbol<T>(kind, augmentedStartSymbol, option));
		Production<T> production = new(productions.Count, augmentedStartSymbol, new Symbol<T>[] { start });
		augmentedStartSymbol.Productions.Add(production);
		productions.Add(production);
	}

	/// <summary>
	/// 定义指定的产生式。
	/// </summary>
	/// <param name="kind">产生式所属的非终结符。</param>
	/// <param name="body">产生式的内容。</param>
	/// <returns>产生式的构造器。</returns>
	public ProductionBuilder<T, TController> DefineProduction(T kind, params Variant<T, SymbolOption>[] body)
	{
		Symbol<T> head = GetOrCreateSymbol(kind, SymbolType.NonTerminal);
		List<Symbol<T>> symbols = new();
		SymbolOption option = SymbolOption.None;
		foreach (var item in body)
		{
			if (item.TryGetValue(out T itemType))
			{
				if (symbols.Count > 0)
				{
					symbols[^1] = ApplyOption(symbols[^1], option);
				}
				symbols.Add(GetOrCreateSymbol(itemType));
				option = SymbolOption.None;
			}
			else
			{
				option = MergeOption(option, (SymbolOption)item);
			}
		}
		if (symbols.Count > 0)
		{
			symbols[^1] = ApplyOption(symbols[^1], option);
		}
		return new ProductionBuilder<T, TController>(this, DefineProduction(head, symbols.ToArray()));
	}

	/// <summary>
	/// 定义具有指定结合性的终结符集合，定义越晚优先级越高。
	/// </summary>
	/// <param name="type">终结符集合的结合性。</param>
	/// <param name="symbols">具有相同结合性的终结符的集合。</param>
	public void DefineAssociativity(AssociativeType type, params T[] symbols)
	{
		Associativity associativity = new(priority++, type);
		foreach (T symbol in symbols)
		{
			associativities.Add(symbol, associativity);
		}
	}

	/// <summary>
	/// 返回指定标识符类型的符号，如果不存在则创建新符号。
	/// </summary>
	/// <param name="kind">标识符类型。</param>
	/// <param name="type">如果符号不存在，那么使用指定的类型初始化符号。</param>
	/// <returns>指定标识符类型的符号。</returns>
	internal Symbol<T> GetOrCreateSymbol(T kind, SymbolType type = SymbolType.Unknown)
	{
		if (symbols.TryGetValue(kind, out Symbol<T>? symbol))
		{
			if (symbol.Type == SymbolType.Unknown)
			{
				symbol.Type = type;
			}
		}
		else
		{
			symbol = new Symbol<T>(kind, kind.ToString()!) { Type = type };
			symbols[kind] = symbol;
			// 允许按照类型名称查找符号。
			symbolNames[symbol.Name] = symbol;
		}
		return symbol;
	}

	/// <summary>
	/// 创建新的临时非终结符。
	/// </summary>
	/// <param name="name">临时非终结符的名称。</param>
	/// <returns>指定标识符类型的符号。</returns>
	private Symbol<T> CreateTempSymbol(string name)
	{
		Symbol<T> symbol = new(kindConverter(tempSymbolIndex), name) { Type = SymbolType.NonTerminal };
		tempSymbolIndex--;
		symbols[symbol.Kind] = symbol;
		return symbol;
	}

	/// <summary>
	/// 定义指定的产生式。
	/// </summary>
	/// <param name="head">产生式的头。</param>
	/// <param name="body">产生式的内容。</param>
	/// <returns>定义的产生式。</returns>
	private Production<T> DefineProduction(Symbol<T> head, Symbol<T>[] body)
	{
		Production<T> production = new(productions.Count, head, body);
		head.Productions.Add(production);
		productions.Add(production);
		return production;
	}

	/// <summary>
	/// 应用指定的选项。
	/// </summary>
	/// <param name="symbol">要应用选项的符号。</param>
	/// <param name="option">符号的选项。</param>
	/// <returns>应用选项后的符号。</returns>
	private Symbol<T> ApplyOption(Symbol<T> symbol, SymbolOption option)
	{
		switch (option)
		{
			case SymbolOption.Optional:
				{
					// 生成 h -> [] 和 h -> [ symbol ] 两条产生式
					Symbol<T> head = CreateTempSymbol($"{symbol.Name}?");
					DefineProduction(head, Array.Empty<Symbol<T>>())
						.Action = ProductionAction.Optional;
					DefineProduction(head, new Symbol<T>[] { symbol })
						.Action = ProductionAction.Optional;
					return head;
				}
			case SymbolOption.ZeroOrMore:
				{
					// 生成 h -> []、h -> [ symbol ] 和 h -> [ h, symbol ] 三条产生式
					Symbol<T> head = CreateTempSymbol($"{symbol.Name}*");
					DefineProduction(head, Array.Empty<Symbol<T>>())
						.Action = ProductionAction.More;
					DefineProduction(head, new Symbol<T>[] { symbol })
						.Action = ProductionAction.More;
					DefineProduction(head, new Symbol<T>[] { head, symbol })
						.Action = ProductionAction.More;
					return head;
				}
			case SymbolOption.OneOrMore:
				{
					// 生成 h -> [ symbol ] 和 h -> [ h, symbol ] 两条产生式
					Symbol<T> head = CreateTempSymbol($"{symbol.Name}+");
					DefineProduction(head, new Symbol<T>[] { symbol })
						.Action = ProductionAction.More;
					DefineProduction(head, new Symbol<T>[] { head, symbol })
						.Action = ProductionAction.More;
					return head;
				}
			default:
				return symbol;
		}
	}

	/// <summary>
	/// 合并指定的符号选项。
	/// </summary>
	/// <param name="last">上一个符号选项。</param>
	/// <param name="other">当前符号选项。</param>
	/// <returns>合并后的符号选项。</returns>
	private static SymbolOption MergeOption(SymbolOption last, SymbolOption other)
	{
		if (last == other)
		{
			return last;
		}
		return last switch
		{
			SymbolOption.None => other,
			SymbolOption.ZeroOrMore => SymbolOption.ZeroOrMore,
			_ => other == SymbolOption.None ? last : SymbolOption.ZeroOrMore,
		};
	}
}
