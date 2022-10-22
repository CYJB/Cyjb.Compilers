using System.Reflection;

namespace Cyjb.Text;

public partial struct Token<T>
{
	/// <summary>
	/// 词法单元类型的显示名称映射表。
	/// </summary>
	private static readonly Lazy<Dictionary<T, string>> displayNames = new(() =>
	{
		Dictionary<T, string> result = new();
		try
		{
			foreach (FieldInfo field in typeof(T).GetFields())
			{
				TokenDisplayNameAttribute? attr = field.GetCustomAttribute<TokenDisplayNameAttribute>();
				if (attr != null && Enum.TryParse(field.Name, out T value))
				{
					result[value] = attr.DisplayName;
				}
			}
		}
		catch (ArgumentException)
		{
			// T 不是枚举。
		}
		return result;
	});

	/// <summary>
	/// 返回指定词法单元类型的显示名称。
	/// </summary>
	/// <param name="kind">要检查的词法单元类型。</param>
	/// <returns>指定词法单元类型的显示名称。</returns>
	public static string GetDisplayName(T kind)
	{
		if (EqualityComparer<T>.Default.Equals(kind, EndOfFile))
		{
			return "<<EOF>>";
		}
		if (displayNames.Value.TryGetValue(kind, out string? name))
		{
			return name;
		}
		else
		{
			return kind.ToString()!;
		}
	}
}
