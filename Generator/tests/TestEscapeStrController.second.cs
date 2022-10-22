using System;
using System.Globalization;
using System.Text;
using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

[LexerContext("vstr2")]

public partial class TestEscapeStrController
{
	[LexerSymbol(@"<vstr2>\""\""")]
	public void VstrQuoteAction2()
	{
		decodedText.Append('"');
	}
}

