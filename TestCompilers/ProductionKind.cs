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
	LBrace,
	/// <summary>
	/// 右小括号。
	/// </summary>
	RBrace,
	/// <summary>
	/// 星号
	/// </summary>
	Star,
	/// <summary>
	/// 加号
	/// </summary>
	Plus,
	/// <summary>
	/// 问号
	/// </summary>
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
