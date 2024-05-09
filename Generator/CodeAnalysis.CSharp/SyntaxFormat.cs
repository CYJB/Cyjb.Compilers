using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 语法的格式信息。
/// </summary>
internal sealed class SyntaxFormat
{
	/// <summary>
	/// 默认语法格式。
	/// </summary>
	public static readonly SyntaxFormat Default = new("", SyntaxFactory.EndOfLine("\t"), 0, new List<SyntaxTrivia>());

	/// <summary>
	/// 缩进字符。
	/// </summary>
	private readonly string indent;
	/// <summary>
	/// 换行符。
	/// </summary>
	private readonly SyntaxTrivia endOfLine;
	/// <summary>
	/// 当前深度。
	/// </summary>
	private readonly int depth;
	/// <summary>
	/// 不同深度的缩进列表。
	/// </summary>
	private readonly List<SyntaxTrivia> indentations = new();

	/// <summary>
	/// 使用指定的根节点初始化 <see cref="SyntaxFormat"/> 类的新实例。
	/// </summary>
	/// <param name="node">根节点。</param>
	public SyntaxFormat(SyntaxNode node)
	{
		SyntaxNode root = node.AncestorsAndSelf().Last();
		IndentDetector detection = new();
		detection.Visit(root);
		indent = detection.Indent;
		if (indent.Length == 0)
		{
			indent = "\t";
		}
		endOfLine = SyntaxFactory.EndOfLine(DetectEndOfLine(root.ToFullString()));
		depth = IndentDetector.GetIndent(node).Split(indent).Length - 1;
		indentations.Add(SyntaxFactory.Whitespace(string.Empty));
	}

	/// <summary>
	/// 使用指定的格式信息初始化 <see cref="SyntaxFormat"/> 类的新实例。
	/// </summary>
	/// <param name="indent">缩进字符。</param>
	/// <param name="endOfLine">换行符。</param>
	/// <param name="depth">当前深度。</param>
	/// <param name="indentations">不同深度的缩进列表。</param>
	private SyntaxFormat(string indent, SyntaxTrivia endOfLine, int depth, List<SyntaxTrivia> indentations)
	{
		this.indent = indent;
		this.endOfLine = endOfLine;
		this.depth = depth;
		this.indentations = indentations;
	}

	/// <summary>
	/// 获取当前深度。
	/// </summary>
	public int Depth => depth;
	/// <summary>
	/// 获取换行符。
	/// </summary>
	public SyntaxTrivia EndOfLine => endOfLine;
	/// <summary>
	/// 获取当前缩进。
	/// </summary>
	public SyntaxTrivia Indentation => GetIndentation(Depth);

	/// <summary>
	/// 返回深度增加 1 的格式信息。
	/// </summary>
	/// <returns>深度增加 1 的格式信息。</returns>
	public SyntaxFormat IncDepth()
	{
		return new SyntaxFormat(indent, endOfLine, depth + 1, indentations);
	}

	/// <summary>
	/// 返回深度减少 1 的格式信息。
	/// </summary>
	/// <returns>深度减少 1 的格式信息。</returns>
	public SyntaxFormat DecDepth()
	{
		return new SyntaxFormat(indent, endOfLine, depth - 1, indentations);
	}

	/// <summary>
	/// 返回设置了指定深度的格式信息。
	/// </summary>
	/// <param name="depth">深度信息。</param>
	/// <returns>设置了指定深度的格式信息。</returns>
	public SyntaxFormat WithDepth(int depth)
	{
		return new SyntaxFormat(indent, endOfLine, depth, indentations);
	}

	/// <summary>
	/// 检查指定文本的换行符。
	/// </summary>
	/// <param name="text">要检查的文本。</param>
	/// <returns>指定文本的换行符。</returns>
	private static string DetectEndOfLine(string text)
	{
		// 检查换行符。
		int index = text.IndexOfAny(new[] { '\r', '\n' });
		if (index >= 0)
		{
			if (text[index] == '\n')
			{
				return "\n";
			}
			else
			{
				return "\r\n";
			}
		}
		else
		{
			return Environment.NewLine;
		}
	}

	/// <summary>
	/// 返回指定深度的缩进。
	/// </summary>
	/// <param name="depth">所进的深度。</param>
	/// <returns>指定深度的缩进。</returns>
	public SyntaxTrivia GetIndentation(int depth)
	{
		indentations.EnsureCapacity(depth + 1);
		for (int i = indentations.Count; i <= depth; i++)
		{
			string text = indentations[i - 1].ToString() + indent;
			indentations.Add(SyntaxFactory.Whitespace(text));
		}
		return indentations[depth];
	}
}
