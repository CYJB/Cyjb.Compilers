namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示字符类的映射。
/// </summary>
public class CharClassMap
{
	/// <summary>
	/// 范围索引。
	/// </summary>
	/// <remarks>索引的高 16 位表示起始字符（包含），低 16 位表示结束字符（包含）</remarks>
	private readonly int[] indexes;
	/// <summary>
	/// 字符类。
	/// </summary>
	/// <remarks>，前 128 个位置表示 ASCII 范围内的字符类，之后为范围索引对应的字符类。</remarks>
	private readonly int[] charClasses;

	/// <summary>
	/// 使用指定的范围索引和字符类初始化。
	/// </summary>
	/// <param name="indexes">范围索引。</param>
	/// <param name="charClasses">字符类。</param>
	public CharClassMap(int[] indexes, int[] charClasses)
	{
		this.indexes = indexes;
		this.charClasses = charClasses;
	}

	/// <summary>
	/// 获取范围索引。
	/// </summary>
	/// <remarks>索引的高 16 位表示起始字符（包含），低 16 位表示结束字符（包含）</remarks>
	public int[] Indexes => indexes;

	/// <summary>
	/// 获取字符类。
	/// </summary>
	/// <remarks>前 127 个位置表示 ASCII 范围内的字符类，之后为范围索引对应的字符类。
	/// 如果值小于 0，表示需要将范围映射到的索引。</remarks>
	public int[] CharClasses => charClasses;

	/// <summary>
	/// 返回指定字符所属的字符类。
	/// </summary>
	/// <param name="ch">要获取所属字符类的字符。</param>
	/// <returns>字符所属的字符类。</returns>
	public int GetCharClass(char ch)
	{
		// ASCII 范围直接读取。
		if (ch < '\x80')
		{
			return charClasses[ch];
		}
		else
		{
			int index = (ch << 0x10) | 0xFFFF;
			int charClassIdx = Array.BinarySearch(indexes, index);
			if (charClassIdx < 0)
			{
				charClassIdx = ~charClassIdx;
				if (charClassIdx == 0)
				{
					// 未找到字符类。
					return 0;
				}
				charClassIdx--;
				if ((indexes[charClassIdx] & 0xFFFF) < ch)
				{
					// 未找到字符类。
					return 0;
				}
			}
			index = charClasses[charClassIdx + 0x80];
			if (index < 0)
			{
				// 是范围映射。
				int start = (indexes[charClassIdx] >> 0x10) & 0xFFFF;
				index = charClasses[ch - start - index];
			}
			return index;
		}
	}
}
