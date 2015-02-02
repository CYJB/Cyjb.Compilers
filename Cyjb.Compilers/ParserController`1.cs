using System.Collections.Generic;
using Cyjb.Collections.ObjectModel;
using Cyjb.IO;
using Cyjb.Text;

namespace Cyjb.Compilers
{
	/// <summary>
	/// 表示词法单元分析器的控制器。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	public sealed class ParserController<T> : ReadOnlyList<Token<T>>, ISourceLocatable
		where T : struct
	{
		/// <summary>
		/// 初始化 <see cref="ParserController&lt;T&gt;"/> 类的新实例。
		/// </summary>
		internal ParserController() { }
		/// <summary>
		/// 获取词法单元的起始位置。
		/// </summary>
		/// <value>词法单元的起始位置。</value>
		public SourcePosition Start { get; private set; }
		/// <summary>
		/// 获取词法单元的结束位置。
		/// </summary>
		/// <value>词法单元的结束位置。</value>
		public SourcePosition End { get; private set; }
		/// <summary>
		/// 向 <see cref="ParserController&lt;T&gt;"/> 中添加指定堆栈中的词法单元。
		/// </summary>
		/// <param name="stack">要添加词法单元的堆栈。</param>
		/// <param name="count">要添加的词法单元的数量。</param>
		internal void InternalAdd(Stack<Token<T>> stack, int count)
		{
			while (this.Count > count)
			{
				base.Items.RemoveAt(this.Count - 1);
			}
			while (this.Count < count)
			{
				base.Items.Add(null);
			}
			if (count == 0)
			{
				if (stack.Count == 0)
				{
					this.Start = SourcePosition.Unknown;
				}
				else
				{
					this.Start = stack.Peek().Start;
				}
				this.End = this.Start;
			}
			else
			{
				while (count-- > 0)
				{
					base.Items[count] = stack.Pop();
				}
				SourceRange range = SourceRange.Merge((IEnumerable<ISourceLocatable>)this);
				this.Start = range.Start;
				this.End = range.End;
			}
		}
	}
}
