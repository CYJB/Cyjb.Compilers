using System.Diagnostics;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示字符类的正则表达式。
	/// </summary>
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
		public new string CharClass
		{
			get { return charClass; }
		}
		///// <summary>
		///// 根据当前的正则表达式构造 NFA。
		///// </summary>
		///// <param name="nfa">要构造的 NFA。</param>
		//internal override void BuildNfa(Nfa nfa)
		//{
		//	nfa.HeadState = nfa.CreateState();
		//	nfa.TailState = nfa.CreateState();
		//	nfa.HeadState.Add(nfa.TailState, charClass);
		//}
		/// <summary>
		/// 获取当前正则表达式匹配的字符长度。变长度则为 <c>-1</c>。
		/// </summary>
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
			return RegexCharClass.CharClassDescription(charClass);
		}
	}
}
