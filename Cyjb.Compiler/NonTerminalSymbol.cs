using System;
using System.Collections.Generic;
using Cyjb.Compiler.Parser;

namespace Cyjb.Compiler
{
	/// <summary>
	/// 表示语法分析中的非终结符。
	/// </summary>
	internal sealed class NonTerminalSymbol : Symbol
	{
		/// <summary>
		/// 使用语法规则和索引初始化 <see cref="NonTerminalSymbol"/> 类的新实例。
		/// </summary>
		/// <param name="grammar">定义非终结符的语法规则。</param>
		/// <param name="index">非终结符的索引。</param>
		internal NonTerminalSymbol(Grammar grammar, int index)
			: base(index)
		{
			this.Grammar = grammar;
		}
		/// <summary>
		/// 获取定义当前非终结符的语法规则。
		/// </summary>
		public Grammar Grammar { get; private set; }
		/// <summary>
		/// 获取以当前非终结符为头的产生式。
		/// </summary>
		public IList<Production> Productions
		{
			get { return ((ProductionCollection)Grammar.Productions)[this]; }
		}
		/// <summary>
		/// 以当前非终结符为左部，定义一个使用默认规约动作的语法产生式。
		/// </summary>
		/// <param name="body">产生式体。</param>
		/// <returns>当前非终结符。</returns>
		public NonTerminalSymbol DefineProduction(params Symbol[] body)
		{
			this.Grammar.DefineProduction(this, body);
			return this;
		}
		/// <summary>
		/// 以当前非终结符为左部，定义一个使用默认规约动作的语法产生式。
		/// </summary>
		/// <param name="body">产生式体。</param>
		/// <param name="associativity">表示产生式的结合性的终结符。</param>
		/// <returns>当前非终结符。</returns>
		public NonTerminalSymbol DefineProduction(Symbol[] body, TerminalSymbol associativity)
		{
			this.Grammar.DefineProduction(this, body, associativity);
			return this;
		}
		/// <summary>
		/// 以当前非终结符为左部，定义一个语法产生式。
		/// </summary>
		/// <param name="body">产生式体。</param>
		/// <param name="action">产生式的动作。</param>
		/// <returns>当前非终结符。</returns>
		public NonTerminalSymbol DefineProduction(Symbol[] body, Func<object[], object> action)
		{
			this.Grammar.DefineProduction(this, body, action);
			return this;
		}
		/// <summary>
		/// 以当前非终结符为左部，定义一个语法产生式。
		/// </summary>
		/// <param name="body">产生式体。</param>
		/// <param name="action">产生式的动作。</param>
		/// <param name="associativity">表示产生式的结合性的终结符。</param>
		/// <returns>当前非终结符。</returns>
		public NonTerminalSymbol DefineProduction(Symbol[] body, Func<object[], object> action,
			TerminalSymbol associativity)
		{
			this.Grammar.DefineProduction(this, body, action, associativity);
			return this;
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			return string.Concat("N", this.Index);
		}
	}
}
