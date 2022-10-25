using Cyjb.Text;

namespace TestCompilers;

/// <summary>
/// 产生式的类型。
/// </summary>
public enum ProductionKind
{
	/// <summary>
	/// 词法单元类型。
	/// </summary>
	Id,
	/// <summary>
	/// 左小括号。
	/// </summary>
	[TokenDisplayName("(")]
	LBrace,
	/// <summary>
	/// 右小括号。
	/// </summary>
	[TokenDisplayName(")")]
	RBrace,
	/// <summary>
	/// 星号。
	/// </summary>
	[TokenDisplayName("*")]
	Star,
	/// <summary>
	/// 加号。
	/// </summary>
	[TokenDisplayName("+")]
	Plus,
	/// <summary>
	/// 问号。
	/// </summary>
	[TokenDisplayName("?")]
	Question,
	/// <summary>
	/// 或符号。
	/// </summary>
	[TokenDisplayName("|")]
	Or,
	/// <summary>
	/// 或产生式。
	/// </summary>
	AltExp,
	/// <summary>
	/// 产生式。
	/// </summary>
	Exp,
	/// <summary>
	/// 重复产生式项。
	/// </summary>
	Repeat,
	/// <summary>
	/// 产生式项。
	/// </summary>
	Item,
}
