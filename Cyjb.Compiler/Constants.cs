using System;
using System.Diagnostics;
using System.Globalization;

namespace Cyjb.Compiler
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
			switch (Convert.GetTypeCode(value))
			{
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
					return Convert.ToInt32(value, CultureInfo.InvariantCulture);
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return (int)Convert.ToUInt32(value, CultureInfo.InvariantCulture);
			}
			Debug.Fail("无效的枚举类型");
			return 0;
		}
	}
}
