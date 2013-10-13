using System.Diagnostics;
using System.Text;
using Cyjb.Compiler.Lexer;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示定位点的正则表达式。
	/// </summary>
	public sealed class AnchorExp : Regex
	{
		/// <summary>
		/// 表示行的结尾的正则表达式。
		/// </summary>
		internal static new readonly Regex EndOfLine = Regex.Symbol('\r').Optional().Concat(Regex.Symbol('\n'));
		/// <summary>
		/// 内部正则表达式。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Regex innerExp;
		/// <summary>
		/// 使用内部正则表达式初始化 <see cref="AnchorExp"/> 类的新实例。
		/// </summary>
		/// <param name="innerExp">包含的内部正则表达式。</param>
		internal AnchorExp(Regex innerExp)
		{
			ExceptionHelper.CheckArgumentNull(innerExp, "innerExp");
			CheckRegex(innerExp);
			this.innerExp = innerExp;
			this.BeginningOfLine = false;
			this.TrailingExpression = null;
		}
		/// <summary>
		/// 获取或设置是否定位到行的起始。
		/// </summary>
		/// <value>如果正则表达式定位到行的起始，则为 <c>true</c>；否则为 <c>false</c>。</value>
		public new bool BeginningOfLine { get; set; }
		/// <summary>
		/// 获取内部的正则表达式。
		/// </summary>
		/// <value>内部的正则表达式。</value>
		public Regex InnerExpression { get { return innerExp; } }
		/// <summary>
		/// 获取或设置要向前看的正则表达式。
		/// </summary>
		/// <value>要向前看的正则表达式。</value>
		public Regex TrailingExpression { get; set; }
		/// <summary>
		/// 获取向前看的正则表达式的 NFA 状态。
		/// </summary>
		/// <value>向前看的正则表达式的 NFA 状态。</value>
		internal NfaState TrailingHeadState { get; private set; }
		/// <summary>
		/// 根据当前的正则表达式构造 NFA。
		/// </summary>
		/// <param name="nfa">要构造的 NFA。</param>
		internal override void BuildNfa(Nfa nfa)
		{
			innerExp.BuildNfa(nfa);
			if (TrailingExpression != null)
			{
				NfaState head = nfa.HeadState;
				TrailingHeadState = nfa.TailState;
				TrailingExpression.BuildNfa(nfa);
				TrailingHeadState.Add(nfa.HeadState);
				nfa.HeadState = head;
				TrailingHeadState.StateType = NfaStateType.TrailingHead;
				nfa.TailState.StateType = NfaStateType.Trailing;
			}
		}
		/// <summary>
		/// 获取当前正则表达式匹配的字符串长度。
		/// </summary>
		/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>-1</c>。</value>
		public override int Length
		{
			get { return innerExp.Length; }
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			if (BeginningOfLine)
			{
				builder.Append('^');
			}
			builder.Append(innerExp.ToString());
			if (TrailingExpression != null)
			{
				if (TrailingExpression == EndOfLine)
				{
					builder.Append('$');
				}
				else
				{
					builder.Append('/');
					builder.Append(TrailingExpression.ToString());
				}
			}
			return builder.ToString();
		}
	}
}
