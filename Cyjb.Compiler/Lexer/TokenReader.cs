using System;
using System.Collections.Generic;
using System.Globalization;
using Cyjb.IO;
using Cyjb.Text;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示词法单元读取器的基类。
	/// </summary>
	public abstract class TokenReader
	{
		/// <summary>
		/// 词法分析器的规则。
		/// </summary>
		private readonly LexerRule lexerRule;
		/// <summary>
		/// 当前词法分析器的控制器。
		/// </summary>
		private readonly ReaderController controller;
		/// <summary>
		/// 上下文的堆栈。
		/// </summary>
		private readonly Stack<LexerContext> contextStack = new Stack<LexerContext>();
		/// <summary>
		/// 要读取的下一个词法单元。
		/// </summary>
		private Token nextToken;
		/// <summary>
		/// 是否已读取下一个词法单元。
		/// </summary>
		private bool peekToken = false;
		/// <summary>
		/// 上一个词法单元匹配的文本。
		/// </summary>
		private string oldText;
		/// <summary>
		/// 使用给定的词法分析器信息初始化 <see cref="TokenReader"/> 类的新实例。
		/// </summary>
		/// <param name="lexerRule">要使用的词法分析器的规则。</param>
		/// <param name="rejectable">当前词法分析器是否允许 Reject 动作。</param>
		/// <param name="reader">要使用的源文件读取器。</param>
		protected TokenReader(LexerRule lexerRule, bool rejectable, SourceReader reader)
		{
			this.lexerRule = lexerRule;
			controller = new ReaderController(this, rejectable);
			this.Source = reader;
			this.BeginContext(this.lexerRule.Contexts[0]);
		}
		/// <summary>
		/// 获取词法分析器的规则。
		/// </summary>
		protected LexerRule LexerRule { get { return this.lexerRule; } }
		/// <summary>
		/// 获取当前词法单元的起始位置。
		/// </summary>
		protected SourceLocation Start { get; private set; }
		/// <summary>
		/// 获取或设置是否接受了当前的词法单元。
		/// </summary>
		internal bool IsAccept { get; set; }
		/// <summary>
		/// 获取或设置是否拒绝了当前的词法单元。
		/// </summary>
		internal bool IsReject { get; set; }
		/// <summary>
		/// 获取或设置下次匹配成功时，是否不替换当前的文本，而是把新的匹配追加在后面。
		/// </summary>
		internal bool IsMore { get; set; }
		/// <summary>
		/// 获取要扫描的源文件。
		/// </summary>
		public SourceReader Source { get; private set; }
		/// <summary>
		/// 获取当前的上下文信息。
		/// </summary>
		public LexerContext Context { get; private set; }
		/// <summary>
		/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
		/// </summary>
		/// <returns>输入流中的下一个词法单元。</returns>
		public Token ReadToken()
		{
			if (this.peekToken)
			{
				this.peekToken = false;
				return this.nextToken;
			}
			else
			{
				return InternalReadToken();
			}
		}
		/// <summary>
		/// 读取输入流中的下一个词法单元，但是并不更改读取器的状态。
		/// </summary>
		/// <returns>输入流中的下一个词法单元。</returns>
		public Token PeekToken()
		{
			if (!this.peekToken)
			{
				this.peekToken = true;
				this.nextToken = InternalReadToken();
			}
			return this.nextToken;
		}
		/// <summary>
		/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
		/// </summary>
		/// <returns>输入流中的下一个词法单元。</returns>
		private Token InternalReadToken()
		{
			while (true)
			{
				if (this.Source.Peek() == -1)
				{
					// 到达了流的结尾。
					Action<ReaderController> action = this.LexerRule.EofActions[Context.Index];
					if (action != null)
					{
						this.DoAction(action, Token.EndOfFileIndex, string.Empty);
						if (this.IsAccept)
						{
							return new Token(this.controller.Index, this.controller.Text,
								Source.StartLocation, SourceLocation.Invalid, this.controller.Value);
						}
					}
					return Token.GetEndOfFile(Source.StartLocation);
				}
				// 起始状态与当前上下文相关。
				int state = this.Context.Index * 2;
				if (Source.StartLocation.Col == 1)
				{
					// 行首规则。
					state++;
				}
				if (!this.IsMore)
				{
					this.Start = Source.StartLocation;
				}
				if (InternalReadToken(state))
				{
					if (this.IsMore)
					{
						oldText = this.controller.Text;
					}
					else
					{
						oldText = null;
					}
					if (this.IsAccept)
					{
						return new Token(this.controller.Index, this.controller.Text,
							this.Start, this.Source.BeforeStartLocation, this.controller.Value);
					}
				}
				else
				{
					// 到达死状态。
					string text = this.Source.Accept();
					if (text.Length == 0)
					{
						// 如果没有匹配任何字符，强制读入一个字符，可以防止死循环出现。
						this.Source.Read();
						text = this.Source.Accept();
					}
					throw CompilerExceptionHelper.UnrecognizedToken(text,
						this.Start, this.Source.BeforeStartLocation);
				}
			}
		}
		/// <summary>
		/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
		/// </summary>
		/// <param name="startState">DFA 的起始状态。</param>
		/// <returns>词法单元读入是否成功。</returns>
		protected abstract bool InternalReadToken(int startState);
		/// <summary>
		/// 执行指定的动作。
		/// </summary>
		/// <param name="action">要执行的动作。</param>
		/// <param name="index">词法单元的符号索引。</param>
		/// <param name="text">词法单元的文本。</param>
		protected void DoAction(Action<ReaderController> action, int index, string text)
		{
			this.IsAccept = this.IsReject = this.IsMore = false;
			this.controller.Index = index;
			if (oldText == null)
			{
				this.controller.Text = text;
			}
			else
			{
				this.controller.Text = oldText + text;
			}
			this.controller.Value = null;
			action(controller);
		}
		/// <summary>
		/// 转移状态。
		/// </summary>
		/// <param name="state">当前的状态。</param>
		/// <param name="ch">当前读入的字符。</param>
		/// <returns>转以后的状态。</returns>
		protected int TransitionState(int state, int ch)
		{
			if (ch == -1)
			{
				// End Of File。
				return LexerRule.DeadState;
			}
			return this.LexerRule.Transitions[state, this.LexerRule.CharClass[ch]];
		}

		#region 上下文切换

		/// <summary>
		/// 将指定上下文设置为当前的上下文。
		/// </summary>
		/// <param name="context">要设置的上下文。</param>
		public void BeginContext(LexerContext context)
		{
			this.Context = this.GetContext(context);
		}
		/// <summary>
		/// 将指定索引的上下文设置为当前的上下文。
		/// </summary>
		/// <param name="index">要设置的上下文的索引。</param>
		public void BeginContext(int index)
		{
			this.Context = this.GetContext(index);
		}
		/// <summary>
		/// 将指定标签的上下文设置为当前的上下文。
		/// </summary>
		/// <param name="label">要设置的上下文的标签。</param>
		public void BeginContext(string label)
		{
			this.Context = this.GetContext(label);
		}
		/// <summary>
		/// 将当前上下文压入堆栈，并将上下文设置为指定的值。
		/// </summary>
		/// <param name="context">要设置的上下文。</param>
		public void PushContext(LexerContext context)
		{
			this.contextStack.Push(this.Context);
			this.Context = this.GetContext(context);
		}
		/// <summary>
		/// 将当前上下文压入堆栈，并将上下文设置为指定的值。
		/// </summary>
		/// <param name="index">要设置的上下文的索引。</param>
		public void PushContext(int index)
		{
			this.contextStack.Push(this.Context);
			this.Context = this.GetContext(index);
		}
		/// <summary>
		/// 将当前上下文压入堆栈，并将上下文设置为指定的值。
		/// </summary>
		/// <param name="label">要设置的上下文的标签。</param>
		public void PushContext(string label)
		{
			this.contextStack.Push(this.Context);
			this.Context = this.GetContext(label);
		}
		/// <summary>
		/// 弹出堆栈顶的上下文，并把它设置为当前的上下文。
		/// </summary>
		public void PopContext()
		{
			if (this.contextStack.Count > 0)
			{
				this.Context = this.contextStack.Pop();
			}
			else
			{
				this.Context = this.lexerRule.Contexts[0];
			}
		}
		/// <summary>
		/// 返回指定的词法分析器上下文。
		/// </summary>
		/// <param name="context">要获取的词法分析器上下文。</param>
		/// <returns>有效的词法分析器上下文。</returns>
		private LexerContext GetContext(LexerContext context)
		{
			if (this.lexerRule.Contexts.Contains(context))
			{
				return context;
			}
			else
			{
				throw CompilerExceptionHelper.InvalidLexerContext("context", context.ToString());
			}
		}
		/// <summary>
		/// 返回指定标签的词法分析器上下文。
		/// </summary>
		/// <param name="label">要获取的词法分析器上下文的标签。</param>
		/// <returns>有效的词法分析器上下文。</returns>
		private LexerContext GetContext(string label)
		{
			LexerContext context;
			if (this.lexerRule.Contexts.TryGetItem(label, out context))
			{
				return context;
			}
			else
			{
				throw CompilerExceptionHelper.InvalidLexerContext("label", label);
			}
		}
		/// <summary>
		/// 返回指定索引的词法分析器上下文。
		/// </summary>
		/// <param name="index">要获取的词法分析器上下文的索引。</param>
		/// <returns>有效的词法分析器上下文。</returns>
		private LexerContext GetContext(int index)
		{
			if (index >= 0 && index < this.lexerRule.Contexts.Count)
			{
				return this.lexerRule.Contexts[index];
			}
			else
			{
				throw CompilerExceptionHelper.InvalidLexerContext("index",
					index.ToString(CultureInfo.InvariantCulture));
			}
		}

		#endregion // 上下文切换

	}
}
