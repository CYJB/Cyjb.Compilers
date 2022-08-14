using Cyjb.Text;

namespace Cyjb.Compilers.Lexers
{
	/// <summary>
	/// 表示词法分析器的工厂。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
	/// <remarks>关于如何构造自己的词法分析器，可以参考我的博文
	/// <see href="http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html">
	/// 《C# 词法分析器（六）构造词法分析器》</see>。</remarks>
	/// <seealso href="http://www.cnblogs.com/cyjb/archive/p/LexerLexer.html">
	/// 《C# 词法分析器（六）构造词法分析器》</seealso>
	[Serializable]
	public sealed class LexerFactory<T> : ILexerFactory<T>
		where T : struct
	{
		/// <summary>
		/// 词法分析器的数据。
		/// </summary>
		private readonly LexerData<T> lexerData;

		/// <summary>
		/// 使用指定的词法分析器数据初始化 <see cref="LexerFactory{T,TController}"/> 类的新实例。
		/// </summary>
		/// <param name="lexerData">词法分析器的数据。</param>
		public LexerFactory(LexerData<T> lexerData)
		{
			this.lexerData = lexerData;
		}

		/// <summary>
		/// 创建分析指定源文件的词法分析器。
		/// </summary>
		/// <param name="source">要读取的源文件。</param>
		/// <returns>指定源文件的词法分析器。</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
		/// <overloads>
		/// <summary>
		/// 创建分析指定源文件的词法分析器。
		/// </summary>
		/// </overloads>
		public TokenReader<T> CreateReader(string source)
		{
			ArgumentNullException.ThrowIfNull(source);
			return CreateReader(new SourceReader(new StringReader(source)));
		}

		/// <summary>
		/// 创建分析指定源文件的词法分析器。
		/// </summary>
		/// <param name="source">要读取的源文件。</param>
		/// <returns>指定源文件的词法分析器。</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <c>null</c>。</exception>
		public TokenReader<T> CreateReader(SourceReader source)
		{
			ArgumentNullException.ThrowIfNull(source);
			LexerController<T> controller = new();
			controller.Init(source, lexerData.Contexts, (Delegate action) =>
			{
				((Action<LexerController<T>>)action)(controller);
			}, lexerData.Rejectable);
			if (lexerData.Rejectable)
			{
				if (lexerData.TrailingType == TrailingType.None)
				{
					return new RejectableReader<T>(lexerData, controller, source);
				}
			}
			else
			{
				if (lexerData.TrailingType == TrailingType.None)
				{
					return new SimpleReader<T>(lexerData, controller, source);
				}
				else if (lexerData.TrailingType == TrailingType.Fixed)
				{
					return new FixedTrailingReader<T>(lexerData, controller, source);
				}
			}
			return new RejectableTrailingReader<T>(lexerData, controller, source);
		}
	}
}
