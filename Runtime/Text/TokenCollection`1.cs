using Cyjb.Collections;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Text;

/// <summary>
/// 表示词法单元集合。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public class TokenCollection<T> : CollectionBase<Token<T>>
	where T : struct
{
	/// <summary>
	/// 词法单元列表。
	/// </summary>
	private readonly List<Token<T>> tokens = new();

	/// <summary>
	/// 返回指定范围内的所有词法单元。
	/// </summary>
	/// <param name="span">要检查的词法单元范围。</param>
	/// <returns>指定范围内的所有词法单元。</returns>
	public IEnumerable<Token<T>> GetTokens(TextSpan span)
	{
		int index = tokens.BinarySearch(span, (token) => token.Span);
		if (index < 0)
		{
			index = ~index;
		}
		int end = span.End;
		while (index < tokens.Count)
		{
			Token<T> token = tokens[index];
			if (token.Span.End <= end)
			{
				yield return token;
			}
			else
			{
				break;
			}
		}
	}

	#region CollectionBase<Token<T>> 成员

	/// <summary>
	/// 获取当前集合包含的元素数。
	/// </summary>
	/// <value>当前集合中包含的元素数。</value>
	public override int Count => tokens.Count;

	/// <summary>
	/// 确定当前集合是否包含指定对象。
	/// </summary>
	/// <param name="item">要在当前集合中定位的对象。</param>
	/// <returns>如果在当前集合中找到 <paramref name="item"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Contains(Token<T> item)
	{
		int index = tokens.BinarySearch(item, TokenSpanComparer<T>.Instance);
		return index >= 0 && tokens[index] == item;
	}

	/// <summary>
	/// 将指定对象添加到当前集合中。
	/// </summary>
	/// <param name="item">要添加到当前集合的对象。</param>
	protected override void AddItem(Token<T> item)
	{
		if (tokens.Count == 0)
		{
			tokens.Add(item);
		}
		else if (item.Span.Start >= tokens[^1].Span.End)
		{
			tokens.Add(item);
		}
		else
		{
			int index = tokens.BinarySearch(item, TokenSpanComparer<T>.Instance);
			if (index >= 0)
			{
				if (tokens[index] == item)
				{
					// 避免重复添加词法单元。
					return;
				}
			}
			else
			{
				index = ~index;
			}
			tokens.Insert(index, item);
		}
	}

	/// <summary>
	/// 从当前集合中移除特定对象的第一个匹配项。
	/// </summary>
	/// <param name="item">要从当前集合中移除的对象。</param>
	/// <returns>如果已从当前集合中成功移除 <paramref name="item"/>，则为 <c>true</c>；否则为 <c>false</c>。
	/// 如果在原始当前集合中没有找到 <paramref name="item"/>，该方法也会返回 <c>false</c>。</returns>
	public override bool Remove(Token<T> item)
	{
		int index = tokens.BinarySearch(item, TokenSpanComparer<T>.Instance);
		if (index >= 0 && tokens[index] == item)
		{
			tokens.RemoveAt(index);
			return true;
		}
		return false;
	}

	/// <summary>
	/// 从当前集合中移除所有元素。
	/// </summary>
	public override void Clear()
	{
		tokens.Clear();
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<Token<T>> GetEnumerator()
	{
		return tokens.GetEnumerator();
	}

	#endregion // CollectionBase<Token<T>> 成员

}
