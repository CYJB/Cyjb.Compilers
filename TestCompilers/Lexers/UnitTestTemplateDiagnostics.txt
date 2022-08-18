[LexerSymbol(10, 20, 30, 40, 50)]
[LexerSymbol(Regex: "f")]
[LexerSymbol("F", regex : "f")]
[LexerSymbol(options: RegexOptions.None, "f")]
[LexerSymbol("f", Test = 10)]
[LexerSymbol("F", Kind = "f", Kind = "F2")]
[LexerSymbol]
[LexerSymbol(10)]
[LexerSymbol("T", RegexOptions.Test)]
[LexerSymbol("T", 10.3)]
[LexerSymbol("T", "abc")]
[LexerContext("")]
[LexerSymbol("<ffffT")]
public partial class TestController : LexerController<Test>
{
	[LexerSymbol("T")]
	public void InvalidAction(int a) { }
}

