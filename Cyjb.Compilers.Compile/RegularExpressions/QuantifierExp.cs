using System.Diagnostics;
using System.Text;

namespace Cyjb.Compilers.RegularExpressions
{
	/// <summary>
	/// 表示指示数量的正则表达式。
	/// </summary>
	public sealed class QuantifierExp : LexRegex
	{
		/// <summary>
		/// 内部正则表达式。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly LexRegex innerExp;
		/// <summary>
		/// 内部正则表达式的最少重复次数。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int minTimes;
		/// <summary>
		/// 内部正则表达式的最多重复次数，使用 <see cref="int.MaxValue"/> 表示不限制次数。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int maxTimes;
		/// <summary>
		/// 当前正则表达式的长度。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int? length;

		/// <summary>
		/// 使用内部正则表达式和数量上下限初始化 <see cref="QuantifierExp"/> 类的新实例。
		/// </summary>
		/// <param name="innerExp">包含的内部正则表达式。</param>
		/// <param name="minTimes">内部正则表达式的最少重复次数。</param>
		/// <param name="maxTimes">内部正则表达式的最多重复次数。</param>
		internal QuantifierExp(LexRegex innerExp, int minTimes, int maxTimes)
		{
			if (minTimes < 0)
			{
				throw CommonExceptions.ArgumentNegative(minTimes);
			}
			if (maxTimes < 0)
			{
				throw CommonExceptions.ArgumentNegative(maxTimes);
			}
			if (minTimes > maxTimes)
			{
				throw CommonExceptions.ArgumentMinMaxValue(nameof(minTimes), nameof(maxTimes));
			}
			CheckRegex(innerExp);
			this.innerExp = innerExp;
			this.minTimes = minTimes;
			this.maxTimes = maxTimes;
			if (minTimes == maxTimes)
			{
				length = minTimes * innerExp.Length;
			}
			else
			{
				length = null;
			}
		}

		/// <summary>
		/// 获取内部正则表达式。
		/// </summary>
		/// <value>内部正则表达式。</value>
		public LexRegex InnerExpression => innerExp;

		/// <summary>
		/// 获取内部正则表达式的最少重复次数。
		/// </summary>
		/// <value>内部正则表达式的最少重复次数，这个一个大于等于零的值。</value>
		public int MinTimes => minTimes;

		/// <summary>
		/// 获取内部正则表达式的最多重复次数。
		/// </summary>
		/// <value>内部正则表达式的最多重复次数，<see cref="System.Int32.MaxValue"/> 
		/// 表示不限制重复次数。</value>
		public int MaxTimes => maxTimes;

		/// <summary>
		/// 获取当前正则表达式匹配的字符串长度。
		/// </summary>
		/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>null</c>。</value>
		public override int? Length => length;

		/// <summary>
		/// 返回当前对象是否等于同一类型的另一对象。
		/// </summary>
		/// <param name="other">要比较的对象。</param>
		/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(LexRegex? other)
		{
			if (other is QuantifierExp exp)
			{
				return innerExp == exp.innerExp && minTimes == exp.minTimes && maxTimes == exp.maxTimes;
			}
			return false;
		}

		/// <summary>
		/// 返回当前对象的哈希值。
		/// </summary>
		/// <returns>当前对象的哈希值。</returns>
		public override int GetHashCode()
		{
			return HashCode.Combine(innerExp, minTimes, maxTimes);
		}

		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <param name="builder">字符串构造器。</param>
		internal override void ToString(StringBuilder builder)
		{
			if (innerExp is AlternationExp || innerExp is ConcatenationExp)
			{
				builder.Append('(');
				innerExp.ToString(builder);
				builder.Append(')');
			}
			else
			{
				innerExp.ToString(builder);
			}
			if (maxTimes == int.MaxValue)
			{
				if (minTimes == 0)
				{
					builder.Append('*');
					return;
				}
				else if (minTimes == 1)
				{
					builder.Append('+');
					return;
				}
			}
			else if (minTimes == 0 && maxTimes == 1)
			{
				builder.Append('?');
				return;
			}
			builder.Append('{');
			builder.Append(minTimes);
			if (minTimes != maxTimes)
			{
				builder.Append(',');
				if (maxTimes != int.MaxValue)
				{
					builder.Append(maxTimes);
				}
			}
			builder.Append('}');
		}
	}
}
