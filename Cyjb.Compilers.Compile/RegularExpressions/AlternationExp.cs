using System.Diagnostics;
using System.Text;
using Cyjb.Collections;

namespace Cyjb.Compilers.RegularExpressions
{
	/// <summary>
	/// 表示并联的正则表达式。
	/// </summary>
	public sealed class AlternationExp : LexRegex
	{
		/// <summary>
		/// 并联的正则表达式。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly LexRegex[] expressions;
		/// <summary>
		/// 当前正则表达式的长度。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly int? length;

		/// <summary>
		/// 使用要并联的正则表达式初始化 <see cref="AlternationExp"/> 类的新实例。
		/// </summary>
		/// <param name="expressions">要并联的正则表达式。</param>
		internal AlternationExp(LexRegex[] expressions)
		{
			CommonExceptions.CheckArgumentNull(expressions);
			UniqueValue<int?> len = new();
			foreach (LexRegex regex in expressions)
			{
				CheckRegex(regex, nameof(expressions));
				len.Value = regex.Length;
			}
			this.expressions = expressions;
			length = len.Value;
		}

		/// <summary>
		/// 获取当前正则表达式匹配的字符串长度。
		/// </summary>
		/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>null</c>。</value>
		public override int? Length => length;

		/// <summary>
		/// 获取并联的正则表达式。
		/// </summary>
		/// <value>并联的正则表达式。</value>
		public LexRegex[] Expressions => expressions;

		/// <summary>
		/// 返回当前对象是否等于同一类型的另一对象。
		/// </summary>
		/// <param name="other">要比较的对象。</param>
		/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(LexRegex? other)
		{
			if (other is AlternationExp exp)
			{
				return ListEqualityComparer<LexRegex>.Default.Equals(expressions, exp.expressions);
			}
			return false;
		}

		/// <summary>
		/// 返回当前对象的哈希值。
		/// </summary>
		/// <returns>当前对象的哈希值。</returns>
		public override int GetHashCode()
		{
			HashCode hashCode = new();
			foreach (LexRegex regex in expressions)
			{
				hashCode.Add(regex);
			}
			return hashCode.GetHashCode();
		}

		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <param name="builder">字符串构造器。</param>
		internal override void ToString(StringBuilder builder)
		{
			bool isFirst = true;
			foreach (LexRegex regex in expressions)
			{
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					builder.Append('|');
				}
				regex.ToString(builder);
			}
		}
	}
}
