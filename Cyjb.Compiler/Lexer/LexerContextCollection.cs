using System;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示词法分析器上下文的集合。
	/// </summary>
	[Serializable]
	public sealed class LexerContextCollection : KeyedListBase<string, LexerContext>
	{
		/// <summary>
		/// 初始化 <see cref="LexerContextCollection"/> 类的新实例。
		/// </summary>
		public LexerContextCollection()
		{ }
		/// <summary>
		/// 使用有序的上下文列表初始化 <see cref="LexerContextCollection"/> 类的新实例。
		/// </summary>
		/// <param name="contexts">上下文的有序列表。</param>
		public LexerContextCollection(params LexerContext[] contexts)
		{
			for (int i = 0; i < contexts.Length; i++)
			{
				base.InsertItem(i, contexts[i]);
			}
		}
		/// <summary>
		/// 定义一个新的词法分析器的上下文。
		/// </summary>
		/// <param name="label">上下文的标签。</param>
		/// <returns>词法分析器的上下文。</returns>
		public LexerContext DefineContext(string label)
		{
			if (this.Contains(label))
			{
				throw ExceptionHelper.KeyDuplicate("label");
			}
			LexerContext item = new LexerContext(base.Count, label, LexerContextType.Exclusive);
			base.InsertItem(base.Count, item);
			return item;
		}
		/// <summary>
		/// 定义一个新的词法分析器的上下文。
		/// </summary>
		/// <returns>词法分析器的上下文。</returns>
		public LexerContext DefineContext()
		{
			return DefineContext(string.Concat("Context#", base.Count));
		}
		/// <summary>
		/// 定义一个新的词法分析器的包含型上下文。
		/// </summary>
		/// <param name="label">上下文的标签。</param>
		/// <returns>词法分析器的上下文。</returns>
		public LexerContext DefineInclusiveContext(string label)
		{
			if (this.Contains(label))
			{
				throw ExceptionHelper.KeyDuplicate("label");
			}
			LexerContext item = new LexerContext(base.Count, label, LexerContextType.Inclusive);
			base.InsertItem(base.Count, item);
			return item;
		}
		/// <summary>
		/// 定义一个新的词法分析器的包含型上下文。
		/// </summary>
		/// <returns>词法分析器的上下文。</returns>
		public LexerContext DefineInclusiveContext()
		{
			return DefineInclusiveContext(string.Concat("Context#", base.Count));
		}

		#region KeyedListBase<string, LexerContext> 成员

		/// <summary>
		/// 从指定元素提取键。
		/// </summary>
		/// <param name="item">从中提取键的元素。</param>
		/// <returns>指定元素的键。</returns>
		protected override string GetKeyForItem(LexerContext item)
		{
			return item.Label;
		}

		#endregion // KeyedListBase<string, LexerContext> 成员

		#region ListBase<LexerContext> 成员

		/// <summary>
		/// 确定 <see cref="LexerContextCollection"/> 中特定项的索引。
		/// </summary>
		/// <param name="item">要在 <see cref="LexerContextCollection"/> 中定位的对象。</param>
		/// <returns>如果在 <see cref="LexerContextCollection"/> 中找到 <paramref name="item"/>，
		/// 则为该项的索引；否则为 <c>-1</c>。</returns>
		public override int IndexOf(LexerContext item)
		{
			int idx = item.Index;
			if (idx >= 0 && idx < this.Count)
			{
				if (this[idx] == item)
				{
					return idx;
				}
			}
			return -1;
		}

		#endregion // ListBase<LexerContext> 成员

	}
}