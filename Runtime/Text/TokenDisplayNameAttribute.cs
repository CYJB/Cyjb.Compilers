namespace Cyjb.Text;

/// <summary>
/// 表示词法单元类型的显示名称。
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class TokenDisplayNameAttribute : Attribute
{
	/// <summary>
	/// 使用指定的显示名称初始化 <see cref="TokenDisplayNameAttribute"/> 类的新实例。
	/// </summary>
	/// <param name="displayName">词法单元类型的显示名称。</param>
	public TokenDisplayNameAttribute(string displayName)
	{
		DisplayName = displayName;
	}

	/// <summary>
	/// 获取词法单元类型的显示名称。
	/// </summary>
	public string DisplayName { get; }
}
