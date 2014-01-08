using System.Diagnostics;
using System.Text;
using Cyjb.Compilers.Lexers;

namespace Cyjb.Compilers.RegularExpressions
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
		private int length = NotInitialized;
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
		/// <value>连接的第一个正则表达式。</value>
		public Regex Left
		{
			get { return left; }
		}
		/// <summary>
		/// 获取连接的第二个正则表达式。
		/// </summary>
		/// <value>连接的第二个正则表达式。</value>
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
		/// 获取当前正则表达式匹配的字符串长度。
		/// </summary>
		/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>-1</c>。</value>
		public override int Length
		{
			get
			{
				if (length == NotInitialized)
				{
					if (left.Length != VariableLength && right.Length != VariableLength)
					{
						length = left.Length + right.Length;
					}
					else
					{
						length = VariableLength;
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
