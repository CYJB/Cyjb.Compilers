using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Cyjb.Compilers.RegularExpressions
{
	/// <summary>
	/// 表示逐字字符串的正则表达式。
	/// </summary>
	public sealed class LiteralExp : LexRegex
	{
		/// <summary>
		/// 正则表达式表示的字符串。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string literal;
		/// <summary>
		/// 忽略大小写时使用的区域信息。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly CultureInfo? culture;

		/// <summary>
		/// 使用表示的字符串初始化 <see cref="LiteralExp"/> 类的新实例。
		/// </summary>
		/// <param name="literal">正则表达式表示的字符串。</param>
		internal LiteralExp(string literal)
		{
			this.literal = literal;
		}

		/// <summary>
		/// 使用表示的不区分大小写的字符串初始化 <see cref="LiteralExp"/> 类的新实例。
		/// </summary>
		/// <param name="literal">正则表达式表示的字符串。</param>
		/// <param name="culture">忽略大小写时使用的区域信息。</param>
		internal LiteralExp(string literal, CultureInfo culture)
		{
			this.literal = literal;
			this.culture = culture;
		}

		/// <summary>
		/// 获取当前正则表达式匹配的字符串长度。
		/// </summary>
		/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>null</c>。</value>
		public override int? Length => literal.Length;

		/// <summary>
		/// 获取正则表达式表示的字符串。
		/// </summary>
		/// <value>正则表达式表示的字符串。</value>
		public new string Literal => literal;

		/// <summary>
		/// 获取是否忽略大小写。
		/// </summary>
		/// <value>如果忽略字符串大小写，则为 <c>true</c>；否则为 <c>false</c>。</value>
		public bool IgnoreCase => culture != null;

		/// <summary>
		/// 获取忽略大小写时使用的区域信息。
		/// </summary>
		/// <value>忽略大小写时使用的区域信息。</value>
		public CultureInfo? Culture => culture;

		/// <summary>
		/// 返回当前对象是否等于同一类型的另一对象。
		/// </summary>
		/// <param name="other">要比较的对象。</param>
		/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public override bool Equals(LexRegex? other)
		{
			if (other is LiteralExp exp)
			{
				return literal == exp.literal && culture == exp.culture;
			}
			return false;
		}

		/// <summary>
		/// 返回当前对象的哈希值。
		/// </summary>
		/// <returns>当前对象的哈希值。</returns>
		public override int GetHashCode()
		{
			return HashCode.Combine(literal, culture);
		}

		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <param name="builder">字符串构造器。</param>
		internal override void ToString(StringBuilder builder)
		{
			if (culture == null)
			{
				builder.Append('"');
				builder.Append(literal);
				builder.Append('"');
			}
			else
			{
				builder.Append("(?i:");
				builder.Append(literal);
				builder.Append(')');
			}
		}
	}
}
