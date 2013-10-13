using System.Diagnostics;
using Cyjb.Compiler.Lexer;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示字符类的正则表达式。
	/// </summary>
	/// <seealso cref="RegexCharClass"/>
	public sealed class CharClassExp : Regex
	{
		/// <summary>
		/// 正则表达式表示的字符类。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string charClass;
		/// <summary>
		/// 使用表示的字符类初始化 <see cref="CharClassExp"/> 类的新实例。
		/// </summary>
		/// <param name="charClass">正则表达式表示的字符类。</param>
		internal CharClassExp(string charClass)
		{
			this.charClass = charClass;
		}
		/// <summary>
		/// 获取正则表达式表示的字符类。
		/// </summary>
		/// <value>正则表达式表示的字符类，该字符类是压缩后的结果，
		/// 需要用 <see cref="RegexCharClass"/> 类的相关静态方法操作。</value>
		public new string CharClass
		{
			get { return charClass; }
		}
		/// <summary>
		/// 根据当前的正则表达式构造 NFA。
		/// </summary>
		/// <param name="nfa">要构造的 NFA。</param>
		internal override void BuildNfa(Nfa nfa)
		{
			nfa.HeadState = nfa.NewState();
			nfa.TailState = nfa.NewState();
			// 添加一个字符类转移。
			nfa.HeadState.Add(nfa.TailState, charClass);
		}
		/// <summary>
		/// 获取当前正则表达式匹配的字符串长度。
		/// </summary>
		/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>-1</c>。</value>
		public override int Length
		{
			get { return 1; }
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			if (RegexCharClass.IsSingleton(charClass))
			{
				// 只表示单一字符。
				char symbol = RegexCharClass.SingletonChar(charClass);
				// 转换字符转义。
				switch (symbol)
				{
					// 转义特殊标志。
					case '.': return "\\.";
					case '[': return "\\[";
					case ']': return "\\]";
					case '(': return "\\(";
					case ')': return "\\)";
					case '{': return "\\{";
					case '}': return "\\}";
					case '?': return "\\?";
					case '+': return "\\+";
					case '*': return "\\*";
					case '|': return "\\|";
					default: return symbol.ToPrintableString();
				}
			}
			else
			{
				return RegexCharClass.GetDescription(charClass);
			}
		}
	}
}
