using System.Collections.Generic;
using Cyjb.Collections;
using Cyjb.Text;

namespace Cyjb.Compiler.Parser
{
	/// <summary>
	/// 表示 LR 词法单元分析器。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	internal sealed class LRParser<T> : TokenParser<T>
		where T : struct
	{
		/// <summary>
		/// 语法分析信息。
		/// </summary>
		private ParserRule<T> rule;
		/// <summary>
		/// 获取符号序列的分析结果。
		/// </summary>
		/// <value>符号序列的分析结果。</value>
		private Token<T> result;
		/// <summary>
		/// 词法单元堆栈。
		/// </summary>
		private Stack<Token<T>> tokenStack = new Stack<Token<T>>();
		/// <summary>
		/// 状态堆栈。
		/// </summary>
		private ListStack<int> stateStack = new ListStack<int>();
		/// <summary>
		/// 语法分析器的控制器。
		/// </summary>
		private ParserController<T> controller = new ParserController<T>();

		/// <summary>
		/// 使用指定的语法规则初始化 <see cref="LRParser&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="rule">LALR 语法分析器的规则。</param>
		internal LRParser(ParserRule<T> rule)
		{
			this.rule = rule;
			this.stateStack.Push(0);
		}
		/// <summary>
		/// 获取符号序列的分析结果。
		/// </summary>
		/// <value>符号序列的分析结果。</value>
		public override Token<T> Result { get { return result; } }
		/// <summary>
		/// 分析当前的符号。
		/// </summary>
		/// <param name="token">要分析的符号。</param>
		public override void Parse(Token<T> token)
		{
			if (token.IsEndOfFile)
			{
				if (stateStack.Count == 1)
				{
					return;
				}
			}
			while (true)
			{
				int state = stateStack.Peek();
				ParseAction action = GetAction(state, token.Id);
				switch (action.ActionType)
				{
					case ParseActionType.Shift:
						Shift(token, action.Index);
						goto ReduceByUnique;
					case ParseActionType.Reduce:
						Reduce(action.Index);
						break;
					case ParseActionType.Accept:
						stateStack.Pop();
						result = tokenStack.Pop();
						return;
					case ParseActionType.Error:
						// ErrorRecovery(state, token.Id);
						return;
				}
			}
		ReduceByUnique:
			// 尝试根据唯一归约进行归约。
			while (true)
			{
				int state = stateStack.Peek();
				ParseAction action = rule.States[state].Actions[Constants.UniqueIdx];
				switch (action.ActionType)
				{
					case ParseActionType.Reduce:
						Reduce(action.Index);
						break;
					case ParseActionType.Error:
						return;
				}
			}
		}
		/// <summary>
		/// 返回指定状态和符号对应的动作。
		/// </summary>
		/// <param name="state">要获取动作的状态。</param>
		/// <param name="tokenId">要获取动作的符号。</param>
		/// <returns>指定状态和符号对应的动作。</returns>
		private ParseAction GetAction(int state, T tokenId)
		{
			return rule.States[state].Actions[((int)(object)tokenId) + Constants.TokenOffset];
		}
		/// <summary>
		/// 移入当前符号，并将指定状态压栈。
		/// </summary>
		/// <param name="token">要移入的符号。</param>
		/// <param name="state">要压栈的状态。</param>
		private void Shift(Token<T> token, int state)
		{
			stateStack.Push(state);
			tokenStack.Push(token);
		}
		/// <summary>
		/// 使用指定的产生式归约。
		/// </summary>
		/// <param name="index">归约使用的产生式的索引。</param>
		private void Reduce(int index)
		{
			ProductionData<T> info = rule.Productions[index];
			int size = info.BodySize;
			controller.InternalAdd(tokenStack, size);
			while (size-- > 0)
			{
				stateStack.Pop();
			}
			object value = rule.Productions[index].Action(controller);
			tokenStack.Push(new Token<T>((T)(object)(info.Head + rule.NonTerminalOffset), null,
				controller.Start, controller.End, value));
			stateStack.Push(rule.States[stateStack.Peek()].Gotos[info.Head]);
		}
	}
}
