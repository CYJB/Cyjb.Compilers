using System;
using System.Collections.Generic;
using System.Text;

namespace Cyjb.Compiler
{
	/// <summary>
	/// 表示文法的产生式。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	internal sealed class Production<T>
		where T : struct
	{
		/// <summary>
		/// 使用产生式的索引、产生式头、产生式体、动作和优先级初始化 
		/// <see cref="Production&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="index">产生式的索引。</param>
		/// <param name="head">产生式头。</param>
		/// <param name="body">产生式体。</param>
		/// <param name="action">产生式的动作。</param>
		/// <param name="prec">表示产生式优先级的终结符。</param>
		public Production(int index, NonTerminal<T> head, Symbol<T>[] body,
			Func<ParserController<T>, object> action, Terminal<T> prec)
		{
			this.Index = index;
			this.Head = head;
			this.Body = body;
			this.Action = action;
			this.Precedence = prec;
		}
		/// <summary>
		/// 获取产生式的索引。
		/// </summary>
		/// <value>产生式的索引。</value>
		public int Index { get; private set; }
		/// <summary>
		/// 获取产生式头。
		/// </summary>
		/// <value>产生式的头。</value>
		public NonTerminal<T> Head { get; private set; }
		/// <summary>
		/// 获取产生式体。
		/// </summary>
		/// <value>产生式体。</value>
		public IList<Symbol<T>> Body { get; private set; }
		/// <summary>
		/// 获取产生式的动作。
		/// </summary>
		/// <value>产生式的动作。</value>
		public Func<ParserController<T>, object> Action { get; private set; }
		/// <summary>
		/// 获取表示当前产生式的结合性的非终结符。
		/// </summary>
		/// <value>表示当前产生式的结合性的非终结符的标识符。</value>
		public Terminal<T> Precedence { get; private set; }
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			StringBuilder text = new StringBuilder();
			text.Append(Head.Id);
			text.Append(" → ");
			int cnt = Body.Count;
			if (cnt == 0)
			{
				return "ε";
			}
			for (int i = 0; i < cnt; i++)
			{
				if (i > 0)
				{
					text.Append(" ");
				}
				text.Append(Body[i]);
			}
			return text.ToString();
		}
	}
}
