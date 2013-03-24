using System.Diagnostics;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示单个字符的正则表达式。
	/// </summary>
	public sealed class SymbolExp : Regex
	{
		/// <summary>
		/// 正则表达式表示的字符。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly char symbol;
		/// <summary>
		/// 使用表示的字符初始化 <see cref="SymbolExp"/> 类的新实例。
		/// </summary>
		/// <param name="symbol">正则表达式表示的字符。</param>
		internal SymbolExp(char symbol)
		{
			this.symbol = symbol;
		}
		/// <summary>
		/// 获取正则表达式表示的字符。
		/// </summary>
		public new char Symbol
		{
			get { return symbol; }
		}
		///// <summary>
		///// 根据当前的正则表达式构造 NFA。
		///// </summary>
		///// <param name="nfa">要构造的 NFA。</param>
		//internal override void BuildNfa(Nfa nfa)
		//{
		//	nfa.HeadState = nfa.CreateState();
		//	nfa.TailState = nfa.CreateState();
		//	nfa.HeadState.Add(nfa.TailState, symbol);
		//}
		/// <summary>
		/// 获取当前正则表达式匹配的字符长度。变长度则为 <c>-1</c>。
		/// </summary>
		public override int Length { get { return 1; } }
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
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
	}
}
