using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Cyjb.Compiler.Lexer;
using Cyjb.Compiler.RegularExpressions;

namespace Cyjb.Compiler
{
	/// <summary>
	/// 表示词法分析或语法分析中的终结符。
	/// </summary>
	internal sealed class TerminalSymbol : Symbol
	{
		/// <summary>
		/// 定义当前终结符的上下文。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private HashSet<LexerContext> context = new HashSet<LexerContext>();
		/// <summary>
		/// 使用终结符的标识符、索引、正则表达式和动作初始化 <see cref="Symbol"/> 类的新实例。
		/// </summary>
		/// <param name="id">当前终结符的标识符。</param>
		/// <param name="index">当前终结符的索引。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		internal TerminalSymbol(string id, int index, Regex regex, Action<ReaderController> action)
			: base(id ?? "_TS_#" + index.ToString(CultureInfo.InvariantCulture), index)
		{
			this.RegularExpression = regex;
			this.Action = action;
		}
		/// <summary>
		/// 获取当前终结符对应的正则表达式。
		/// </summary>
		/// <value>当前终结符对应的正则表达式。</value>
		public Regex RegularExpression { get; private set; }
		/// <summary>
		/// 获取定义当前终结符的上下文。
		/// </summary>
		/// <value>定义当前终结符的上下文。</value>
		public HashSet<LexerContext> Context
		{
			get { return context; }
		}
		/// <summary>
		/// 获取或设置当前终结符的动作。
		/// </summary>
		/// <value>当前终结符的动作。</value>
		public Action<ReaderController> Action { get; set; }
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			return string.Concat(this.Id, " <", string.Join(",", context), ">", this.RegularExpression);
		}
	}
}
