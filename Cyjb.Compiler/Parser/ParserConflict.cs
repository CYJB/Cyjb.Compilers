using System;
using System.Text;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compiler.Parser
{
	/// <summary>
	/// 表示语法分析器的冲突和解决办法。
	/// </summary>
	[Serializable]
	public sealed class ParserConflict : ReadOnlyList<ParserConflictItem>
	{
		/// <summary>
		/// 使用冲突的信息初始化 <see cref="ParserConflict"/> 类的新实例。
		/// </summary>
		/// <param name="state">产生冲突的状态。</param>
		/// <param name="symbol">产生冲突的终结符。</param>
		/// <param name="choice">被选择的冲突项。</param>
		/// <param name="items">冲突项。</param>
		public ParserConflict(int state, string symbol, int choice, params ParserConflictItem[] items)
			: base(items)
		{
			this.State = state;
			this.Symbol = symbol;
			this.Choice = choice;
		}
		/// <summary>
		/// 获取产生冲突的状态。
		/// </summary>
		/// <value>产生冲突的状态。</value>
		public int State { get; private set; }
		/// <summary>
		/// 获取产生冲突的终结符。
		/// </summary>
		/// <value>产生冲突的终结符。</value>
		public string Symbol { get; private set; }
		/// <summary>
		/// 获取默认被选择的冲突项。
		/// </summary>
		/// <value>默认被选择的冲突项。</value>
		public int Choice { get; private set; }
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			StringBuilder text = new StringBuilder();
			text.Append("state ");
			text.Append(State);
			text.Append(" conflicts on ");
			text.AppendLine(Symbol);
			for (int j = 0; j < Count; j++)
			{
				text.Append("	");
				text.AppendLine(this[j].ToString());
			}
			text.Append("	default ");
			text.AppendLine(this[Choice].Action.ToString());
			return text.ToString();
		}
	}
}
