using Cyjb.Text;

namespace Cyjb.Compilers.Parsers.Production;

/// <summary>
/// 产生式的词法单元类型。
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
	/// 星号
	/// </summary>
	[TokenDisplayName("*")]
	Star,
	/// <summary>
	/// 加号
	/// </summary>
	[TokenDisplayName("+")]
	Plus,
	/// <summary>
	/// 问号
	/// </summary>
	[TokenDisplayName("?")]
	Question,
	/// <summary>
	/// 产生式。
	/// </summary>
	Expression,
	/// <summary>
	/// 重复产生式项。
	/// </summary>
	Repeat,
	/// <summary>
	/// 产生式项。
	/// </summary>
	Item,
}
