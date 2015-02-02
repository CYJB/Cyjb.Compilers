using Cyjb.Compilers.Lexers;
using Cyjb.IO;

namespace Cyjb.Compilers
{
	/// <summary>
	/// 表示词法单元读取器的控制器。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	public sealed class ReaderController<T>
		where T : struct
	{
		/// <summary>
		/// 当前的词法单元读取器。
		/// </summary>
		private TokenReaderBase<T> reader;
		/// <summary>
		/// 是否允许 Reject 动作。
		/// </summary>
		private bool rejectable;
		/// <summary>
		/// 使用当前的词法单元信息初始化 <see cref="ReaderController&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="reader">词法单元的读取器。</param>
		/// <param name="rejectable">是否允许 Reject 动作。</param>
		internal ReaderController(TokenReaderBase<T> reader, bool rejectable)
		{
			this.reader = reader;
			this.rejectable = rejectable;
		}
		/// <summary>
		/// 获取当前的上下文标签。
		/// </summary>
		/// <value>当前的上下文标签。</value>
		public string Context { get { return reader.Context; } }
		/// <summary>
		/// 获取要扫描的源文件。
		/// </summary>
		/// <value>要扫描的源文件。</value>
		public SourceReader Source { get { return reader.Source; } }
		/// <summary>
		/// 获取词法单元的标识符。
		/// </summary>
		/// <value>词法单元的标识符。</value>
		public T Id { get; internal set; }
		/// <summary>
		/// 获取词法单元的文本。
		/// </summary>
		/// <value>词法单元的文本。</value>
		public string Text { get; internal set; }
		/// <summary>
		/// 获取词法单元的值。
		/// </summary>
		/// <value>词法单元的值。</value>
		public object Value { get; internal set; }
		/// <summary>
		/// 拒绝当前的匹配，并尝试寻找下一个匹配。如果找不到下一个匹配，则会返回错误。
		/// </summary>
		public void Reject()
		{
			if (!this.rejectable)
			{
				throw CompilerCommonExceptions.NotRejectable();
			}
			if (this.reader.IsAccept)
			{
				throw CommonExceptions.ConflictingRejectAction();
			}
			this.reader.IsReject = true;
		}
		/// <summary>
		/// 在下次匹配成功时，不替换当前的文本，而是把新的匹配追加在后面。
		/// </summary>
		public void More()
		{
			this.reader.IsMore = true;
		}

		#region Accept

		/// <summary>
		/// 接受当前的匹配。
		/// </summary>
		/// <overloads>
		/// <summary>
		/// 接受当前的匹配。
		/// </summary>
		/// </overloads>
		public void Accept()
		{
			if (this.reader.IsReject)
			{
				throw CommonExceptions.ConflictingAcceptAction();
			}
			this.reader.IsAccept = true;
		}
		/// <summary>
		/// 接受当前的匹配，使用指定的标识符。
		/// </summary>
		/// <param name="id">匹配的标识符。</param>
		public void Accept(T id)
		{
			if (this.reader.IsReject)
			{
				throw CommonExceptions.ConflictingAcceptAction();
			}
			this.reader.IsAccept = true;
			this.Id = id;
		}
		/// <summary>
		/// 接受当前的匹配，使用指定的用户数据。
		/// </summary>
		/// <param name="value">用户数据。</param>
		public void Accept(object value)
		{
			if (this.reader.IsReject)
			{
				throw CommonExceptions.ConflictingAcceptAction();
			}
			this.reader.IsAccept = true;
			this.Value = value;
		}
		/// <summary>
		/// 接受当前的匹配，使用指定的标识符和用户数据。
		/// </summary>
		/// <param name="id">匹配的标识符。</param>
		/// <param name="value">用户数据。</param>
		public void Accept(T id, object value)
		{
			if (this.reader.IsReject)
			{
				throw CommonExceptions.ConflictingAcceptAction();
			}
			this.reader.IsAccept = true;
			this.Id = Id;
			this.Value = value;
		}
		/// <summary>
		/// 接受当前的匹配，使用指定的文本和用户数据。
		/// </summary>
		/// <param name="text">匹配的文本。</param>
		/// <param name="value">用户数据。</param>
		public void Accept(string text, object value)
		{
			if (this.reader.IsReject)
			{
				throw CommonExceptions.ConflictingAcceptAction();
			}
			this.reader.IsAccept = true;
			this.Text = text;
			this.Value = value;
		}
		/// <summary>
		/// 接受当前的匹配，使用指定的标识符、文本和用户数据。
		/// </summary>
		/// <param name="id">匹配的标识符。</param>
		/// <param name="text">匹配的文本。</param>
		/// <param name="value">用户数据。</param>
		public void Accept(T id, string text, object value)
		{
			if (this.reader.IsReject)
			{
				throw CommonExceptions.ConflictingAcceptAction();
			}
			this.reader.IsAccept = true;
			this.Id = Id;
			this.Text = text;
			this.Value = value;
		}

		#endregion

		#region 上下文切换

		/// <summary>
		/// 将指定标签的上下文设置为当前的上下文。
		/// </summary>
		/// <param name="label">要设置的上下文的标签。</param>
		public void BeginContext(string label)
		{
			this.reader.BeginContext(label);
		}
		/// <summary>
		/// 将当前上下文压入堆栈，并将上下文设置为指定的值。
		/// </summary>
		/// <param name="label">要设置的上下文的标签。</param>
		public void PushContext(string label)
		{
			this.reader.PushContext(label);
		}
		/// <summary>
		/// 弹出堆栈顶的上下文，并把它设置为当前的上下文。
		/// </summary>
		public void PopContext()
		{
			this.reader.PopContext();
		}

		#endregion // 上下文切换

	}
}
