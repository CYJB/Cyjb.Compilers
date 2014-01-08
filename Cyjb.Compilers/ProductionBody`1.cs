using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cyjb.Text;

namespace Cyjb.Compilers
{
	/// <summary>
	/// 表示语法产生式体。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	public sealed class ProductionBody<T>
		where T : struct
	{
		/// <summary>
		/// 默认的归约动作。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static readonly Func<ParserController<T>, object> DefaultAction = c =>
		{
			Token<T>[] tokens = new Token<T>[c.Count];
			for (int i = 0; i < tokens.Length; i++)
			{
				tokens[i] = c[i];
			}
			return tokens;
		};
		/// <summary>
		/// 使用索引和产生式体初始化 <see cref="ProductionBody&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="index">产生式的索引。</param>
		/// <param name="body">产生式体。</param>
		internal ProductionBody(int index, T[] body)
		{
			this.Index = index;
			this.Body = body;
			this.ProductionAction = DefaultAction;
		}
		/// <summary>
		/// 获取产生式的索引。
		/// </summary>
		/// <value>产生式的索引。</value>
		internal int Index { get; private set; }
		/// <summary>
		/// 获取产生式体。
		/// </summary>
		/// <value>产生式体。</value>
		internal IList<T> Body { get; private set; }
		/// <summary>
		/// 获取产生式的动作。
		/// </summary>
		/// <value>产生式的动作。</value>
		internal Func<ParserController<T>, object> ProductionAction { get; private set; }
		/// <summary>
		/// 获取表示当前产生式的结合性的非终结符。
		/// </summary>
		/// <value>表示当前产生式的结合性的非终结符的标识符。</value>
		internal T Precedence { get; private set; }
		/// <summary>
		/// 获取是否设置了产生式体的优先级。
		/// </summary>
		/// <value>如果设置了优先级，则为 <c>true</c>；否则为 <c>false</c>。</value>
		internal bool PrecedenceSet { get; private set; }
		/// <summary>
		/// 设置当前产生式体的动作。
		/// </summary>
		/// <param name="prodAction">当前产生式体的动作。</param>
		/// <returns>当前产生式体。</returns>
		public ProductionBody<T> Action(Func<ParserController<T>, object> prodAction)
		{
			this.ProductionAction = prodAction;
			return this;
		}
		/// <summary>
		/// 设置当前产生式体的优先级与指定的非终结符相同。
		/// </summary>
		/// <param name="id">非终结符的标识符。</param>
		/// <returns>当前产生式体。</returns>
		public ProductionBody<T> Prec(T id)
		{
			this.Precedence = id;
			this.PrecedenceSet = true;
			return this;
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			if (Body.Count == 0)
			{
				return "ε";
			}
			return string.Join(" ", Body);
		}
	}
}
