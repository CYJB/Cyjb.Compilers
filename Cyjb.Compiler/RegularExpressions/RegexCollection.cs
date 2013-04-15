using System.Collections;
using System.Collections.Generic;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示正则表达式的集合。
	/// </summary>
	public sealed class RegexCollection : ICollection<Regex>
	{
		/// <summary>
		/// 储存正则表达式的字典。
		/// </summary>
		private Dictionary<string, Regex> dict = new Dictionary<string, Regex>();
		/// <summary>
		/// 初始化 <see cref="RegexCollection"/> 类的新实例。
		/// </summary>
		internal RegexCollection() { }
		/// <summary>
		/// 获取定义正则表达式的字典。
		/// </summary>
		internal Dictionary<string, Regex> Regexs
		{
			get { return dict; }
		}
		/// <summary>
		/// 定义一个特定名称的正则表达式。
		/// </summary>
		/// <param name="name">正则表达式的名称。</param>
		/// <param name="regex">定义的正则表达式。</param>
		internal void DefineRegex(string name, Regex regex)
		{
			dict.Add(name, regex);
		}
		/// <summary>
		/// 获取具有指定名称的正则表达式。
		/// </summary>
		/// <param name="name">要获取的正则表达式的名称。</param>
		/// <returns>具有指定名称的正则表达式。</returns>
		public Regex this[string name]
		{
			get { return dict[name]; }
		}
		/// <summary>
		/// 返回是否存在具有指定名称的正则表达式。
		/// </summary>
		/// <param name="name">要测试的正则表达式的名称。</param>
		/// <returns>如果存在具有指定名称的正则表达式，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		public bool Contains(string name)
		{
			return dict.ContainsKey(name);
		}
		/// <summary>
		/// 获取具有指定名称的正则表达式。
		/// </summary>
		/// <param name="name">要获取的正则表达式的名称。</param>
		/// <param name="regex">当此方法返回值时，如果找到该名称，便会返回与指定的名称相关联的正则表达式；
		/// 否则，则会返回 <c>null</c>。</param>
		/// <returns>如果 <see cref="RegexCollection"/> 包含具有指定名称的正则表达式，则为 <c>true</c>；
		/// 否则为 <c>false</c>。</returns>
		public bool TryGetValue(string name, out Regex regex)
		{
			return dict.TryGetValue(name, out regex);
		}

		#region ICollection<Regex> 成员

		/// <summary>
		/// 获取 <see cref="RegexCollection"/> 中包含的元素数。
		/// </summary>
		/// <value><see cref="RegexCollection"/> 中包含的元素数。</value>
		public int Count
		{
			get { return dict.Count; }
		}
		/// <summary>
		/// 获取一个值，该值指示 <see cref="RegexCollection"/> 是否为只读。
		/// </summary>
		/// <value>如果 <see cref="RegexCollection"/> 为只读，则为 <c>true</c>；否则为 <c>false</c>。</value>
		public bool IsReadOnly
		{
			get { return true; }
		}

		/// <summary>
		/// 将某项添加到 <see cref="RegexCollection"/> 中。此实现总是引发 <see cref="System.NotSupportedException"/>。
		/// </summary>
		/// <param name="item">要添加到 <see cref="RegexCollection"/> 的对象。</param>
		/// <exception cref="System.NotSupportedException">总是引发。</exception>
		void ICollection<Regex>.Add(Regex item)
		{
			throw ExceptionHelper.ReadOnlyCollection();
		}

		/// <summary>
		/// 从 <see cref="RegexCollection"/> 中移除所有项。此实现总是引发 <see cref="System.NotSupportedException"/>。
		/// </summary>
		/// <exception cref="System.NotSupportedException">总是引发。</exception>
		void ICollection<Regex>.Clear()
		{
			throw ExceptionHelper.ReadOnlyCollection();
		}

		/// <summary>
		/// 确定 <see cref="RegexCollection"/> 是否包含特定值。
		/// </summary>
		/// <param name="item">要在 <see cref="RegexCollection"/> 中定位的对象。</param>
		public bool Contains(Regex item)
		{
			return dict.ContainsValue(item);
		}

		/// <summary>
		/// 从特定的 <see cref="System.Array"/> 索引处开始，将 <see cref="RegexCollection"/> 的元素复制到一个 <see cref="System.Array"/> 中。
		/// </summary>
		/// <param name="array">作为从 <see cref="RegexCollection"/> 复制的元素的目标位置的一维 <see cref="System.Array"/>。<see cref="System.Array"/> 必须具有从零开始的索引。</param>
		/// <param name="arrayIndex"><paramref name="array"/> 中从零开始的索引，在此处开始复制。</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="array"/> 为 <c>null</c>。</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> 小于零。</exception>
		/// <exception cref="System.ArgumentException"><paramref name="array"/> 是多维的。</exception>
		/// <exception cref="System.ArgumentException">源 <see cref="RegexCollection"/> 中的元素数目大于从 <paramref name="arrayIndex"/> 到目标 <paramref name="array"/> 末尾之间的可用空间。</exception>
		public void CopyTo(Regex[] array, int arrayIndex)
		{
			//ExceptionHelper.CheckArgumentNull(array, "array");
			//ExceptionHelper.CheckFlatArray(array);
			//if (arrayIndex < 0)
			//{
			//	throw ExceptionHelper.ArgumentOutOfRange("arrayIndex");
			//}
			//if (array.Length - arrayIndex < this.Count)
			//{
			//	throw ExceptionHelper.ArrayPlusOffTooSmall();
			//}
			//foreach (Regex obj in this)
			//{
			//	array[arrayIndex++] = obj;
			//}
		}

		/// <summary>
		/// 从 <see cref="RegexCollection"/> 中移除特定对象的第一个匹配项。此实现总是引发 <see cref="System.NotSupportedException"/>。
		/// </summary>
		/// <param name="item">要从 <see cref="RegexCollection"/> 中移除的对象。</param>
		/// <returns>如果已从 <see cref="RegexCollection"/> 中成功移除 <paramref name="item"/>，则为 <c>true</c>；否则为 <c>false</c>。如果在原始 <see cref="RegexCollection"/> 中没有找到 <paramref name="item"/>，该方法也会返回 <c>false</c>。</returns>
		/// <exception cref="System.NotSupportedException">总是引发。</exception>
		bool ICollection<Regex>.Remove(Regex item)
		{
			throw ExceptionHelper.ReadOnlyCollection();
		}

		#endregion // ICollection<Regex> 成员

		#region IEnumerable<Regex> 成员

		/// <summary>
		/// 返回一个循环访问集合的枚举器。
		/// </summary>
		/// <returns>可用于循环访问集合的 <see cref="System.Collections.Generic.IEnumerator&lt;T&gt;"/>。</returns>
		public IEnumerator<Regex> GetEnumerator()
		{
			return dict.Values.GetEnumerator();
		}

		#endregion // IEnumerable<Regex> 成员

		#region IEnumerable 成员

		/// <summary>
		/// 返回一个循环访问集合的枚举器。
		/// </summary>
		/// <returns>可用于循环访问集合的 <see cref="System.Collections.IEnumerator"/>。</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion // IEnumerable 成员

	}
}
