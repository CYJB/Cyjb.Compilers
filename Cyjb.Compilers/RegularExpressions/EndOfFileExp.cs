using System.Diagnostics;
using Cyjb.Compilers.Lexers;

namespace Cyjb.Compilers.RegularExpressions
{
	/// <summary>
	/// 表示文件结尾的正则表达式。
	/// </summary>
	public sealed class EndOfFileExp : Regex
	{
		/// <summary>
		/// 默认实例。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static readonly EndOfFileExp defaultValue = new EndOfFileExp();
		/// <summary>
		/// 获取默认的实例。
		/// </summary>
		/// <value>默认的 <see cref="EndOfFileExp"/> 实例。</value>
		internal static EndOfFileExp Default
		{
			get { return defaultValue; }
		}
		/// <summary>
		/// 初始化 <see cref="EndOfFileExp"/> 类的新实例。
		/// </summary>
		private EndOfFileExp()
		{ }
		/// <summary>
		/// 根据当前的正则表达式构造 NFA。
		/// </summary>
		/// <param name="nfa">要构造的 NFA。</param>
		internal override void BuildNfa(Nfa nfa)
		{ }
		/// <summary>
		/// 获取当前正则表达式匹配的字符串长度。
		/// </summary>
		/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>-1</c>。</value>
		public override int Length
		{
			get { return 0; }
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			return "<<EOF>>";
		}
	}
}
