using System.Diagnostics.CodeAnalysis;
using Cyjb.Collections.ObjectModel;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示 LR 语法分析器的动作集合。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal class ActionCollection<T> : ReadOnlyDictionaryBase<Symbol<T>, Action>
	where T : struct
{
	/// <summary>
	/// 动作字典。
	/// </summary>
	private readonly Dictionary<Symbol<T>, Action> actions = new();
	/// <summary>
	/// 默认动作。
	/// </summary>
	private UniqueValue<Action> defaultAction = new();

	/// <summary>
	/// 添加指定终结符的动作。
	/// </summary>
	/// <param name="symbol">动作关联到的终结符。</param>
	/// <param name="action">分析器动作。</param>
	public void Add(Symbol<T> symbol, Action action)
	{
		actions[symbol] = action;
		if (action is ReduceAction<T>)
		{
			defaultAction.Value = action;
		}
	}

	/// <summary>
	/// 返回当前动作集合的字典。
	/// </summary>
	/// <returns>当前动作集合的字典。</returns>
	public Dictionary<T, ParserAction> GetActions()
	{
		// 默认动作可以不添加到集合中。
		return new Dictionary<T, ParserAction>(actions
			.Where(pair => pair.Value != defaultAction.Value)
			.Select(pair => new KeyValuePair<T, ParserAction>(pair.Key.Kind, pair.Value.ToParserAction())));
	}

	/// <summary>
	/// 返回当前动作预期词法单元类型集合。
	/// </summary>
	/// <returns>当前动作预期词法单元类型集合。</returns>
	public HashSet<T> GetExpecting()
	{
		return new HashSet<T>(actions.Select(pair => pair.Key.Kind));
	}

	/// <summary>
	/// 返回当前状态的默认动作。
	/// </summary>
	/// <remarks>如果只包含一种规约动作，则可以将此动作作为默认动作。</remarks>
	public ParserAction GetDefaultAction()
	{
		if (defaultAction.IsUnique)
		{
			return defaultAction.Value.ToParserAction();
		}
		else
		{
			return ParserAction.Error;
		}
	}

	#region ReadOnlyDictionaryBase<Symbol<T>, ParseAction> 成员

	/// <summary>
	/// 获取当前字典包含的元素数。
	/// </summary>
	/// <value>当前字典中包含的元素数。</value>
	public override int Count => actions.Count;

	/// <summary>
	/// 获取包含当前字典的键的 <see cref="ICollection{Symbol}"/>。
	/// </summary>
	public override ICollection<Symbol<T>> Keys => actions.Keys;

	/// <summary>
	/// 获取包含当前字典的值的 <see cref="ICollection{ParseAction}"/>。
	/// </summary>
	public override ICollection<Action> Values => actions.Values;

	/// <summary>
	/// 获取具有指定键的元素。
	/// </summary>
	/// <param name="key">要获取的元素的键。</param>
	/// <returns>指定键对应的值。</returns>
	protected override Action GetItem(Symbol<T> key)
	{
		return actions[key];
	}

	/// <summary>
	/// 确定当前字典是否包含带有指定键的元素。
	/// </summary>
	/// <param name="key">要在当前字典中定位的键。</param>
	/// <returns>如果当前字典包含包含具有键的元素，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="key"/> 为 <c>null</c>。</exception>
	public override bool ContainsKey(Symbol<T> key)
	{
		return actions.ContainsKey(key);
	}

	/// <summary>
	/// 获取与指定键关联的值。
	/// </summary>
	/// <param name="key">要获取其值的键。</param>
	/// <param name="value">如果找到指定键，则返回与该键相关联的值；
	/// 否则返回 <paramref name="value"/> 参数的类型的默认值。</param>
	/// <returns>如果当前字典包含具有指定键的元素，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	/// <exception cref="ArgumentNullException"><paramref name="key"/> 为 <c>null</c>。</exception>
	public override bool TryGetValue(Symbol<T> key, [MaybeNullWhen(false)] out Action value)
	{
		return actions.TryGetValue(key, out value);
	}

	/// <summary>
	/// 返回一个循环访问字典的枚举器。
	/// </summary>
	/// <returns>可用于循环访问字典的 <see cref="IEnumerator{T}"/> 对象。</returns>
	public override IEnumerator<KeyValuePair<Symbol<T>, Action>> GetEnumerator()
	{
		return actions.GetEnumerator();
	}

	#endregion // ReadOnlyDictionaryBase<Symbol<T>, ParseAction> 成员

}
