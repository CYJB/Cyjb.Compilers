using System.Diagnostics;
using System.Text;

namespace Cyjb.Compilers.Parsers;

/// <summary>
/// 表示产生式的数据。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class ProductionData<T>
	where T : struct
{
	/// <summary>
	/// 产生式体的大小。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly int bodySize;

	/// <summary>
	/// 使用产生式的数据初始化 <see cref="ProductionData{T}"/> 类的新实例。
	/// </summary>
	/// <param name="head">产生式头。</param>
	/// <param name="action">产生式对应的动作。</param>
	/// <param name="body">产生式体。</param>
	public ProductionData(T head, Delegate? action, params T[] body)
	{
		Head = head;
		Action = action;
		Body = body;
		bodySize = body.Length;
	}

	/// <summary>
	/// 获取产生式头。
	/// </summary>
	/// <value>产生式头。</value>
	public T Head { get; }
	/// <summary>
	/// 获取产生式对应的动作。
	/// </summary>
	/// <value>产生式对应的动作。</value>
	public Delegate? Action { get; }
	/// <summary>
	/// 获取产生式体的大小。
	/// </summary>
	public int BodySize => bodySize;
	/// <summary>
	/// 获取产生式体。
	/// </summary>
	/// <value>产生式体。</value>
	public T[] Body { get; }

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <returns>当前对象的字符串表示形式。</returns>
	public override string ToString()
	{
		StringBuilder text = new();
		text.Append(Head);
		text.Append(" ->");
		if (bodySize == 0)
		{
			text.Append(" ε");
		}
		else
		{
			foreach (T item in Body)
			{
				text.Append(' ');
				text.Append(item);
			}
		}
		return text.ToString();
	}
}
