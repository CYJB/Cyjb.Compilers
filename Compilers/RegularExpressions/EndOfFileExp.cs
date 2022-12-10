using System.Text;

namespace Cyjb.Compilers.RegularExpressions;

/// <summary>
/// 表示文件结束的正则表达式，只能用于先前看表达式中。
/// </summary>
public sealed class EndOfFileExp : LexRegex
{
	/// <summary>
	/// 初始化 <see cref="EndOfFileExp"/> 类的新实例。
	/// </summary>
	internal EndOfFileExp() { }

	/// <summary>
	/// 获取当前正则表达式匹配的字符串长度。
	/// </summary>
	/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>null</c>。</value>
	public override int? Length => 0;

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Equals(LexRegex? other)
	{
		return (other is EndOfFileExp);
	}

	/// <summary>
	/// 返回当前对象的哈希值。
	/// </summary>
	/// <returns>当前对象的哈希值。</returns>
	public override int GetHashCode()
	{
		return typeof(EndOfFileExp).GetHashCode();
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <param name="builder">字符串构造器。</param>
	internal override void ToString(StringBuilder builder)
	{
		builder.Append("{EOF}");
	}
}
