using System.Text;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示文法的产生式。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
internal sealed class Production<T>
	where T : struct
{
	/// <summary>
	/// 使用产生式的索引和内容初始化 <see cref="Production{T}"/> 类的新实例。
	/// </summary>
	/// <param name="index">产生式的索引。</param>
	/// <param name="head">产生式头。</param>
	/// <param name="body">产生式体。</param>
	internal Production(int index, Symbol<T> head, params Symbol<T>[] body)
	{
		Index = index;
		Head = head;
		Body = body;
	}

	/// <summary>
	/// 获取产生式的索引。
	/// </summary>
	/// <value>产生式的索引。</value>
	internal int Index { get; }
	/// <summary>
	/// 获取产生式头。
	/// </summary>
	/// <value>产生式的头。</value>
	internal Symbol<T> Head { get; }
	/// <summary>
	/// 获取产生式体。
	/// </summary>
	/// <value>产生式体。</value>
	internal Symbol<T>[] Body { get; }
	/// <summary>
	/// 获取或设置产生式的动作。
	/// </summary>
	/// <value>产生式的动作。</value>
	internal Delegate? Action { get; set; }
	/// <summary>
	/// 获取或设置表示当前产生式的结合性的非终结符。
	/// </summary>
	/// <value>表示当前产生式的结合性的非终结符的标识符。</value>
	internal Symbol<T>? Precedence { get; set; }

	/// <summary>
	/// 返回当前产生式的数据。
	/// </summary>
	/// <returns>当前产生式的数据。</returns>
	public ProductionData<T> GetData()
	{
		return new ProductionData<T>(Head.Index, Head.Kind, Action, Body.Select(s => s.Kind).ToArray());
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		StringBuilder text = new();
		text.Append(Head.Name);
		text.Append(" ->");
		if (Body.Length == 0)
		{
			text.Append(" ε");
		}
		else
		{
			foreach (Symbol<T> item in Body)
			{
				text.Append(' ');
				text.Append(item);
			}
		}
		return text.ToString();
	}
}
