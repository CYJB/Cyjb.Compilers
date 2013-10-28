using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cyjb.Text;

namespace Cyjb.Compiler.Parser
{
	/// <summary>
	/// 表示 LALR 语法分析器的规则。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	[Serializable]
	public sealed class ParserRule<T>
		where T : struct
	{
		/// <summary>
		/// 表示文件结束的终结符。
		/// </summary>
		private static readonly Terminal<T> EndOfFile = new Terminal<T>(Token<T>.EndOfFile, -1, null, null);
		/// <summary>
		/// 表示空串的终结符。
		/// </summary>
		private static readonly Terminal<T> Empty = new Terminal<T>(Symbol<T>.Invalid, -1, null, null);
		/// <summary>
		/// 表示会被传递的终结符。
		/// </summary>
		private static readonly Terminal<T> SpreadFlag = new Terminal<T>(Symbol<T>.Invalid, -2, null, null);
		/// <summary>
		/// LR 语法分析表的状态列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private StateData[] states;
		/// <summary>
		/// LR 语法分析表的动作长度。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int actionCount;
		/// <summary>
		/// LR 语法产生式列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[NonSerialized]
		private Production<T>[] productions;
		/// <summary>
		/// 非终结符的索引偏移量。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int nonTerminalOffset;
		/// <summary>
		/// LR 语法产生式数据列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ProductionData<T>[] productionData;
		/// <summary>
		/// LR 语法中的冲突列表。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<ParserConflict> conflicts = new List<ParserConflict>();
		/// <summary>
		/// 使用指定的语法初始化 <see cref="Cyjb.Compiler.Parser.ParserRule&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="grammar">词法分析器使用的语法。</param>
		/// <param name="offset">非终结符的索引偏移量。</param>
		/// <param name="actionCount">LR 语法分析表的动作长度。</param>
		internal ParserRule(Grammar<T> grammar, int offset, int actionCount)
		{
			this.nonTerminalOffset = offset;
			this.actionCount = actionCount;
			FillProductions(grammar);
			List<LRItemset<T>> itemsets = BuildLR0Itemsets(grammar);
			Dictionary<Symbol<T>, HashSet<Terminal<T>>> first = CalFirst(grammar);
			SpreadForward(grammar, itemsets, first);
			BuildLRTable(grammar, itemsets);
		}
		/// <summary>
		/// 获取 LR 语法分析表的状态列表。
		/// </summary>
		/// <value>LR 语法分析表的状态列表。</value>
		public IList<StateData> States { get { return states; } }
		/// <summary>
		/// 获取 LR 语法分析表的动作长度。
		/// </summary>
		/// <value>LR 语法分析表的动作长度，等于终结符的数量加 <c>2</c>。</value>
		public int ActionCount { get { return actionCount; } }
		/// <summary>
		/// 获取 LR 语法产生式数据列表。
		/// </summary>
		/// <value>LR 语法产生式数据列表</value>
		public IList<ProductionData<T>> Productions { get { return productionData; } }
		/// <summary>
		/// 获取非终结符的索引偏移量。
		/// </summary>
		/// <value>非终结符的索引偏移量，大于等于该值的标识符，对应的都是非终结符；
		/// 大于等于零，小于该值的标识符，都认为是终结符。</value>
		public int NonTerminalOffset { get { return nonTerminalOffset; } }
		/// <summary>
		/// 获取 LR 语法中的冲突列表。
		/// </summary>
		/// <value>LR 语法中的冲突列表。</value>
		public IList<ParserConflict> Conflicts { get { return conflicts; } }
		/// <summary>
		/// 填充产生式信息。
		/// </summary>
		/// <param name="grammar">语法分析器使用的语法。</param>
		private void FillProductions(Grammar<T> grammar)
		{
			this.productions = new Production<T>[grammar.ProductionCount];
			this.productionData = new ProductionData<T>[grammar.ProductionCount];
			foreach (NonTerminal<T> symbol in grammar.NonTerminals)
			{
				int cnt = symbol.Productions.Count;
				for (int i = 0; i < cnt; i++)
				{
					Production<T> production = symbol.Productions[i];
					this.productions[production.Index] = production;
					this.productionData[production.Index] =
						new ProductionData<T>(production.Head.Index, production.Body.Count, production.Action);
				}
			}
		}

		#region LR(0) 项集族

		/// <summary>
		/// 构建 LR(0) 项集族。
		/// </summary>
		/// <param name="grammar">语法规则。</param>
		private static List<LRItemset<T>> BuildLR0Itemsets(Grammar<T> grammar)
		{
			// LR(0) 项集族（现在只包含核心项）。
			List<LRItemset<T>> itemsets = new List<LRItemset<T>>();
			// LR(0) 项集的索引字典。
			Dictionary<LRItemCollection<T>, int> itemsIndexMap = new Dictionary<LRItemCollection<T>, int>();
			// 从增广文法的起始符号的产生式开始遍历。
			itemsets.Add(new LRItemset<T>());
			// 添加初始项集。
			itemsets[0].Items.Add(grammar.AugmentedStart.Productions[0], 0);
			itemsIndexMap.Add(itemsets[0].Items, 0);
			// 项集的临时 GOTO 函数。
			Dictionary<Symbol<T>, LRItemCollection<T>> tempGoto = new Dictionary<Symbol<T>, LRItemCollection<T>>();
			for (int i = 0; i < itemsets.Count; i++)
			{
				LRItemset<T> itemset = itemsets[i];
				// 在计算 GOTO 之前计算闭包。
				LRItemCollection<T> closure = LR0Closure(itemset.Items);
				tempGoto.Clear();
				foreach (LRItem<T> tmpItem in closure)
				{
					// 仅对定点右边有符号的项进行处理。
					Symbol<T> sym = tmpItem.SymbolAtIndex;
					if (sym == null)
					{
						continue;
					}
					LRItemCollection<T> collection = null;
					if (!tempGoto.TryGetValue(sym, out collection))
					{
						collection = new LRItemCollection<T>();
						tempGoto.Add(sym, collection);
					}
					collection.Add(tmpItem.Production, tmpItem.Index + 1);
				}
				// 设置项集族的 GOTO 函数。
				foreach (KeyValuePair<Symbol<T>, LRItemCollection<T>> pair in tempGoto)
				{
					// 用于识别项的集合。
					LRItemCollection<T> items = pair.Value;
					int index;
					if (!itemsIndexMap.TryGetValue(items, out index))
					{
						index = itemsets.Count;
						itemsets.Add(new LRItemset<T>(pair.Value));
						itemsIndexMap.Add(items, index);
					}
					itemset.Goto.Add(pair.Key, index);
				}
			}
			return itemsets;
		}
		/// <summary>
		/// 计算指定 LR(0) 项集的闭包。
		/// </summary>
		/// <param name="items">要计算闭包的项集。</param>
		/// <returns>计算得到的闭包。</returns>
		private static LRItemCollection<T> LR0Closure(IEnumerable<LRItem<T>> items)
		{
			LRItemCollection<T> list = new LRItemCollection<T>(items);
			for (int i = 0; i < list.Count; i++)
			{
				NonTerminal<T> nextSym = list[i].SymbolAtIndex as NonTerminal<T>;
				if (nextSym == null)
				{
					continue;
				}
				int cnt = nextSym.Productions.Count;
				for (int j = 0; j < cnt; j++)
				{
					list.Add(nextSym.Productions[j], 0);
				}
			}
			return list;
		}

		#endregion // LR(0) 项集族

		#region 计算 FIRST

		/// <summary>
		/// 计算非终结符的 FIRST 集。
		/// </summary>
		/// <param name="grammar">语法规则。</param>
		/// <returns>计算得到的 FIRST 集。</returns>
		/// <remarks>非终结符的 FIRST 集是可以从其推导得到的串的首符号的集合；
		/// 终结符的 FIRST 集只包含其本身。</remarks>
		private static Dictionary<Symbol<T>, HashSet<Terminal<T>>> CalFirst(Grammar<T> grammar)
		{
			Dictionary<Symbol<T>, HashSet<Terminal<T>>> first = new Dictionary<Symbol<T>, HashSet<Terminal<T>>>();
			// 循环计算非终结符的 FIRST 集。
			foreach (NonTerminal<T> sym in grammar.NonTerminals)
			{
				first.Add(sym, new HashSet<Terminal<T>>());
			}
			// 是否需要接着循环。
			bool needLoop = true;
			while (needLoop)
			{
				needLoop = false;
				foreach (NonTerminal<T> sym in grammar.NonTerminals)
				{
					if (CalFirst(sym, first))
					{
						needLoop = true;
					}
				}
			}
			return first;
		}
		/// <summary>
		/// 计算指定非终结符的 FIRST 集。
		/// </summary>
		/// <param name="symbol">要计算 FIRST 集的非终结符。</param>
		/// <param name="first">已经计算了的 FIRST 集。</param>
		/// <returns>当前非终结符的 FIRST 集包含的元素是否增加了。</returns>
		private static bool CalFirst(NonTerminal<T> symbol, Dictionary<Symbol<T>, HashSet<Terminal<T>>> first)
		{
			HashSet<Terminal<T>> set = first[symbol];
			int oldCnt = set.Count;
			int cnt = symbol.Productions.Count;
			for (int i = 0; i < cnt; i++)
			{
				IList<Symbol<T>> body = symbol.Productions[i].Body;
				int bodyCnt = body.Count;
				// 产生式为 X->ε。
				if (bodyCnt == 0)
				{
					set.Add(Empty);
					continue;
				}
				int j = 0;
				for (; j < bodyCnt; j++)
				{
					Terminal<T> tSym = body[j] as Terminal<T>;
					if (tSym == null)
					{
						// 添加非终结符的 FIRST 集。
						HashSet<Terminal<T>> tmpFirst = first[body[j]];
						set.UnionWith(tmpFirst);
						if (!tmpFirst.Contains(Empty))
						{
							break;
						}
					}
					else
					{
						// 终结符的 FIRST 集就是它本身。
						set.Add(tSym);
						break;
					}
				}
				if (j == bodyCnt)
				{
					// X 可以推导出 ε。
					set.Add(Empty);
				}
			}
			return oldCnt < set.Count;
		}

		#endregion // FIRST

		#region LALR(1) 项集族

		/// <summary>
		/// 对向前看符号进行传播，生成 LALR(1) 项集族。
		/// </summary>
		/// <param name="grammar">语法规则。</param>
		/// <param name="itemsets">LR(0) 项集族。</param>
		/// <param name="first">非终结符号的 FIRST 集。</param>
		private static void SpreadForward(Grammar<T> grammar, List<LRItemset<T>> itemsets,
			Dictionary<Symbol<T>, HashSet<Terminal<T>>> first)
		{
			// LALR 向前看符号的传播规则。
			List<Tuple<LRItem<T>, LRItem<T>>> forwardSpread = new List<Tuple<LRItem<T>, LRItem<T>>>();
			int cnt = itemsets.Count;
			for (int i = 0; i < cnt; i++)
			{
				LRItemset<T> itemset = itemsets[i];
				// 计算向前看符号的传播和自发生。
				foreach (LRItem<T> item in itemset.Items)
				{
					// 为 item 临时添加传递标志。
					item.Forwards.Add(SpreadFlag);
					// 当前项的集合。
					LRItemCollection<T> closure = new LRItemCollection<T>() { item };
					LR1Closure(closure, first);
					foreach (LRItem<T> closureItem in closure)
					{
						// 仅对定点右边有符号的项进行处理。
						Symbol<T> sym = closureItem.SymbolAtIndex;
						if (sym == null)
						{
							continue;
						}
						LRItemCollection<T> gotoTarget = itemsets[itemset.Goto[sym]].Items;
						LRItem<T> gotoItem = gotoTarget.AddOrGet(closureItem.Production, closureItem.Index + 1);
						// 自发生成的向前看符号。
						gotoItem.Forwards.UnionWith(closureItem.Forwards);
						// 传播的向前看符号。
						if (gotoItem.Forwards.Contains(SpreadFlag))
						{
							forwardSpread.Add(new Tuple<LRItem<T>, LRItem<T>>(item, gotoItem));
							gotoItem.Forwards.Remove(SpreadFlag);
						}
					}
					// 移除 item 的传递标志。
					item.Forwards.Remove(SpreadFlag);
				}
			}
			// 为起始符号添加向前看符号 EOF
			itemsets[0].Items[0].Forwards.Add(EndOfFile);
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
			for (int i = 0; i < cnt; i++)
			{
				LR1Closure(itemsets[i].Items, first);
			}
		}
		/// <summary>
		/// 计算指定 LR(1) 项集的闭包。
		/// </summary>
		/// <param name="items">要计算闭包的项集。</param>
		/// <param name="first">非终结符号的 FIRST 集合。</param>
		private static void LR1Closure(LRItemCollection<T> items,
			Dictionary<Symbol<T>, HashSet<Terminal<T>>> first)
		{
			for (int i = 0; i < items.Count; i++)
			{
				LRItem<T> item = items[i];
				// 项的定点右边是非终结符。 
				NonTerminal<T> nextSym = item.SymbolAtIndex as NonTerminal<T>;
				if (nextSym == null)
				{
					continue;
				}
				int index = item.Index + 1;
				Production<T> production = item.Production;
				HashSet<Terminal<T>> forwards = null;
				if (index < production.Body.Count)
				{
					forwards = new HashSet<Terminal<T>>();
					Symbol<T> nNextSym = production.Body[index];
					Terminal<T> tSym = nNextSym as Terminal<T>;
					if (tSym != null)
					{
						forwards.Add(tSym);
					}
					else
					{
						forwards.UnionWith(first[nNextSym]);
						if (forwards.Contains(Empty))
						{
							// FIRST 中包含 ε，需要被替换为 item 的向前看符号。
							forwards.Remove(Empty);
							forwards.UnionWith(item.Forwards);
						}
					}
				}
				else
				{
					forwards = item.Forwards;
				}
				int pCnt = nextSym.Productions.Count;
				for (int j = 0; j < pCnt; j++)
				{
					LRItem<T> newItem = items.AddOrGet(nextSym.Productions[j], 0);
					newItem.Forwards.UnionWith(forwards);
				}
			}
		}

		#endregion // LALR(1) 项集族

		#region LALR 语法分析表

		/// <summary>
		/// 构造 LALR 语法分析表。
		/// </summary>
		/// <param name="grammar">语法分析器使用的语法。</param>
		/// <param name="itemset">LR 项集族。</param>
		private void BuildLRTable(Grammar<T> grammar, List<LRItemset<T>> itemset)
		{
			int stateCnt = itemset.Count;
			this.states = new StateData[stateCnt];
			int gotoCnt = grammar.NonTerminals.Count;
			for (int i = 0; i < stateCnt; i++)
			{
				List<Tuple<LRItem<T>, ParseAction>>[] actions = new List<Tuple<LRItem<T>, ParseAction>>[actionCount];
				for (int j = 0; j < actionCount; j++)
				{
					actions[j] = new List<Tuple<LRItem<T>, ParseAction>>();
				}
				foreach (LRItem<T> item in itemset[i].Items)
				{
					if (item.Index < item.Production.Body.Count)
					{
						// 移入动作。
						Symbol<T> sym = item.Production.Body[item.Index];
						Terminal<T> tSym = sym as Terminal<T>;
						if (tSym != null)
						{
							int targetIndex = itemset[i].Goto[tSym];
							actions[tSym.Index + Constants.TokenOffset].Add(
								new Tuple<LRItem<T>, ParseAction>(item, ParseAction.Shift(targetIndex)));
						}
					}
					else if (item.Production.Head != grammar.AugmentedStart)
					{
						// 归约动作。
						int targetIndex = item.Production.Index;
						foreach (Terminal<T> sym in item.Forwards)
						{
							actions[sym.Index + Constants.TokenOffset].Add(
								new Tuple<LRItem<T>, ParseAction>(item, ParseAction.Reduce(targetIndex)));
						}
					}
					else if (item.Forwards.Contains(EndOfFile) && actions[Constants.EOFIdx].Count == 0)
					{
						// 接受动作。
						actions[Constants.EOFIdx].Add(new Tuple<LRItem<T>, ParseAction>(item, ParseAction.Accept));
					}
				}
				// 生成 GOTO 转换。
				int[] gotos = new int[gotoCnt];
				foreach (NonTerminal<T> sym in grammar.NonTerminals)
				{
					int state;
					if (!itemset[i].Goto.TryGetValue(sym, out state))
					{
						state = Constants.DeadState;
					}
					gotos[sym.Index] = state;
				}
				this.states[i] = new StateData(ResolveActions(grammar, i, actions), gotos);
			}
		}
		/// <summary>
		/// 分析并尝试解决动作中的冲突。
		/// </summary>
		/// <param name="grammar">语法分析器使用的语法。</param>
		/// <param name="index">当前状态的索引。</param>
		/// <param name="actionList">动作列表。</param>
		/// <returns>解决冲突后的动作列表。</returns>
		private ParseAction[] ResolveActions(Grammar<T> grammar, int index,
			List<Tuple<LRItem<T>, ParseAction>>[] actionList)
		{
			ParseAction[] actions = new ParseAction[actionCount];
			for (int i = 1; i < actionCount; i++)
			{
				List<Tuple<LRItem<T>, ParseAction>> list = actionList[i];
				int cnt = list.Count;
				if (cnt == 0)
				{
					continue;
				}
				if (cnt == 1)
				{
					actions[i] = list[0].Item2;
					continue;
				}
				// 当前的终结符。
				T sym = (T)Enum.ToObject(typeof(T), i - Constants.TokenOffset);
				List<ParserConflictItem> conflictItems = new List<ParserConflictItem>();
				conflictItems.Add(new ParserConflictItem(list[0].Item1.ToString(), list[0].Item2));
				ParseAction action = list[0].Item2;
				int actionIdx = 0;
				for (int j = 1; j < list.Count; j++)
				{
					if (action.ActionType == ParseActionType.Reduce)
					{
						if (list[j].Item2.ActionType == ParseActionType.Shift)
						{
							int assocCmp = grammar.CompareAssociativity(productions[action.Index], sym);
							// 冲突时采用移入。
							if (assocCmp == 0)
							{
								action = list[j].Item2;
								conflictItems.Add(new ParserConflictItem(list[j].Item1.ToString(), list[j].Item2));
							}
							else if (assocCmp == -1)
							{
								action = list[j].Item2;
								conflictItems.RemoveAt(actionIdx);
								actionIdx = conflictItems.Count;
								conflictItems.Add(new ParserConflictItem(list[j].Item1.ToString(), list[j].Item2));
							}
						}
						else
						{
							int assocCmp = grammar.CompareAssociativity(productions[action.Index],
								list[j].Item1.Production);
							if (assocCmp == -1)
							{
								action = list[j].Item2;
								conflictItems.RemoveAt(actionIdx);
								actionIdx = conflictItems.Count;
								conflictItems.Add(new ParserConflictItem(list[j].Item1.ToString(), list[j].Item2));
							}
							else if (assocCmp == 0)
							{
								// 冲突时采用标号较小的规则。
								if (list[j].Item2.Index < action.Index)
								{
									action = list[j].Item2;
								}
								conflictItems.Add(new ParserConflictItem(list[j].Item1.ToString(), list[j].Item2));
							}
						}
					}
					else if (list[j].Item2.ActionType == ParseActionType.Reduce)
					{
						int assocCmp = grammar.CompareAssociativity(productions[action.Index], sym);
						// 冲突时采用移入。
						if (assocCmp == 1)
						{
							action = list[j].Item2;
							conflictItems.RemoveAt(actionIdx);
							actionIdx = conflictItems.Count;
							conflictItems.Add(new ParserConflictItem(list[j].Item1.ToString(), list[j].Item2));
						}
						else if (assocCmp == 0)
						{
							conflictItems.Add(new ParserConflictItem(list[j].Item1.ToString(), list[j].Item2));
						}
					}
				}
				if (conflictItems.Count > 1)
				{
					conflicts.Add(new ParserConflict(index, sym.ToString(), actionIdx, conflictItems.ToArray()));
				}
				actions[i] = action;
			}
			actions[Constants.UniqueIdx] = FindUniqueReduction(actions);
			return actions;
		}
		/// <summary>
		/// 返回指定状态的唯一可行归约。
		/// </summary>
		/// <param name="actions">要寻找唯一归约的状态动作。</param>
		/// <returns>如果指定状态中只有一个归约是可行的，则返回唯一的可行归约；
		/// 否则返回错误动作。</returns>
		private static ParseAction FindUniqueReduction(ParseAction[] actions)
		{
			ParseAction action = ParseAction.Error;
			for (int i = 1; i < actions.Length; i++)
			{
				switch (actions[i].ActionType)
				{
					case ParseActionType.Accept:
					case ParseActionType.Shift:
						return ParseAction.Error;
					case ParseActionType.Reduce:
						if (action.ActionType == ParseActionType.Error)
						{
							action = actions[i];
						}
						else if (action.Index != actions[i].Index)
						{
							return ParseAction.Error;
						}
						break;
				}
			}
			return action;
		}

		#endregion // LALR 语法分析表

		#region 生成词法单元分析器

		/// <summary>
		/// 返回词法单元分析器。
		/// </summary>
		/// <returns>词法单元分析器的实例。</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public TokenParser<T> GetParser()
		{
			return new LRParser<T>(this);
		}

		#endregion // 生成词法单元分析器

	}
}
