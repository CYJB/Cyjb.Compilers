using System.Diagnostics;
using System.Globalization;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示字符类的映射。
/// </summary>
[CLSCompliant(false)]
public sealed class CharClassMap
{
	/// <summary>
	/// 范围索引。
	/// </summary>
	/// <remarks>索引的高 16 位表示起始字符（包含），低 16 位表示结束字符（包含）</remarks>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly uint[] indexes;
	/// <summary>
	/// 字符类。
	/// </summary>
	/// <remarks>前 128 个位置表示 ASCII 范围内的字符类，之后为范围索引对应的字符类。</remarks>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly int[] charClasses;
	/// <summary>
	/// Unicode 类别对应的字符类。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IReadOnlyDictionary<UnicodeCategory, int>? categories;

	/// <summary>
	/// 使用指定的范围索引和字符类初始化。
	/// </summary>
	/// <param name="indexes">范围索引。</param>
	/// <param name="charClasses">字符类。</param>
	/// <param name="categories">Unicode 类别对应的字符类。</param>
	public CharClassMap(uint[] indexes, int[] charClasses,
		IReadOnlyDictionary<UnicodeCategory, int>? categories = null)
	{
		this.indexes = indexes;
		this.charClasses = charClasses;
		this.categories = categories;
	}

	/// <summary>
	/// 获取范围索引。
	/// </summary>
	/// <remarks>索引的高 16 位表示起始字符（包含），低 16 位表示结束字符（包含）</remarks>
	public uint[] Indexes => indexes;

	/// <summary>
	/// 获取字符类。
	/// </summary>
	/// <remarks>前 128 个位置表示 ASCII 范围内的字符类，之后为范围索引对应的字符类。
	/// 如果值小于 0，表示需要将范围映射到的索引。</remarks>
	public int[] CharClasses => charClasses;

	/// <summary>
	/// 获取 Unicode 类别对应的字符类。
	/// </summary>
	public IReadOnlyDictionary<UnicodeCategory, int>? Categories => categories;

	/// <summary>
	/// 返回指定字符所属的字符类。
	/// </summary>
	/// <param name="ch">要获取所属字符类的字符。</param>
	/// <returns>字符所属的字符类，使用 <c>-1</c> 表示未找到字符类。</returns>
	public int GetCharClass(char ch)
	{
		// ASCII 范围直接读取。
		if (ch < '\x80')
		{
			return charClasses[ch];
		}
		uint index = ((uint)ch << 0x10) | 0xFFFF;
		int charClassIdx = Array.BinarySearch(indexes, index);
		if (charClassIdx < 0)
		{
			charClassIdx = (~charClassIdx) - 1;
			if (charClassIdx < 0 || (indexes[charClassIdx] & 0xFFFF) < ch)
			{
				// 未找到字符类。
				if (categories != null)
				{
					// 检查 Unicode 类别。
					if (categories.TryGetValue(char.GetUnicodeCategory(ch), out int result))
					{
						return result;
					}
				}
				return -1;
			}
		}
		int charClass = charClasses[charClassIdx + 0x80];
		if (charClass < -1)
		{
			// 是范围映射。
			uint start = (indexes[charClassIdx] >> 0x10) & 0xFFFF;
			charClass = charClasses[ch - start - charClass];
		}
		return charClass;
	}
}
