using Cyjb.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 特性列表构造器。
/// </summary>
internal sealed class AttributeListBuilder : ListBase<AttributeBuilder>
{
	/// <summary>
	/// 特性列表。
	/// </summary>
	private readonly List<AttributeBuilder> attributes = new();

	/// <summary>
	/// 构造特性语法节点。
	/// </summary>
	/// <param name="format">语法的格式信息。</param>
	/// <returns>特性语法节点。</returns>
	public SyntaxList<AttributeListSyntax> GetSyntax(SyntaxFormat format)
	{
		if (attributes.Count == 0)
		{
			return SyntaxFactory.List<AttributeListSyntax>();
		}
		return SyntaxFactory.List(attributes.Select(
			attr => attr.GetListSyntax(format)
				.WithLeadingTrivia(format.Indentation)
				.WithTrailingTrivia(format.EndOfLine)));
	}

	#region ListBase<AttributeBuilder> 成员

	/// <summary>
	/// 获取当前列表包含的元素数。
	/// </summary>
	/// <value>当前列表中包含的元素数。</value>
	public override int Count => attributes.Count;

	/// <summary>
	/// 将元素插入当前列表的指定索引处。
	/// </summary>
	/// <param name="index">从零开始的索引，应在该位置插入 <paramref name="item"/>。</param>
	/// <param name="item">要插入的对象。</param>
	protected override void InsertItem(int index, AttributeBuilder item)
	{
		attributes.Insert(index, item);
	}

	/// <summary>
	/// 移除当前列表指定索引处的元素。
	/// </summary>
	/// <param name="index">要移除的元素的从零开始的索引。</param>
	protected override void RemoveItem(int index)
	{
		attributes.RemoveAt(index);
	}

	/// <summary>
	/// 返回指定索引处的元素。
	/// </summary>
	/// <param name="index">要返回元素的从零开始的索引。</param>
	/// <returns>位于指定索引处的元素。</returns>
	protected override AttributeBuilder GetItemAt(int index)
	{
		return attributes[index];
	}

	/// <summary>
	/// 替换指定索引处的元素。
	/// </summary>
	/// <param name="index">待替换元素的从零开始的索引。</param>
	/// <param name="item">位于指定索引处的元素的新值。</param>
	protected override void SetItemAt(int index, AttributeBuilder item)
	{
		attributes[index] = item;
	}

	/// <summary>
	/// 确定当前列表中指定对象的索引。
	/// </summary>
	/// <param name="item">要在当前列表中定位的对象。</param>
	/// <returns>如果在当前列表中找到 <paramref name="item"/>，则为该对象的索引；否则为 <c>-1</c>。</returns>
	public override int IndexOf(AttributeBuilder item)
	{
		return attributes.IndexOf(item);
	}

	/// <summary>
	/// 从当前列表中移除所有元素。
	/// </summary>
	public override void Clear()
	{
		attributes.Clear();
	}

	/// <summary>
	/// 返回一个循环访问集合的枚举器。
	/// </summary>
	/// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<AttributeBuilder> GetEnumerator()
	{
		return attributes.GetEnumerator();
	}

	#endregion // ListBase<AttributeBuilder> 成员
}
