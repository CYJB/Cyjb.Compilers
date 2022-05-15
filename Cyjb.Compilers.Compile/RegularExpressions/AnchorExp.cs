using System.Diagnostics;
using System.Text;

namespace Cyjb.Compilers.RegularExpressions;

/// <summary>
/// 表示定位点的正则表达式。
/// </summary>
public sealed class AnchorExp : LexRegex
{
	/// <summary>
	/// 表示行的结尾的正则表达式。
	/// </summary>
	internal static new readonly LexRegex EndOfLine = Concat(Symbol('\r').Optional(), Symbol('\n'));

	/// <summary>
	/// 内部正则表达式。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly LexRegex innerExp;

	/// <summary>
	/// 使用内部正则表达式初始化 <see cref="AnchorExp"/> 类的新实例。
	/// </summary>
	/// <param name="innerExp">包含的内部正则表达式。</param>
	internal AnchorExp(LexRegex innerExp)
	{
		CheckRegex(innerExp);
		this.innerExp = innerExp;
		BeginningOfLine = false;
	}

	/// <summary>
	/// 获取当前正则表达式匹配的字符串长度。
	/// </summary>
	/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>null</c>。</value>
	public override int? Length => innerExp.Length;

	/// <summary>
	/// 获取内部的正则表达式。
	/// </summary>
	/// <value>内部的正则表达式。</value>
	public LexRegex InnerExpression => innerExp;

	/// <summary>
	/// 获取或设置是否定位到行的起始。
	/// </summary>
	/// <value>如果正则表达式定位到行的起始，则为 <c>true</c>；否则为 <c>false</c>。</value>
	public new bool BeginningOfLine { get; set; }

	/// <summary>
	/// 获取或设置要向前看的正则表达式。
	/// </summary>
	/// <value>要向前看的正则表达式。</value>
	public LexRegex? TrailingExpression { get; set; }

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Equals(LexRegex? other)
	{
		if (other is AnchorExp exp)
		{
			return innerExp == exp.innerExp && BeginningOfLine == exp.BeginningOfLine &&
				TrailingExpression == exp.TrailingExpression;
		}
		return false;
	}

	/// <summary>
	/// 返回当前对象的哈希值。
	/// </summary>
	/// <returns>当前对象的哈希值。</returns>
	public override int GetHashCode()
	{
		return HashCode.Combine(innerExp, BeginningOfLine, TrailingExpression);
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <param name="builder">字符串构造器。</param>
	internal override void ToString(StringBuilder builder)
	{
		if (BeginningOfLine)
		{
			builder.Append('^');
		}
		innerExp.ToString(builder);
		if (TrailingExpression != null)
		{
			if (TrailingExpression == EndOfLine)
			{
				builder.Append('$');
			}
			else
			{
				builder.Append('/');
				TrailingExpression.ToString(builder);
			}
		}
	}
}
