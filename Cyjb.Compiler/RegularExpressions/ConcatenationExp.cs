using System.Diagnostics;
using System.Text;
using Cyjb.Compiler.Lexers;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示连接的正则表达式。
	/// </summary>
	public sealed class ConcatenationExp : Regex
	{
		/// <summary>
		/// 连接的第一个正则表达式。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Regex left;
		/// <summary>
		/// 连接的第二个正则表达式。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Regex right;
		/// <summary>
		/// 当前正则表达式的长度。
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int length = -2;
		/// <summary>
		/// 使用要连接的正则表达式初始化 <see cref="ConcatenationExp"/> 类的新实例。
		/// </summary>
		/// <param name="left">要连接的第一个正则表达式。</param>
		/// <param name="right">要连接的第二个正则表达式。</param>
		internal ConcatenationExp(Regex left, Regex right)
		{
			ExceptionHelper.CheckArgumentNull(left, "left");
			ExceptionHelper.CheckArgumentNull(right, "right");
			CheckRegex(left);
			CheckRegex(right);
			this.left = left;
			this.right = right;
		}
		/// <summary>
		/// 获取连接的第一个正则表达式。
		/// </summary>
		public Regex Left
		{
			get { return left; }
		}
		/// <summary>
		/// 获取连接的第二个正则表达式。
		/// </summary>
		public Regex Right
		{
			get { return right; }
		}
		/// <summary>
		/// 根据当前的正则表达式构造 NFA。
		/// </summary>
		/// <param name="nfa">要构造的 NFA。</param>
		internal override void BuildNfa(Nfa nfa)
		{
			left.BuildNfa(nfa);
			NfaState head = nfa.HeadState;
			NfaState tail = nfa.TailState;
			right.BuildNfa(nfa);
			tail.Add(nfa.HeadState);
			nfa.HeadState = head;
		}
		/// <summary>
		/// 获取当前正则表达式匹配的字符长度。变长度则为 <c>-1</c>。
		/// </summary>
		public override int Length
		{
			get
			{
				if (length == -2)
				{
					// -2 表示未初始化。
					if (left.Length != -1 && right.Length != -1)
					{
						length = left.Length + right.Length;
					}
					else
					{
						length = -1;
					}
				}
				return length;
			}
		}
		/// <summary>
		/// 返回当前对象的字符串表示形式。
		/// </summary>
		/// <returns>当前对象的字符串表示形式。</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			if (left is AlternationExp)
			{
				builder.Append("(");
				builder.Append(left.ToString());
				builder.Append(")");
			}
			else
			{
				builder.Append(left.ToString());
			}
			if (right is AlternationExp)
			{
				builder.Append("(");
				builder.Append(right.ToString());
				builder.Append(")");
			}
			else
			{
				builder.Append(right.ToString());
			}
			return builder.ToString();
		}
	}
}
