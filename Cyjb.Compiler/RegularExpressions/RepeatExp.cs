using System.Diagnostics;
using System.Text;
using Cyjb.Compiler.Lexer;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示重复多次的正则表达式。
	/// </summary>
	public sealed class RepeatExp : Regex
	{
		/// <summary>
		/// 重复多次的内部正则表达式。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Regex innerExp;
		/// <summary>
		/// 内部正则表达式的最少重复次数。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int minTimes;
		/// <summary>
		/// 内部正则表达式的最多重复次数。<see cref="System.Int32.MaxValue"/> 表示不限制次数。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int maxTimes;
		/// <summary>
		/// 使用内部正则表达式和重复次数初始化 <see cref="RepeatExp"/> 类的新实例。
		/// </summary>
		/// <param name="innerExp">包含的内部正则表达式。</param>
		/// <param name="minTimes">内部正则表达式的最少重复次数。</param>
		/// <param name="maxTimes">内部正则表达式的最多重复次数。</param>
		internal RepeatExp(Regex innerExp, int minTimes, int maxTimes)
		{
			if (minTimes < 0)
			{
				ExceptionHelper.ArgumentNegative("minTimes");
			}
			if (maxTimes < 0)
			{
				ExceptionHelper.ArgumentNegative("maxTimes");
			}
			if (minTimes > maxTimes)
			{
				ExceptionHelper.ReversedArgument("minTimes", "maxTimes");
			}
			ExceptionHelper.CheckArgumentNull(innerExp, "innerExp");
			CheckRegex(innerExp);
			this.innerExp = innerExp;
			this.minTimes = minTimes;
			this.maxTimes = maxTimes;
		}
		/// <summary>
		/// 获取重复多次的内部正则表达式。
		/// </summary>
		public Regex InnerExpression
		{
			get { return innerExp; }
		}
		/// <summary>
		/// 获取内部正则表达式的最少重复次数。
		/// </summary>
		public int MinTimes
		{
			get { return minTimes; }
		}
		/// <summary>
		/// 获取内部正则表达式的最多重复次数。<see cref="System.Int32.MaxValue"/> 表示不限制次数。
		/// </summary>
		public int MaxTimes
		{
			get { return maxTimes; }
		}
		/// <summary>
		/// 获取当前正则表达式匹配的字符长度。变长度则为 <c>-1</c>。
		/// </summary>
		public override int Length
		{
			get
			{
				if (minTimes == maxTimes)
				{
					return minTimes;
				}
				return -1;
			}
		}
		/// <summary>
		/// 根据当前的正则表达式构造 NFA。
		/// </summary>
		/// <param name="nfa">要构造的 NFA。</param>
		internal override void BuildNfa(Nfa nfa)
		{
			NfaState head = nfa.NewState();
			NfaState tail = nfa.NewState();
			NfaState lastHead = head;
			// 如果没有上限，则需要特殊处理。
			int times = maxTimes == int.MaxValue ? minTimes : maxTimes;
			if (times == 0)
			{
				// 至少要构造一次。
				times = 1;
			}
			for (int i = 0; i < times; i++)
			{
				innerExp.BuildNfa(nfa);
				lastHead.Add(nfa.HeadState);
				if (i >= minTimes)
				{
					// 添加到最终的尾状态的转移。
					lastHead.Add(tail);
				}
				lastHead = nfa.TailState;
			}
			// 为最后一个节点添加转移。
			lastHead.Add(tail);
			// 无上限的情况。
			if (maxTimes == int.MaxValue)
			{
				// 在尾部添加一个无限循环。
				nfa.TailState.Add(nfa.HeadState);
			}
			nfa.HeadState = head;
			nfa.TailState = tail;
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			if (innerExp is AlternationExp || innerExp is ConcatenationExp)
			{
				builder.Append("(");
				builder.Append(innerExp);
				builder.Append(")");
			}
			else
			{
				builder.Append(innerExp);
			}
			bool skip = false;
			if (maxTimes == int.MaxValue)
			{
				if (minTimes == 0)
				{
					builder.Append("*");
					skip = true;
				}
				else if (minTimes == 1)
				{
					builder.Append("+");
					skip = true;
				}
			}
			else if (minTimes == 0 && maxTimes == 1)
			{
				builder.Append("?");
				skip = true;
			}
			if (!skip)
			{
				builder.Append("{");
				builder.Append(minTimes);
				if (minTimes != maxTimes)
				{
					builder.Append(',');
					if (maxTimes != int.MaxValue)
					{
						builder.Append(maxTimes);
					}
				}
				builder.Append("}");
			}
			return builder.ToString();
		}
	}
}
