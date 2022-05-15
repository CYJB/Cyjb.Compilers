using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示自动机中使用的字符类集合。
/// </summary>
public sealed class CharClassSet : CollectionBase<CharClass>
{
	/// <summary>
	/// 表示不包含任何字符类的集合。
	/// </summary>
	public static readonly CharClassSet Empty = new(new HashSet<char>());

	/// <summary>
	/// 当前集合包含的字符类。
	/// </summary>
	private readonly HashSet<CharClass> charClasses = new();

	/// <summary>
	/// 初始化字符类集合的新实例。
	/// </summary>
	internal CharClassSet(ISet<char> chars)
	{
		Chars = chars;
	}

	/// <summary>
	/// 当前字符类包含的字符集合。
	/// </summary>
	public ISet<char> Chars { get; }

	#region CollectionBase<CharClass> 成员

	/// <summary>
	/// 获取当前集合包含的元素数。
	/// </summary>
	/// <value>当前集合中包含的元素数。</value>
	public override int Count => charClasses.Count;

	/// <summary>
	/// 确定当前集合是否包含指定对象。
	/// </summary>
	/// <param name="item">要在当前集合中定位的对象。</param>
	/// <returns>如果在当前集合中找到 <paramref name="item"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Contains(CharClass item)
	{
		return charClasses.Contains(item);
	}

	/// <summary>
	/// 将指定对象添加到当前集合中。
	/// </summary>
	/// <param name="item">要添加到当前集合的对象。</param>
	protected override void AddItem(CharClass item)
	{
		charClasses.Add(item);
	}

	/// <summary>
	/// 从当前集合中移除特定对象的第一个匹配项。
	/// </summary>
	/// <param name="item">要从当前集合中移除的对象。</param>
	/// <returns>如果已从当前集合中成功移除 <paramref name="item"/>，则为 <c>true</c>；否则为 <c>false</c>。
	/// 如果在原始当前集合中没有找到 <paramref name="item"/>，该方法也会返回 <c>false</c>。</returns>
	public override bool Remove(CharClass item)
	{
		return charClasses.Remove(item);
	}

	/// <summary>
	/// 从当前集合中移除所有元素。
	/// </summary>
	public override void Clear()
	{
		charClasses.Clear();
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<CharClass> GetEnumerator()
	{
		return charClasses.GetEnumerator();
	}

	#endregion // CollectionBase<CharClass> 成员

}

