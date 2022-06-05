using Cyjb.Text;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 表示词法分析器的工厂。
/// </summary>
/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
public sealed class TokenReaderFactory<T>
	where T : struct
{
	/// <summary>
	/// 词法分析器的类型。
	/// </summary>
	private enum ReaderType
	{
		Simple,
		Rejectable,
		FixedTrailing,
		RejectableTrailing,
	}

	/// <summary>
	/// 词法分析器的数据。
	/// </summary>
	private readonly LexerData<T> lexerData;
	/// <summary>
	/// 词法分析器的环境初始化器。
	/// </summary>
	private readonly Func<object?>? envInitializer;
	/// <summary>
	/// 词法分析器的类型。
	/// </summary>
	private readonly ReaderType readerType;

	/// <summary>
	/// 使用指定的词法单元读信息初始化 <see cref="TokenReaderFactory{T}"/> 类的新实例。
	/// </summary>
	/// <param name="lexerData">词法分析器的数据。</param>
	/// <param name="envInitializer">词法分析器的环境初始化器。</param>
	/// <param name="trailingType">向前看符号的类型。</param>
	/// <param name="rejectable">是否允许 Reject 动作。</param>
	internal TokenReaderFactory(LexerData<T> lexerData, Func<object?>? envInitializer, TrailingType trailingType, bool rejectable)
	{
		this.lexerData = lexerData;
		this.envInitializer = envInitializer;
		switch (trailingType)
		{
			case TrailingType.None:
				readerType = rejectable ? ReaderType.Rejectable : ReaderType.Simple;
				break;
			case TrailingType.Fixed:
				readerType = rejectable ? ReaderType.RejectableTrailing : ReaderType.FixedTrailing;
				break;
			case TrailingType.Variable:
				readerType = ReaderType.RejectableTrailing;
				break;
		}
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
		object? env = null;
		if (envInitializer != null)
		{
			env = envInitializer();
		}
		return readerType switch
		{
			ReaderType.Simple => new SimpleReader<T>(lexerData, env, source),
			ReaderType.Rejectable => new RejectableReader<T>(lexerData, env, source),
			ReaderType.FixedTrailing => new FixedTrailingReader<T>(lexerData, env, source),
			ReaderType.RejectableTrailing => new RejectableTrailingReader<T>(lexerData, env, source),
			_ => throw new InvalidOperationException("Code supposed to be unreachable."),
		};
	}
}
