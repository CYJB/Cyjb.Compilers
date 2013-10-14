using System;
using System.Collections.Generic;
using System.Text;
using Cyjb.Text;

namespace Cyjb.Compiler.Parser
{
	/// <summary>
	/// 表示文法的产生式。
	/// </summary>
	internal sealed class Production
	{
		/// <summary>
		/// 使用产生式头、产生式体、动作和结合性初始化 <see cref="Production"/> 类的新实例。
		/// </summary>
		/// <param name="index">产生式的索引。</param>
		/// <param name="head">产生式头。</param>
		/// <param name="body">产生式体。</param>
		/// <param name="action">产生式的动作。</param>
		/// <param name="associativity">表示产生式结合性的终结符。</param>
		internal Production(int index, NonTerminalSymbol head, Symbol[] body,
			Func<Token[], object> action, TerminalSymbol associativity)
		{
			this.Index = index;
			this.Head = head;
			this.Body = body;
			this.Action = action;
			if (associativity == null)
			{
				// 使用最右的终结符代表当前产生式的结合性。
				for (int i = body.Length - 1; i >= 0; i--)
				{
					TerminalSymbol symbol = body[i] as TerminalSymbol;
					if (symbol != null)
					{
						associativity = symbol;
						break;
					}
				}
			}
			this.AssociativitySymbol = associativity;
		}
		/// <summary>
		/// 获取产生式的索引。
		/// </summary>
		internal int Index { get; private set; }
		/// <summary>
		/// 获取产生式头。
		/// </summary>
		public NonTerminalSymbol Head { get; private set; }
		/// <summary>
		/// 获取产生式体。
		/// </summary>
		public IList<Symbol> Body { get; private set; }
		/// <summary>
		/// 获取产生式的动作。
		/// </summary>
		public Func<Token[], object> Action { get; private set; }
		/// <summary>
		/// 获取表示当前产生式的结合性的非终结符。
		/// </summary>
		public TerminalSymbol AssociativitySymbol { get; private set; }
		/// <summary>
		/// 返回产生式体的字符串表示方式。
		/// </summary>
		/// <returns>产生式体的字符串表示方式。</returns>
		internal string GetBodyString()
		{
			int cnt = Body.Count;
			if (cnt == 0)
			{
				return "ε";
			}
			StringBuilder text = new StringBuilder();
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
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			return string.Concat(this.Head, " → ", this.GetBodyString());
		}
	}
}
