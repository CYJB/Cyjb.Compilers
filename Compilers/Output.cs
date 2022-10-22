using System.Text;

namespace Cyjb.Compilers;

/// <summary>
/// 提供文本输出的工具方法。
/// </summary>
internal class Output
{
	/// <summary>
	/// 返回索引的宽度。
	/// </summary>
	/// <param name="count">索引的总个数。</param>
	/// <returns>索引位置的宽度。</returns>
	public static int GetIndexWidth(int count)
	{
		if (count <= 10)
		{
			return 1;
		}
		return (int)Math.Log10(count - 1) + 1;
	}

	/// <summary>
	/// 输出指定的表格。
	/// </summary>
	/// <param name="text">要输出到的文本。</param>
	/// <param name="columns">表格的列定义。</param>
	/// <param name="table">表格数据。</param>
	public static void AppendTable(StringBuilder text, IList<OutputTableColumn> columns, IList<string[]> table)
	{
		int rowCount = table.Count;
		int indexWidth = GetIndexWidth(rowCount);
		text.Append(new string(' ', indexWidth));
		int columnCount = columns.Count;
		int[] widths = new int[columnCount];
		for (int i = 0; i < columnCount; i++)
		{
			OutputTableColumn column = columns[i];
			string name = column.Name;
			int width = 0;
			if (column.IsFixedWidth)
			{
				width = table.Select((row) => row[i].Length).Max();
				if (width < name.Length)
				{
					width = name.Length;
				}
				// 使用两个空格分割列。
				width += 2;
				widths[i] = width;
			}
			text.AppendFormat(name.PadLeft(width));
		}
		text.AppendLine();
		for (int i = 0; i < rowCount; i++)
		{
			string[] row = table[i];
			text.AppendFormat(i.ToString().PadLeft(indexWidth));
			for (int j = 0; j < columnCount; j++)
			{
				text.AppendFormat(row[j].PadLeft(widths[j]));
			}
			text.AppendLine();
		}
	}
}
