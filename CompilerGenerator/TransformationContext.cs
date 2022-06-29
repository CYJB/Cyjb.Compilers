using Microsoft.CodeAnalysis;

namespace Cyjb.Compilers;

/// <summary>
/// 表示转换的上下文。
/// </summary>
internal class TransformationContext
{
	/// <summary>
	/// 获取是否包含错误。
	/// </summary>
	public bool HasError { get; private set; }

	/// <summary>
	/// 添加指定的错误信息。
	/// </summary>
	/// <param name="message">错误信息。</param>
	/// <param name="node">语法节点。</param>
	public void AddError(string message, SyntaxNode node)
	{
		AddError(message, node.GetLocation());
	}

	/// <summary>
	/// 添加指定的错误信息。
	/// </summary>
	/// <param name="message">错误信息。</param>
	/// <param name="location">错误信息的位置。</param>
	public void AddError(string message, Location? location)
	{
		HasError = true;
		string position = string.Empty;
		if (location != null)
		{
			FileLinePositionSpan span = location.GetLineSpan();
			string file = Path.GetFileName(span.Path);
			int line = span.StartLinePosition.Line + 1;
			int column = span.StartLinePosition.Character + 1;
			position = $"{file}({line},{column}): ";
		}
		Console.WriteLine(position + message);
	}
}
