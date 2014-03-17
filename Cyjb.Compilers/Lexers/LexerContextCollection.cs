using System.Collections;
using System.Collections.Generic;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compilers.Lexers
{
	/// <summary>
	/// 表示词法分析器上下文的集合。
	/// </summary>
	internal sealed class LexerContextCollection : KeyedCollectionBase<string, LexerContext>
	{
		/// <summary>
		/// 上下文的标签集合。
		/// </summary>
		private ICollection<string> labels;
		/// <summary>
		/// 初始化 <see cref="LexerContextCollection"/> 类的新实例。
		/// </summary>
		public LexerContextCollection() : base(true) { }
		/// <summary>
		/// 获取上下文的标签集合。
		/// </summary>
		/// <value>上下文的标签集合。</value>
		public ICollection<string> Labels
		{
			get
			{
				if (this.labels == null)
				{
					this.labels = new LabelCollection(this);
				}
				return this.labels;
			}
		}
		/// <summary>
		/// 定义一个新的词法分析器的上下文。
		/// </summary>
		/// <param name="label">上下文的标签。</param>
		public void DefineContext(string label)
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
		public void DefineInclusiveContext(string label)
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

		/// <summary>
		/// 上下文的标签集合。
		/// </summary>
		private sealed class LabelCollection : ICollection<string>
		{
			/// <summary>
			/// 上下文集合。
			/// </summary>
			private LexerContextCollection contexts;
			/// <summary>
			/// 使用上下文的集合初始化 <see cref="LabelCollection"/> 类的新实例。
			/// </summary>
			/// <param name="contexts">上下文集合。</param>
			public LabelCollection(LexerContextCollection contexts)
			{
				this.contexts = contexts;
			}

			#region ICollection<string> 成员

			/// <summary>
			/// 将某元素添加到 <see cref="LabelCollection"/> 中。
			/// </summary>
			/// <param name="item">要添加到 <see cref="LabelCollection"/> 的元素。</param>
			/// <exception cref="System.NotSupportedException">
			/// <see cref="LabelCollection"/> 是只读的。</exception>
			public void Add(string item)
			{
				throw ExceptionHelper.MethodNotSupported();
			}
			/// <summary>
			/// 从 <see cref="LabelCollection"/> 中移除所有元素。
			/// </summary>
			/// <exception cref="System.NotSupportedException">
			/// <see cref="LabelCollection"/> 是只读的。</exception>
			public void Clear()
			{
				throw ExceptionHelper.MethodNotSupported();
			}
			/// <summary>
			/// 确定 <see cref="LabelCollection"/> 是否包含特定值。
			/// </summary>
			/// <param name="item">要在 <see cref="LabelCollection"/> 中定位的对象。</param>
			/// <returns>如果在 <see cref="LabelCollection"/> 中找到 <paramref name="item"/>，
			/// 则为 <c>true</c>；否则为 <c>false</c>。</returns>
			public bool Contains(string item)
			{
				return this.contexts.Contains(item);
			}
			/// <summary>
			/// 从特定的 <see cref="System.Array"/> 索引处开始，
			/// 将 <see cref="LabelCollection"/> 的元素复制到一个 <see cref="System.Array"/> 中。
			/// </summary>
			/// <param name="array">作为从 <see cref="LabelCollection"/> 
			/// 复制的元素的目标位置的一维 <see cref="System.Array"/>。
			/// <see cref="System.Array"/> 必须具有从零开始的索引。</param>
			/// <param name="arrayIndex"><paramref name="array"/> 中从零开始的索引，在此处开始复制。</param>
			/// <exception cref="System.ArgumentNullException">
			/// <paramref name="array"/> 为 <c>null</c>。</exception>
			/// <exception cref="System.ArgumentOutOfRangeException">
			/// <paramref name="arrayIndex"/> 小于零。</exception>
			/// <exception cref="System.ArgumentException">
			/// <paramref name="array"/> 是多维的。</exception>
			/// <exception cref="System.ArgumentException">源 <see cref="LabelCollection"/> 
			/// 中的元素数目大于从 <paramref name="arrayIndex"/> 到目标 <paramref name="array"/> 
			/// 末尾之间的可用空间。</exception>
			public void CopyTo(string[] array, int arrayIndex)
			{
				ExceptionHelper.CheckArgumentNull(array, "array");
				ExceptionHelper.CheckFlatArray(array, "array");
				if (arrayIndex < 0)
				{
					throw ExceptionHelper.ArgumentOutOfRange("arrayIndex");
				}
				if (array.Length - arrayIndex < this.Count)
				{
					throw ExceptionHelper.ArrayTooSmall("array");
				}
				foreach (LexerContext context in this.contexts)
				{
					array[arrayIndex++] = context.Label;
				}
			}
			/// <summary>
			/// 获取 <see cref="LabelCollection"/> 中包含的元素数。
			/// </summary>
			/// <value><see cref="LabelCollection"/> 中包含的元素数。</value>
			public int Count
			{
				get { return this.contexts.Count; }
			}
			/// <summary>
			/// 获取一个值，该值指示 <see cref="LabelCollection"/> 是否为只读。
			/// </summary>
			/// <value>如果 <see cref="LabelCollection"/> 为只读，则为 <c>true</c>；
			/// 否则为 <c>false</c>。</value>
			public bool IsReadOnly
			{
				get { return true; }
			}
			/// <summary>
			/// 从 <see cref="LabelCollection"/> 中移除特定对象的第一个匹配项。
			/// </summary>
			/// <param name="item">要从 <see cref="LabelCollection"/> 中移除的对象。</param>
			/// <returns>如果已从 <see cref="LabelCollection"/> 中成功移除 <paramref name="item"/>，
			/// 则为 <c>true</c>；否则为 <c>false</c>。如果在原始 <see cref="LabelCollection"/> 
			/// 中没有找到 <paramref name="item"/>，该方法也会返回 <c>false</c>。</returns>
			/// <exception cref="System.NotSupportedException">
			/// <see cref="LabelCollection"/> 是只读的。</exception>
			public bool Remove(string item)
			{
				throw ExceptionHelper.MethodNotSupported();
			}

			#endregion

			#region IEnumerable<string> 成员

			/// <summary>
			/// 返回一个循环访问集合的枚举器。
			/// </summary>
			/// <returns>可用于循环访问集合的 <see cref="System.Collections.Generic.IEnumerator&lt;T&gt;"/>。</returns>
			public IEnumerator<string> GetEnumerator()
			{
				foreach (LexerContext context in this.contexts)
				{
					yield return context.Label;
				}
			}

			#endregion

			#region IEnumerable 成员

			/// <summary>
			/// 返回一个循环访问集合的枚举器。
			/// </summary>
			/// <returns>可用于循环访问集合的 <see cref="System.Collections.IEnumerator"/>。</returns>
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#endregion
		}
	}
}