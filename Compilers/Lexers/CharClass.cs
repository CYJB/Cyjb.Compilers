using Cyjb.Collections;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示一个字符类。
/// </summary>
public class CharClass
{
	/// <summary>
	/// 使用指定的字符集合初始化。
	/// </summary>
	/// <param name="index">当前字符类的索引。</param>
	/// <param name="chars">当前字符类包含的字符集合。</param>
	internal CharClass(int index, CharSet chars)
	{
		Index = index;
		Chars = chars;
		Containers = new HashSet<CharClassSet>();
	}

	/// <summary>
	/// 获取当前字符类的索引。
	/// </summary>
	public int Index { get; internal set; }
	/// <summary>
	/// 获取当前字符类包含的字符集合。
	/// </summary>
	public CharSet Chars { get; }
	/// <summary>
	/// 获取包含当前字符类的字符类集合。
	/// </summary>
	internal ISet<CharClassSet> Containers { get; }

	/// <summary>
	/// 分割当前字符类。
	/// </summary>
	/// <param name="index">新字符类的索引。</param>
	/// <param name="chars">新字符类包含的字符集合。</param>
	/// <returns>新的字符类。</returns>
	internal CharClass Split(int index, CharSet chars)
	{
		// 1. 剔除被分割的部分。
		Chars.ExceptWith(chars);
		// 2. 创建新字符类。
		CharClass newItem = new(index, chars);
		if (chars.Count > 1)
		{
			// 复制所属字符类集合。
			newItem.Containers.UnionWith(Containers);
		}
		// 3. 更新现有字符类。
		foreach (CharClassSet charClass in Containers)
		{
			charClass.Add(newItem);
		}
		// 4. 如果分割后的现有字符类只包含一个字符，无法继续分割，那么可以移除 Containers
		if (Chars.Count == 1)
		{
			Containers.Clear();
		}
		return newItem;
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		return Chars.ToString();
	}
}
