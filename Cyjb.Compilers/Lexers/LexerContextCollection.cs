using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compilers.Lexers
{
	/// <summary>
	/// 表示词法分析器上下文的集合。
	/// </summary>
	internal sealed class LexerContextCollection : KeyedCollectionBase<string, LexerContext>
	{
		/// <summary>
		/// 初始化 <see cref="LexerContextCollection"/> 类的新实例。
		/// </summary>
		internal LexerContextCollection() : base(true) { }
		/// <summary>
		/// 定义一个新的词法分析器的上下文。
		/// </summary>
		/// <param name="label">上下文的标签。</param>
		internal void DefineContext(string label)
		{
			if (this.Contains(label))
			{
				throw ExceptionHelper.KeyDuplicate("label");
			}
			LexerContext item = new LexerContext(base.Count, label, LexerContextType.Exclusive);
			base.AddItem(item);
		}
		/// <summary>
		/// 定义一个新的词法分析器的包含型上下文。
		/// </summary>
		/// <param name="label">上下文的标签。</param>
		internal void DefineInclusiveContext(string label)
		{
			if (this.Contains(label))
			{
				throw ExceptionHelper.KeyDuplicate("label");
			}
			LexerContext item = new LexerContext(base.Count, label, LexerContextType.Inclusive);
			base.AddItem(item);
		}

		#region KeyedCollectionBase<string, LexerContext> 成员

		/// <summary>
		/// 从指定元素提取键。
		/// </summary>
		/// <param name="item">从中提取键的元素。</param>
		/// <returns>指定元素的键。</returns>
		protected override string GetKeyForItem(LexerContext item)
		{
			return item.Label;
		}

		#endregion // KeyedCollectionBase<string, LexerContext> 成员

	}
}