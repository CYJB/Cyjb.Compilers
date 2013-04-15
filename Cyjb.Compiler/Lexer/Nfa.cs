using System.Collections.Generic;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compiler.Lexers
{
	/// <summary>
	/// 表示不确定有穷自动机（NFA）。
	/// </summary>
	internal sealed class Nfa : ReadOnlyList<NfaState>
	{
		/// <summary>
		/// NFA 使用的字符类。
		/// </summary>
		private CharClass charClass = new CharClass();
		/// <summary>
		/// 获取或设置 NFA 的首状态。
		/// </summary>
		public NfaState HeadState { get; set; }
		/// <summary>
		/// 获取或设置 NFA 的尾状态。
		/// </summary>
		public NfaState TailState { get; set; }
		/// <summary>
		/// 在当前 NFA 中创建一个新状态。
		/// </summary>
		/// <returns>新创建的状态。</returns>
		public NfaState NewState()
		{
			NfaState state = new NfaState(this, base.Items.Count);
			base.Items.Add(state);
			return state;
		}
		/// <summary>
		/// 返回指定的字符类对应的字符类索引。
		/// </summary>
		/// <param name="regCharClass">要获取字符类索引的字符类。</param>
		/// <returns>字符类对应的字符类索引。</returns>
		public HashSet<int> GetCharClass(string regCharClass)
		{
			return charClass.GetCharClass(regCharClass);
		}
		/// <summary>
		/// 返回指定的字符对应的字符类索引。
		/// </summary>
		/// <param name="ch">要获取字符类索引的字符。</param>
		/// <returns>字符对应的字符类索引。</returns>
		public HashSet<int> GetCharClass(char ch)
		{
			return charClass.GetCharClass(ch);
		}
	}
}
