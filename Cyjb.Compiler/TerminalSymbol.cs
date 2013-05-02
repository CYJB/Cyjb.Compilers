using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cyjb.Compiler.Lexer;
using Cyjb.Compiler.RegularExpressions;

namespace Cyjb.Compiler
{
	/// <summary>
	/// 表示词法分析或语法分析的终结符。
	/// </summary>
	public sealed class TerminalSymbol : Symbol
	{
		/// <summary>
		/// 当前终结符定义的上下文。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private HashSet<LexerContext> context = new HashSet<LexerContext>();
		/// <summary>
		/// 使用符号的标识符、正则表达式和动作初始化 <see cref="Symbol"/> 类的新实例。
		/// </summary>
		/// <param name="index">当前符号的标识符。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		internal TerminalSymbol(int index, Regex regex, Action<ReaderController> action)
			: base(index)
		{
			this.RegularExpression = regex;
			this.Action = action;
		}
		/// <summary>
		/// 获取终结符对应的正则表达式。
		/// </summary>
		public Regex RegularExpression { get; private set; }
		/// <summary>
		/// 获取当前终结符定义的上下文。
		/// </summary>
		public HashSet<LexerContext> Context
		{
			get { return context; }
		}
		/// <summary>
		/// 获取或设置终结符的动作。
		/// </summary>
		public Action<ReaderController> Action { get; set; }
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			return string.Concat("T", this.Index, " <", string.Join(",", context), ">", this.RegularExpression);
		}
	}
}
