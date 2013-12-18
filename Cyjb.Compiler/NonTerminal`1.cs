using System.Collections.Generic;
using System.Diagnostics;
using Cyjb.Text;

namespace Cyjb.Compiler
{
	/// <summary>
	/// 表示语法分析中的非终结符。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	internal sealed class NonTerminal<T> : Symbol<T>
		where T : struct
	{
		/// <summary>
		/// 当前非终结符的所有产生式体。
		/// </summary>
		private readonly ProductionBody<T>[] bodies;
		/// <summary>
		/// 当前非终结符的所有产生式。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Production<T>[] productions;
		/// <summary>
		/// 使用非终结符的标识符、索引和产生式体初始化 <see cref="NonTerminal&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="id">非终结符的标识符。</param>
		/// <param name="index">非终结符的索引。</param>
		/// <param name="bodies">非终结符的产生式体。</param>
		internal NonTerminal(T id, int index, ProductionBody<T>[] bodies)
			: base(id, index)
		{
			this.bodies = bodies;
		}
		/// <summary>
		/// 获取当前非终结符的所有产生式。
		/// </summary>
		/// <value>当前非终结符的所有产生式。</value>
		public IList<Production<T>> Productions { get { return productions; } }
		/// <summary>
		/// 构造产生式。
		/// </summary>
		/// <param name="symbolMapper">符号的映射表。</param>
		public void BuildProductions(Dictionary<T, Symbol<T>> symbolMapper)
		{
			int len = bodies.Length;
			productions = new Production<T>[len];
			for (int i = 0; i < len; i++)
			{
				ProductionBody<T> body = bodies[i];
				int cnt = body.Body.Count;
				Symbol<T>[] symbolBody = new Symbol<T>[cnt];
				for (int j = 0; j < cnt; j++)
				{
					T id = body.Body[j];
					Symbol<T> sym;
					if (symbolMapper.TryGetValue(id, out sym))
					{
						symbolBody[j] = sym;
					}
					else if (EqualityComparer<T>.Default.Equals(id, Token<T>.Error))
					{
						symbolBody[j] = Error;
					}
					else
					{
						throw CompilerExceptionHelper.InvalidSymbolId("body", id.ToString());
					}
				}
				Terminal<T> prec = null;
				if (body.PrecedenceSet)
				{
					Symbol<T> sym;
					if (!symbolMapper.TryGetValue(body.Precedence, out sym))
					{
						throw CompilerExceptionHelper.InvalidSymbolId("Precedence", body.Precedence.ToString());
					}
					prec = sym as Terminal<T>;
					if (prec == null)
					{
						throw CompilerExceptionHelper.InvalidSymbolId("Precedence", body.Precedence.ToString());
					}
				}
				else
				{
					// 使用最右的终结符代表当前产生式的结合性。
					for (int j = symbolBody.Length - 1; j >= 0; j--)
					{
						Terminal<T> sym = symbolBody[j] as Terminal<T>;
						if (sym != null)
						{
							prec = sym;
							break;
						}
					}
				}
				productions[i] = new Production<T>(body.Index, this, symbolBody, body.ProductionAction, prec);
			}
		}
	}
}
