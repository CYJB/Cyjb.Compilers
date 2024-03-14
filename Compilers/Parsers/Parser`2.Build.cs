using System.Text;
using Cyjb.Collections;

namespace Cyjb.Compilers.Parsers;

public partial class Parser<T, TController>
{
	/// <summary>
	/// 语法分析器的状态列表。
	/// </summary>
	private readonly List<LRState<T>> states = new();
	/// <summary>
	/// 非终结符号的 FIRST 集。
	/// </summary>
	private readonly Dictionary<Symbol<T>, HashSet<Symbol<T>>> firstSet = new();
	/// <summary>
	/// 非终结符集合。
	/// </summary>
	private IEnumerable<Symbol<T>> NonTerminals => symbols.Values
		.Where((symbol) => symbol.Type == SymbolType.NonTerminal);
	/// <summary>
	/// 终结符集合。
	/// </summary>
	private IEnumerable<Symbol<T>> Terminals => symbols.Values
		.Where((symbol) => symbol.Type == SymbolType.Terminal);

	/// <summary>
	/// 比较产生式和终结符的优先级。
	/// </summary>
	/// <param name="production">要比较的产生式。</param>
	/// <param name="symbol">要比较的终结符。</param>
	/// <returns>如果产生式优先级较高，则为 <c>1</c>；
	/// 如果终结符优先级较高，则为 <c>-1</c>；如果未定义则为 <c>0</c>。
	/// 如果优先级相同，左结合则为 <c>1</c>，右结合则为 <c>-1</c>，非结合则为 <c>0</c>。
	/// </returns>
	internal int CompareAssociativity(Production<T> production, Symbol<T> symbol)
	{
		Symbol<T>? prec = production.Precedence;
		if (prec == null)
		{
			return 0;
		}
		return CompareAssociativity(prec.Kind, symbol.Kind);
	}

	/// <summary>
	/// 比较两个产生式的优先级。
	/// </summary>
	/// <param name="leftProduction">要比较的第一个产生式。</param>
	/// <param name="rightProduction">要比较的第二个产生式。</param>
	/// <returns>如果第一个产生式优先级较高，则为 <c>1</c>；
	/// 如果第二个产生式优先级较高，则为 <c>-1</c>；如果未定义则为 <c>0</c>。
	/// 如果优先级相同，左结合则为 <c>1</c>，右结合则为 <c>-1</c>，非结合则为 <c>0</c>。
	/// </returns>
	internal int CompareAssociativity(Production<T> leftProduction, Production<T> rightProduction)
	{
		Symbol<T>? leftPrec = leftProduction.Precedence;
		Symbol<T>? rightPrec = rightProduction.Precedence;
		if (leftPrec == null || rightPrec == null)
		{
			return 0;
		}
		else
		{
			return CompareAssociativity(leftPrec.Kind, rightPrec.Kind);
		}
	}

	/// <summary>
	/// 比较两个终结符的优先级。
	/// </summary>
	/// <param name="left">要比较的第一个终结符。</param>
	/// <param name="right">要比较的第二个终结符。</param>
	/// <returns>如果第一个终结符优先级较高，则为 <c>1</c>；
	/// 如果第二个终结符优先级较高，则为 <c>-1</c>；如果未定义则为 <c>0</c>。
	/// 如果优先级相同，左结合则为 <c>1</c>，右结合则为 <c>-1</c>，非结合则为 <c>0</c>。
	/// </returns>
	private int CompareAssociativity(T left, T right)
	{
		if (associativities.TryGetValue(left, out Associativity? leftAssoc) &&
			associativities.TryGetValue(right, out Associativity? rightAssic))
		{
			return leftAssoc.CompareTo(rightAssic);
		}
		return 0;
	}

	/// <summary>
	/// 返回语法分析器的数据。
	/// </summary>
	/// <returns>语法分析器的数据。</returns>
	public ParserData<T> GetData()
	{
		// 将所有未知的符号填充为终结符。
		foreach (Symbol<T> symbol in symbols.Values)
		{
			if (symbol.Type == SymbolType.Unknown)
			{
				symbol.Type = SymbolType.Terminal;
			}
		}
		// 如果未指定起始符号，那么将首条产生式的头作为起始符号。
		if (startSymbols.Count == 0)
		{
			AddStart(firstProduction!.Head.Kind);
		}
		// 检查产生式。
		foreach (Production<T> production in productions)
		{
			Symbol<T>? prec = production.Precedence;
			if (prec == null)
			{
				// 使用最右的终结符代表产生式的结合性。
				for (int i = production.Body.Length - 1; i >= 0; i--)
				{
					Symbol<T> symbol = production.Body[i];
					if (symbol.Type == SymbolType.Terminal)
					{
						production.Precedence = symbol;
						break;
					}
				}
			}
			else if (prec.Type != SymbolType.Terminal)
			{
				throw CompilerExceptions.PrecedenceMustBeTerminal(prec.Name);
			}
		}
		BuildLR0States();
		FillFirstSet();
		SpreadForward();
		BuildLRTable();

		// 提取起始状态。
		Dictionary<T, int> startStates = new();
		for (int i = 0; i < startSymbols.Count; i++)
		{
			startStates[startSymbols[i].Kind] = i;
		}
		// 提取产生式数据
		List<ProductionData<T>> productionData = new();
		foreach (Production<T> production in productions)
		{
			productionData.Add(production.GetData());
		}
		// 提取状态数据
		List<ParserStateData<T>> states = new();
		Dictionary<T, List<KeyValuePair<int, int>>> gotoTransitions = new();
		int stateCount = this.states.Count;
		foreach (LRState<T> state in this.states)
		{
			LRItem<T> recoverItem = state.GetRecoverItem();
			states.Add(new ParserStateData<T>(state.Actions.GetActions(),
				state.Actions.GetDefaultAction(), state.Actions.GetExpecting(),
				productionData[recoverItem.Production.Index], recoverItem.Index));
			// 提取 GOTO 信息
			foreach (var (symbol, nextState) in state.Gotos)
			{
				if (!gotoTransitions.TryGetValue(symbol.Kind, out List<KeyValuePair<int, int>>? list))
				{
					list = new List<KeyValuePair<int, int>>(stateCount);
					gotoTransitions[symbol.Kind] = list;
				}
				list.Add(new KeyValuePair<int, int>(state.Index, nextState.Index));
			}
		}
		TripleArrayCompress<T, int> gotoCompress = new(Symbol<T>.EndOfFile.Kind, ParserData.InvalidState);
		Dictionary<T, int> gotoMap = new();
		foreach (var (kind, transition) in gotoTransitions)
		{
			gotoMap[kind] = gotoCompress.AddNode(kind, transition);
		}
		return new ParserData<T>(productionData.ToArray(), startStates, states.ToArray(),
			gotoMap, gotoCompress.Next.ToArray(), gotoCompress.Check.ToArray());
	}

	/// <summary>
	/// 返回语法分析的工厂。
	/// </summary>
	/// <returns>语法分析器的工厂。</returns>
	public IParserFactory<T> GetFactory()
	{
		ParserData<T> data = GetData();
		return new ParserFactory<T, TController>(data);
	}

	/// <summary>
	/// 返回最近一次构造的词法分析器的状态描述信息。
	/// </summary>
	/// <param name="index">要获取描述信息的状态索引。</param>
	/// <returns>最近一次构造的词法分析器的状态描述信息。</returns>
	public string GetStateDescription(int index)
	{
		if (index >= states.Count)
		{
			return string.Empty;
		}
		StringBuilder text = new();
		text.Append(index);
		text.Append(": ");
		LRState<T> state = states[index];
		// 输出状态的核心项
		string prefix = new(' ', text.Length);
		bool isFirst = true;
		foreach (string line in state.ToString().Split(Environment.NewLine))
		{
			if (isFirst)
			{
				isFirst = false;
			}
			else
			{
				text.Append(prefix);
			}
			text.AppendLine(line);
		}
		// 输出状态的动作
		bool needNewLine = false;
		foreach (Symbol<T> symbol in Terminals.OrderBy((symbol) => symbol.Name).Append(Symbol<T>.EndOfFile))
		{
			if (state.Actions.TryGetValue(symbol, out Action? action))
			{
				needNewLine = true;
				text.Append(prefix);
				text.Append(symbol.Name);
				text.Append(" -> ");
				text.Append(action);
				text.AppendLine();
				if (state.Conflicts.TryGetValue(symbol, out ParserConflict<T>? conflict))
				{
					text.Append(prefix);
					text.Append("    ");
					text.AppendLine("conflict:");
					text.Append(prefix);
					text.Append("    ");
					text.AppendLine(conflict.Selected);
					foreach (string item in conflict)
					{
						text.Append(prefix);
						text.Append("    ");
						text.AppendLine(item);
					}
				}
			}
		}
		// 输出状态的转移
		foreach (Symbol<T> symbol in NonTerminals.OrderBy((symbol) => symbol.Name))
		{
			if (state.Gotos.TryGetValue(symbol, out LRState<T>? nextState))
			{
				if (needNewLine)
				{
					text.AppendLine();
					needNewLine = false;
				}
				text.Append(prefix);
				text.Append(symbol.Name);
				text.Append(" -> ");
				text.AppendLine(nextState.Index.ToString());
			}
		}
		return text.ToString();
	}

	#region LR(0) 状态列表

	/// <summary>
	/// 构建 LR(0) 状态列表。
	/// </summary>
	private void BuildLR0States()
	{
		// LR(0) 状态列表（现在只包含核心项）。
		states.Clear();
		// LR(0) 项集的字典。
		Dictionary<LRItemCollection<T>, LRState<T>> itemsMap = new();
		// 添加起始符号的产生式。
		foreach (StartSymbol<T> symbol in startSymbols)
		{
			LRItemCollection<T> items = new()
			{
				{ symbol.AugmentedStartSymbol.Productions[0], 0 }
			};
			LRState<T> state = new(states.Count, items);
			states.Add(state);
			itemsMap[state.Items] = state;
		}
		// 状态的 GOTO 函数。
		Dictionary<Symbol<T>, LRItemCollection<T>> gotos = new();
		for (int i = 0; i < states.Count; i++)
		{
			LRState<T> state = states[i];
			// 在计算 GOTO 之前计算闭包。
			LRItemCollection<T> closure = LR0Closure(state.Items);
			gotos.Clear();
			foreach (LRItem<T> tmpItem in closure)
			{
				// 仅对定点右边有符号的项进行处理。
				Symbol<T>? nextSymbol = tmpItem.SymbolAtIndex;
				if (nextSymbol == null)
				{
					continue;
				}
				if (!gotos.TryGetValue(nextSymbol, out LRItemCollection<T>? collection))
				{
					collection = new LRItemCollection<T>();
					gotos.Add(nextSymbol, collection);
				}
				collection.Add(tmpItem.Production, tmpItem.Index + 1);
			}
			// 设置状态的 GOTO 函数。
			foreach (var (symbol, items) in gotos)
			{
				// 用于识别项的集合。
				if (!itemsMap.TryGetValue(items, out LRState<T>? nextState))
				{
					nextState = new LRState<T>(states.Count, items);
					states.Add(nextState);
					itemsMap[items] = nextState;
				}
				state.Gotos[symbol] = nextState;
			}
		}
	}

	/// <summary>
	/// 计算指定 LR(0) 项集的闭包。
	/// </summary>
	/// <param name="items">要计算闭包的项集。</param>
	/// <returns>计算得到的闭包。</returns>
	private static LRItemCollection<T> LR0Closure(IEnumerable<LRItem<T>> items)
	{
		LRItemCollection<T> list = new(items);
		for (int i = 0; i < list.Count; i++)
		{
			Symbol<T>? nextNonTerminal = list[i].SymbolAtIndex;
			if (nextNonTerminal == null || nextNonTerminal.Type != SymbolType.NonTerminal)
			{
				continue;
			}
			foreach (Production<T> production in nextNonTerminal.Productions)
			{
				list.Add(production, 0);
			}
		}
		return list;
	}

	#endregion // LR(0) 状态列表

	#region 计算 FIRST

	/// <summary>
	/// 计算符号的 FIRST 集。
	/// </summary>
	/// <remarks>非终结符的 FIRST 集是可以从其推导得到的串的首符号的集合；
	/// 终结符的 FIRST 集只包含其本身。</remarks>
	private void FillFirstSet()
	{
		firstSet.Clear();
		foreach (Symbol<T> symbol in symbols.Values)
		{
			if (symbol.Type == SymbolType.Terminal)
			{
				// 终结符的 FIRST 集只包含其自身。
				firstSet.Add(symbol, new HashSet<Symbol<T>>() { symbol });
			}
			else
			{
				// 非终结符的 FIRST 集需要稍后计算。
				firstSet.Add(symbol, new HashSet<Symbol<T>>());
			}
		}
		bool changed = true;
		while (changed)
		{
			changed = false;
			foreach (Symbol<T> symbol in NonTerminals)
			{
				HashSet<Symbol<T>> first = firstSet[symbol];
				int oldCnt = first.Count;
				foreach (Production<T> production in symbol.Productions)
				{
					bool hasEmpty = true;
					foreach (Symbol<T> sym in production.Body)
					{
						// 添加后继的 FIRST 集。
						HashSet<Symbol<T>> nextFirst = firstSet[sym];
						first.UnionWith(nextFirst);
						if (!nextFirst.Contains(Symbol<T>.Epsilon))
						{
							hasEmpty = false;
							break;
						}
					}
					if (hasEmpty)
					{
						// X 可以推导出 ε。
						first.Add(Symbol<T>.Epsilon);
					}
				}
				if (first.Count > oldCnt)
				{
					changed = true;
				}
			}
		}
	}

	#endregion // 计算 FIRST

	#region LALR 项集族

	/// <summary>
	/// 对向前看符号进行传播，生成 LALR 状态列表。
	/// </summary>
	private void SpreadForward()
	{
		// LALR 向前看符号的传播规则。
		List<Tuple<LRItem<T>, LRItem<T>>> forwardSpread = new();
		foreach (LRState<T> state in states)
		{
			// 计算向前看符号的传播和自发生。
			foreach (LRItem<T> item in state.Items)
			{
				// 为 item 临时添加传递标志。
				item.Forwards.Add(Symbol<T>.Spread);
				// 当前项的集合。
				LRItemCollection<T> closure = new() { item };
				closure.CalculateLR1Closure(firstSet);
				foreach (LRItem<T> closureItem in closure)
				{
					// 仅对定点右边有符号的项进行处理。
					Symbol<T>? symbol = closureItem.SymbolAtIndex;
					if (symbol == null)
					{
						continue;
					}
					LRItemCollection<T> gotoTarget = state.Gotos[symbol].Items;
					LRItem<T> gotoItem = gotoTarget.AddOrGet(closureItem.Production, closureItem.Index + 1);
					// 自发生成的向前看符号。
					gotoItem.AddForwards(closureItem.Forwards);
					// 传播的向前看符号。
					if (gotoItem.Forwards.Contains(Symbol<T>.Spread))
					{
						forwardSpread.Add(new Tuple<LRItem<T>, LRItem<T>>(item, gotoItem));
						gotoItem.Forwards.Remove(Symbol<T>.Spread);
					}
				}
				// 移除 item 的传递标志。
				item.Forwards.Remove(Symbol<T>.Spread);
			}
		}
		// 为起始符号添加向前看符号 EOF
		int startSymbolCount = startSymbols.Count;
		for (int i = 0; i < startSymbolCount; i++)
		{
			LRItem<T> item = states[i].Items[0];
			switch (startSymbols[i].Option)
			{
				case ParseOptions.ScanToEOF:
					// 扫描到 EOF 时，只添加 EOF 即可。
					item.Forwards.Add(Symbol<T>.EndOfFile);
					break;
				default:
					// 扫描到匹配时，添加所有终结符。
					item.Forwards.Add(Symbol<T>.EndOfFile);
					item.Forwards.UnionWith(Terminals);
					break;
			}
		}
		// 需要继续传播向前看符号。
		bool continueSpread = true;
		while (continueSpread)
		{
			continueSpread = false;
			foreach (Tuple<LRItem<T>, LRItem<T>> tuple in forwardSpread)
			{
				int oldCnt = tuple.Item2.Forwards.Count;
				tuple.Item2.Forwards.UnionWith(tuple.Item1.Forwards);
				if (oldCnt < tuple.Item2.Forwards.Count)
				{
					continueSpread = true;
				}
			}
		}
		// 根据核心项构造完整项集。
		foreach (LRState<T> state in states)
		{
			state.CoreItemCount = state.Items.Count;
			state.Items.CalculateLR1Closure(firstSet);
		}
	}

	#endregion // LALR 项集族

	#region LR 语法分析表

	/// <summary>
	/// 构造 LR 语法分析表。
	/// </summary>
	private void BuildLRTable()
	{
		Dictionary<Symbol<T>, List<CandidateAction<T>>> actions = new();
		foreach (LRState<T> state in states)
		{
			actions.Clear();
			// 填充动作列表
			foreach (LRItem<T> item in state.Items)
			{
				if (item.Index < item.Production.Body.Length)
				{
					// 移入动作。
					Symbol<T> sym = item.Production.Body[item.Index];
					if (sym.Type == SymbolType.Terminal)
					{
						AddAction(actions, sym, item, new ShiftAction<T>(state.Gotos[sym]));
					}
				}
				else if (item.IsAccept)
				{
					AcceptAction action = item.IsHighPriorityAccept ?
						AcceptAction.HighPriority : AcceptAction.LowPriority;
					foreach (Symbol<T> symbol in item.Forwards)
					{
						AddAction(actions, symbol, item, action);
					}
				}
				else
				{
					// 归约动作。
					ReduceAction<T> action = new(item.Production);
					foreach (Symbol<T> symbol in item.Forwards)
					{
						AddAction(actions, symbol, item, action);
					}
				}
			}
			ResolveActions(state, actions);
		}
	}

	/// <summary>
	/// 添加指定终结符的动作。
	/// </summary>
	/// <param name="actions">要添加到的动作列表。</param>
	/// <param name="symbol">动作关联到的终结符。</param>
	/// <param name="item">动作关联到的 LR 项。</param>
	/// <param name="action">分析器动作。</param>
	private static void AddAction(Dictionary<Symbol<T>, List<CandidateAction<T>>> actions,
		Symbol<T> symbol, LRItem<T> item, Action action)
	{
		if (!actions.TryGetValue(symbol, out List<CandidateAction<T>>? list))
		{
			list = new List<CandidateAction<T>>();
			actions[symbol] = list;
		}
		list.Add(new CandidateAction<T>(item, action));
	}

	/// <summary>
	/// 分析并尝试解决动作中的冲突。
	/// </summary>
	/// <param name="state">当前状态。</param>
	/// <param name="actions">动作列表。</param>
	/// <returns>解决冲突后的动作列表。</returns>
	private ActionCollection<T> ResolveActions(LRState<T> state,
		Dictionary<Symbol<T>, List<CandidateAction<T>>> actions)
	{
		ActionCollection<T> result = state.Actions;
		foreach (var (symbol, list) in actions)
		{
			int cnt = list.Count;
			if (cnt == 0)
			{
				continue;
			}
			if (cnt == 1)
			{
				result.Add(symbol, list[0].Action);
				continue;
			}
			// 当前的终结符。
			ParserConflict<T> conflict = new(state, symbol, list[0]);
			Action action = list.Aggregate((CandidateAction<T> item, CandidateAction<T> next) =>
			{
				// 接受动作的优先级最高。
				if (item.Action is AcceptAction acceptAction1)
				{
					return acceptAction1.IsHighPriority ? item : next;
				}
				else if (next.Action is AcceptAction acceptAction2)
				{
					return acceptAction2.IsHighPriority ? next : item;
				}
				else if (item.Action is ReduceAction<T> reduceAction1)
				{
					if (next.Action is ShiftAction<T> shiftAction2)
					{
						int cmp = CompareAssociativity(reduceAction1.Production, symbol);
						// 冲突时采用移入。
						if (cmp == 0)
						{
							conflict.SetSelected(next);
							return next;
						}
						else if (cmp == -1)
						{
							conflict.Replace(next);
							return next;
						}
					}
					else
					{
						ReduceAction<T> reduceAction2 = (ReduceAction<T>)next.Action;
						int cmp = CompareAssociativity(reduceAction1.Production, next.Item.Production);
						if (cmp == -1)
						{
							conflict.Replace(next);
							return next;
						}
						else if (cmp == 0)
						{
							// 冲突时采用标号较小的规则。
							if (reduceAction1.Production.Index <= reduceAction2.Production.Index)
							{
								conflict.Add(next);
							}
							else
							{
								conflict.SetSelected(next);
								return next;
							}
						}
					}
				}
				else if (next.Action is ReduceAction<T> reduceAction2)
				{
					int cmp = CompareAssociativity(reduceAction2.Production, symbol);
					if (cmp == 1)
					{
						conflict.Replace(next);
						return next;
					}
					else if (cmp == 0)
					{
						// 冲突时采用移入。
						conflict.Add(next);
					}
				}
				else
				{
					// 不存在 shift/shift 冲突
				}
				return item;
			}).Action;
			result.Add(symbol, action);
			if (conflict.Count > 0)
			{
				state.Conflicts[symbol] = conflict;
			}
		}
		return result;
	}

	#endregion // LR 语法分析表

}
