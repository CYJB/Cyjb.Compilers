using System;
using System.Collections.Generic;
using Cyjb.IO;
using Cyjb.Text;

namespace Cyjb.Compiler.Lexer
{
	/// <summary>
	/// 表示词法单元读取器的基类。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，必须是一个枚举类型。</typeparam>
	internal abstract class TokenReaderBase<T> : TokenReader<T>
		where T : struct
	{
		/// <summary>
		/// 表示文件结束的终结符索引。
		/// </summary>
		protected const int EndOfFileIndex = -1;
		/// <summary>
		/// 词法分析器的规则。
		/// </summary>
		private readonly LexerRule<T> lexerRule;
		/// <summary>
		/// 当前词法分析器的控制器。
		/// </summary>
		private readonly ReaderController<T> controller;
		/// <summary>
		/// 上下文的堆栈。
		/// </summary>
		private readonly Stack<KeyValuePair<string, int>> contextStack = new Stack<KeyValuePair<string, int>>();
		/// <summary>
		/// 当前的上下文。
		/// </summary>
		private KeyValuePair<string, int> context;
		/// <summary>
		/// 之前匹配的文本。
		/// </summary>
		private string oldText;
		/// <summary>
		/// 使用给定的词法分析器信息初始化 <see cref="TokenReaderBase&lt;T&gt;"/> 类的新实例。
		/// </summary>
		/// <param name="lexerRule">要使用的词法分析器的规则。</param>
		/// <param name="rejectable">当前词法分析器是否允许 Reject 动作。</param>
		/// <param name="reader">要使用的源文件读取器。</param>
		protected TokenReaderBase(LexerRule<T> lexerRule, bool rejectable, SourceReader reader)
			: base(reader)
		{
			this.lexerRule = lexerRule;
			controller = new ReaderController<T>(this, rejectable);
			this.BeginContext(Constants.InitialContext);
		}
		/// <summary>
		/// 获取词法分析器的规则。
		/// </summary>
		protected LexerRule<T> LexerRule { get { return this.lexerRule; } }
		/// <summary>
		/// 获取当前的上下文标签。
		/// </summary>
		/// <value>当前的上下文标签。</value>
		internal string Context { get { return this.context.Key; } }
		/// <summary>
		/// 获取当前词法单元的起始位置。
		/// </summary>
		/// <value>当前词法单元的起始位置。</value>
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
		/// 读取输入流中的下一个词法单元并提升输入流的字符位置。
		/// </summary>
		/// <returns>输入流中的下一个词法单元。</returns>
		protected override Token<T> InternalReadToken()
		{
			while (true)
			{
				if (this.Source.Peek() == -1)
				{
					// 到达了流的结尾。
					Action<ReaderController<T>> action = this.LexerRule.EofActions[context.Value];
					if (action != null)
					{
						this.DoAction(action, EndOfFileIndex);
						if (this.IsAccept)
						{
							return new Token<T>(this.controller.Id, this.controller.Text,
								Source.StartLocation, SourceLocation.Invalid, this.controller.Value);
						}
					}
					return Token<T>.GetEndOfFile(Source.StartLocation);
				}
				// 起始状态与当前上下文相关。
				int state = this.context.Value * 2;
				if (this.Source.StartLocation.Col == 1)
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
					oldText = this.IsMore ? this.controller.Text : null;
					if (!this.IsMore && !this.IsReject)
					{
						this.Source.Drop();
					}
					if (this.IsAccept)
					{
						return new Token<T>(this.controller.Id, this.controller.Text,
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
		protected void DoAction(Action<ReaderController<T>> action, int index)
		{
			this.IsAccept = this.IsReject = this.IsMore = false;
			this.controller.Id = index == EndOfFileIndex ? Token<T>.EndOfFile : lexerRule.Symbols[index].Id;
			this.controller.Text = string.Concat(this.oldText, this.Source.ReadedBlock());
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
				return EndOfFileIndex;
			}
			return this.LexerRule.States[state].Transitions[this.LexerRule.CharClass[ch]];
		}

		#region 上下文切换

		/// <summary>
		/// 将指定标签的上下文设置为当前的上下文。
		/// </summary>
		/// <param name="label">要设置的上下文的标签。</param>
		internal void BeginContext(string label)
		{
			this.context = this.GetContext(label);
		}
		/// <summary>
		/// 将当前上下文压入堆栈，并将上下文设置为指定的值。
		/// </summary>
		/// <param name="label">要设置的上下文的标签。</param>
		internal void PushContext(string label)
		{
			this.contextStack.Push(this.context);
			this.context = this.GetContext(label);
		}
		/// <summary>
		/// 弹出堆栈顶的上下文，并把它设置为当前的上下文。
		/// </summary>
		internal void PopContext()
		{
			if (this.contextStack.Count > 0)
			{
				this.context = this.contextStack.Pop();
			}
			else
			{
				this.context = new KeyValuePair<string, int>(Constants.InitialContext, 0);
			}
		}
		/// <summary>
		/// 返回指定标签的词法分析器上下文。
		/// </summary>
		/// <param name="label">要获取的词法分析器上下文的标签。</param>
		/// <returns>有效的词法分析器上下文。</returns>
		private KeyValuePair<string, int> GetContext(string label)
		{
			int index;
			if (this.lexerRule.Contexts.TryGetValue(label, out index))
			{
				return new KeyValuePair<string, int>(label, index);
			}
			else
			{
				throw CompilerExceptionHelper.InvalidLexerContext("label", label);
			}
		}

		#endregion // 上下文切换

	}
}
