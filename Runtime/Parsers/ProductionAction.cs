namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 提供产生式数据的预置动作。
/// </summary>
public sealed class ProductionAction
{
	/// <summary>
	/// 供 <see cref="SymbolOptions.Optional"/> 使用的动作。
	/// </summary>
	public static readonly Delegate Optional = () => "OptionalAction";
	/// <summary>
	/// 供 <see cref="SymbolOptions.ZeroOrMore"/> 和 <see cref="SymbolOptions.OneOrMore"/> 使用的动作。
	/// </summary>
	public static readonly Delegate More = () => "MoreAction";
}
