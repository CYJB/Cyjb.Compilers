namespace Cyjb.Compilers;

/// <summary>
/// 表示编译相关的异常。
/// </summary>
public sealed class CompileException : Exception
{
	/// <summary>
	/// 使用指定的错误消息初始化 <see cref="CompileException"/> 类的新实例。
	/// </summary>
	/// <param name="message">错误消息。</param>
	public CompileException(string message) : base(message) { }
}
