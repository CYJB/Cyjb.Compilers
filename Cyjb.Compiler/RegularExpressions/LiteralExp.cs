using System.Diagnostics;
using System.Globalization;
using Cyjb.Compiler.Lexer;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示逐字字符串的正则表达式。
	/// </summary>
	public sealed class LiteralExp : Regex
	{
		/// <summary>
		/// 正则表达式表示的字符串。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string literal;
		/// <summary>
		/// 是否忽略大小写。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly bool ignoreCase;
		/// <summary>
		/// 忽略大小写时使用的区域信息。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly CultureInfo culture;
		/// <summary>
		/// 使用表示的字符串初始化 <see cref="LiteralExp"/> 类的新实例。
		/// </summary>
		/// <param name="literal">正则表达式表示的字符串。</param>
		internal LiteralExp(string literal)
		{
			this.literal = literal;
			this.ignoreCase = false;
		}
		/// <summary>
		/// 使用表示的不区分大小写的字符串初始化 <see cref="LiteralExp"/> 类的新实例。
		/// </summary>
		/// <param name="literal">正则表达式表示的字符串。</param>
		/// <param name="culture">忽略大小写时使用的区域信息。</param>
		internal LiteralExp(string literal, CultureInfo culture)
		{
			this.literal = literal;
			this.ignoreCase = true;
			this.culture = culture;
		}
		/// <summary>
		/// 获取正则表达式表示的字符串。
		/// </summary>
		/// <value>正则表达式表示的字符串。</value>
		public new string Literal
		{
			get { return literal; }
		}
		/// <summary>
		/// 获取是否忽略大小写。
		/// </summary>
		/// <value>如果忽略字符串大小写，则为 <c>true</c>；否则为 <c>false</c>。</value>
		public bool IgnoreCase
		{
			get { return ignoreCase; }
		}
		/// <summary>
		/// 获取忽略大小写时使用的区域信息。
		/// </summary>
		/// <value>忽略大小写时使用的区域信息。</value>
		public CultureInfo Culture
		{
			get { return culture; }
		}
		/// <summary>
		/// 根据当前的正则表达式构造 NFA。
		/// </summary>
		/// <param name="nfa">要构造的 NFA。</param>
		internal override void BuildNfa(Nfa nfa)
		{
			string str = literal;
			if (ignoreCase)
			{
				str = literal.ToUpper(culture);
			}
			nfa.HeadState = nfa.NewState();
			nfa.TailState = nfa.HeadState;
			for (int i = 0; i < literal.Length; i++)
			{
				NfaState state = nfa.NewState();
				if (culture == null)
				{
					// 区分大小写。
					nfa.TailState.Add(state, str[i]);
				}
				else
				{
					// 不区分大小写。
					RegexCharClass cc = new RegexCharClass();
					cc.AddChar(str[i]);
					cc.AddChar(char.ToLower(str[i], culture));
					nfa.TailState.Add(state, cc.ToStringClass());
				}
				nfa.TailState = state;
			}
		}
		/// <summary>
		/// 获取当前正则表达式匹配的字符串长度。
		/// </summary>
		/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>-1</c>。</value>
		public override int Length
		{
			get { return literal.Length; }
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			if (ignoreCase)
			{
				return string.Concat("(?i:", literal, ")");
			}
			return string.Concat("\"", literal, "\"");
		}
	}
}
