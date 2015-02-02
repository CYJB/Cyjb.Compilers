using System;
using System.Diagnostics;
using System.Globalization;
using Cyjb.Reflection;

namespace Cyjb.Compilers
{
	/// <summary>
	/// 包含了各种常量定义，以及基本方法。
	/// </summary>
	internal static class Constants
	{
		/// <summary>
		/// 初始的词法分析器上下文。
		/// </summary>
		public const string InitialContext = "Initial";
		/// <summary>
		/// 增广文法的起始非终结符的标签。
		/// </summary>
		public const string AugmentedStartLabel = "$accept";
		/// <summary>
		/// DFA 的死状态。
		/// </summary>
		public const int DeadState = -1;
		/// <summary>
		/// 表示不存在的终结符的索引。
		/// </summary>
		public const int None = -1;
		/// <summary>
		/// 表示唯一归约的动作索引。
		/// </summary>
		public const int UniqueIdx = 0;
		/// <summary>
		/// 表示错误的动作索引。
		/// </summary>
		public const int ErrIdx = 1;
		/// <summary>
		/// 表示文件结束的动作索引。
		/// </summary>
		public const int EOFIdx = 2;
		/// <summary>
		/// 表示词法单元标识符的偏移。
		/// </summary>
		public const int TokenOffset = 3;
		/// <summary>
		/// 获取 <see cref="System.Int32"/> 类型的枚举值。
		/// </summary>
		/// <param name="value">要获取的枚举值。</param>
		/// <returns><see cref="System.Int32"/> 类型的枚举值。</returns>
		public static int ToInt32(object value)
		{
			if (value.GetType().IsUnsigned())
			{
				return unchecked((int)Convert.ChangeType<uint>(value));
			}
			return Convert.ChangeType<int>(value);
		}
	}
}
