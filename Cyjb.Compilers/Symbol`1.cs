using System;
using Cyjb.Text;

namespace Cyjb.Compilers
{
	/// <summary>
	/// 表示词法或语法分析中的终结符或非终结符。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	public abstract class Symbol<T>
		where T : struct
	{
		/// <summary>
		/// 表示无效的终结符标识符。
		/// </summary>
		internal static readonly T Invalid = (T)Enum.ToObject(typeof(T), -3);
		/// <summary>
		/// 表示错误的终结符。
		/// </summary>
		internal static readonly Terminal<T> Error = new Terminal<T>(Token<T>.Error, -2, null, null);
		/// <summary>
		/// 使用符号的标识符初始化 <see cref="Symbol&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="id">当前符号的标识符。</param>
		/// <param name="index">当前符号的索引。</param>
		protected Symbol(T id, int index)
		{
			this.Id = id;
			this.Index = index;
		}
		/// <summary>
		/// 获取当前符号的标识符。
		/// </summary>
		/// <value>当前符号的标识符。</value>
		public T Id { get; private set; }
		/// <summary>
		/// 获取或设置当前符号的索引。
		/// </summary>
		/// <value>当前符号的索引。</value>
		internal int Index { get; set; }
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			return this.Id.ToString();
		}
	}
}
