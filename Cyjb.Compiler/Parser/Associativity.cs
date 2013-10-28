namespace Cyjb.Compiler.Parser
{
	/// <summary>
	/// 表示终结符的结合性。
	/// </summary>
	internal class Associativity
	{
		/// <summary>
		/// 使用指定结合性的优先级和类型初始化 <see cref="Associativity"/> 类的新实例。
		/// </summary>
		/// <param name="priority">终结符的优先级。</param>
		/// <param name="type">结合性的类型。</param>
		internal Associativity(int priority, AssociativeType type)
		{
			this.Priority = priority;
			this.AssociativeType = type;
		}
		/// <summary>
		/// 获取终结符的优先级。
		/// </summary>
		/// <value>当前终结符的优先级。</value>
		public int Priority { get; private set; }
		/// <summary>
		/// 获取结合性的类型。
		/// </summary>
		/// <value>结合性的类型。</value>
		public AssociativeType AssociativeType { get; private set; }
	}
}
