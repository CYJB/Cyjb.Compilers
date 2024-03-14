using System.Globalization;
using System.Text;
using Cyjb.Compilers.Lexers;
using Cyjb.Text;

namespace TestCompilers.Lexers;

/// <summary>
/// 用于单元测试的转义字符串控制器。
/// </summary>
[LexerContext("str")]
[LexerContext("vstr")]
public partial class TestEscapeStrLexer : LexerController<Str>
{
	private const string CtxStr = "str";
	private const string CtxVstr = "vstr";

	/// <summary>
	/// 当前起始索引。
	/// </summary>
	private int curStart;
	/// <summary>
	/// 处理后的文本。
	/// </summary>
	private readonly StringBuilder decodedText = new();

	[LexerSymbol(@"\""")]
	public void BeginStrAction()
	{
		PushContext(CtxStr);
		curStart = Start;
		decodedText.Clear();
	}

	[LexerSymbol(@"@\""")]
	public void BeginVstrAction()
	{
		PushContext(CtxVstr);
		curStart = Start;
		decodedText.Clear();
	}

	[LexerSymbol(@"<str, vstr>\""", Kind = Str.Str)]
	public void EndAction()
	{
		PopContext();
		Start = curStart;
		Text = decodedText.ToString();
	}

	[LexerSymbol(@"<str>\\u[0-9]{4}")]
	[LexerSymbol(@"<str>\\x[0-9]{2}")]
	public void HexEscapeAction()
	{
		decodedText.Append((char)int.Parse(Text.AsSpan(2), NumberStyles.HexNumber));
	}

	[LexerSymbol(@"<str>\\n")]
	public void EscapeLFAction()
	{
		decodedText.Append('\n');
	}

	[LexerSymbol(@"<str>\\\""")]
	public void EscapeQuoteAction()
	{
		decodedText.Append('\"');
	}

	[LexerSymbol(@"<str>\\r")]
	public void EscapeCRAction()
	{
		decodedText.Append('\r');
	}

	[LexerSymbol(@"<*>.")]
	public void CopyAction()
	{
		StringBuilderUtil.Append(decodedText, Text);
	}

	[LexerSymbol(@"<vstr>\""\""")]
	public void VstrQuoteAction()
	{
		decodedText.Append('"');
	}
}

