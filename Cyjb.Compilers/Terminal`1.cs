using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cyjb.Compilers.Lexers;
using Cyjb.Compilers.RegularExpressions;

namespace Cyjb.Compilers
{
	/// <summary>
	/// 表示词法分析中的终结符。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	internal sealed class Terminal<T> : Symbol<T>
		where T : struct
	{
		/// <summary>
		/// 定义当前终结符的上下文。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private HashSet<LexerContext> context = new HashSet<LexerContext>();
		/// <summary>
		/// 使用终结符的索引、正则表达式和动作初始化 <see cref="Terminal&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="index">当前终结符的索引。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		internal Terminal(int index, Regex regex, Action<ReaderController<T>> action)
			: base(Invalid, index)
		{
			this.ValidId = false;
			this.RegularExpression = regex;
			this.Action = action;
		}
		/// <summary>
		/// 使用终结符的标识符、索引、正则表达式和动作初始化 <see cref="Terminal&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="id">当前终结符的标识符。</param>
		/// <param name="index">当前终结符的索引。</param>
		/// <param name="regex">终结符对应的正则表达式。</param>
		/// <param name="action">终结符的动作。</param>
		internal Terminal(T id, int index, Regex regex, Action<ReaderController<T>> action)
			: base(id, index)
		{
			this.ValidId = true;
			this.RegularExpression = regex;
			this.Action = action;
		}
		/// <summary>
		/// 当前终结符是否包含有效标识符。
		/// </summary>
		/// <value>如果当前终结符包含有效标识符，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</value>
		public bool ValidId { get; private set; }
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
		public Action<ReaderController<T>> Action { get; set; }
	}
}
