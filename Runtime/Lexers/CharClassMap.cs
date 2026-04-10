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
	private readonly short[] charClasses;
	/// <summary>
	/// Unicode 类别对应的字符类。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IReadOnlyDictionary<UnicodeCategory, short>? categories;

	/// <summary>
	/// 使用指定的范围索引和字符类初始化。
	/// </summary>
	/// <param name="indexes">范围索引。</param>
	/// <param name="charClasses">字符类。</param>
	/// <param name="categories">Unicode 类别对应的字符类。</param>
	public CharClassMap(uint[] indexes, short[] charClasses,
		IReadOnlyDictionary<UnicodeCategory, short>? categories = null)
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
	public short[] CharClasses => charClasses;

	/// <summary>
	/// 获取 Unicode 类别对应的字符类。
	/// </summary>
	public IReadOnlyDictionary<UnicodeCategory, short>? Categories => categories;

	/// <summary>
	/// 返回指定字符所属的字符类。
	/// </summary>
	/// <param name="ch">要获取所属字符类的字符。</param>
	/// <returns>字符所属的字符类，使用 <c>-1</c> 表示未找到字符类。</returns>
	public short GetCharClass(char ch)
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
					if (categories.TryGetValue(char.GetUnicodeCategory(ch), out short result))
					{
						return result;
					}
				}
				return -1;
			}
		}
		short charClass = charClasses[charClassIdx + 0x80];
		if (charClass < -1)
		{
			// 是范围映射。
			uint start = (indexes[charClassIdx] >> 0x10) & 0xFFFF;
			charClass = charClasses[ch - start - charClass];
		}
		return charClass;
	}

	/// <summary>
	/// 将从 <paramref name="start"/> 开始的连续字符的字符类填充到指定范围中。
	/// </summary>
	/// <param name="charClassMap">要填充的字符类映射范围。</param>
	/// <param name="start">要填充的起始字符，默认为 <c>'\0'</c>。</param>
	public void FillCharClass(Span<short> charClassMap, char start = '\0')
	{
		int len = charClassMap.Length;
		if (len == 0)
		{
			return;
		}
		int endIdx = start + len - 1;
		if (endIdx >= 0x10000)
		{
			throw CommonExceptions.ArgumentOutOfRange(start);
		}
		int startIdx = start;
		int copyIdx = 0;
		// 1. 处理 ASCII 部分 [0x00, 0x80)
		if (startIdx < 0x80)
		{
			int copyLen = Math.Min(startIdx + len, 0x80) - startIdx;
			charClasses.AsSpan(startIdx, copyLen).CopyTo(charClassMap);
			copyIdx += copyLen;
			if (copyIdx >= len)
			{
				return;
			}
			startIdx += copyLen;
		}
		// 2. 处理非 ASCII 部分 [0x80, 0x10000)
		// 二分查找首个可能包含 cur 的区间
		uint key = ((uint)startIdx << 0x10) | 0xFFFF;
		int rangeIdx = Array.BinarySearch(indexes, key);
		if (rangeIdx < 0)
		{
			rangeIdx = ~rangeIdx - 1;
			if (rangeIdx < 0)
			{
				rangeIdx = 0;
			}
		}
		for (int i = rangeIdx; i < indexes.Length; i++)
		{
			uint range = indexes[i];
			int rangeStart = (int)(range >> 0x10);
			int rangeEnd = (int)(range & 0xFFFF);
			if (rangeStart > endIdx)
			{
				break;
			}
			if (rangeEnd < startIdx)
			{
				continue;
			}
			// 处理当前区间之前的间隙（未覆盖的字符）
			if (startIdx < rangeStart)
			{
				int copyLen = rangeStart - startIdx;
				FillCharClassForUnicodeCategory(charClassMap.Slice(copyIdx, copyLen), startIdx);
				startIdx = rangeStart;
				copyIdx += copyLen;
			}
			// 处理区间与 [startIdx, endIdx] 的交集
			int overlapEnd = Math.Min(rangeEnd, endIdx);
			if (startIdx <= overlapEnd)
			{
				short charClass = charClasses[0x80 + i];
				int overlapLen = overlapEnd - startIdx + 1;
				if (charClass >= -1)
				{
					charClassMap.Slice(copyIdx, overlapLen).Fill(charClass);
					copyIdx += overlapLen;
				}
				else
				{
					// 是范围映射。
					int offsetIdx = -charClass;
					for (int ch = startIdx; ch <= overlapEnd; ch++)
					{
						int cls = charClasses[ch - rangeStart + offsetIdx];
						charClassMap[ch - start] = (short)cls;
					}
					int srcStart = startIdx - rangeStart - charClass;
					charClasses.AsSpan(srcStart, overlapLen).CopyTo(charClassMap[copyIdx..]);
					copyIdx += overlapLen;
				}
				startIdx = overlapEnd + 1;
			}
		}
		// 处理最后的间隙
		if (startIdx <= endIdx)
		{
			int copyLen = endIdx - startIdx + 1;
			FillCharClassForUnicodeCategory(charClassMap.Slice(copyIdx, copyLen), startIdx);
		}
	}

	/// <summary>
	/// 根据字符的 Unicode 类别填充字符类映射。。
	/// </summary>
	private void FillCharClassForUnicodeCategory(Span<short> charClassMap, int start)
	{
		if (categories == null)
		{
			// 没有 Unicode 类别，直接使用 -1 填充。
			charClassMap.Fill(-1);
			return;
		}
		for (int i = 0; i < charClassMap.Length; i++)
		{
			if (categories.TryGetValue(char.GetUnicodeCategory((char)(start + i)), out short charClass))
			{
				charClassMap[i] = charClass;
			}
			else
			{
				charClassMap[i] = -1;
			}
		}
	}
}
