using Cyjb.Compilers.Lexers;

namespace TestCompilers.Lexers;

public enum Str { Str }

/// <summary>
/// 用于单元测试的字符串控制器。
/// </summary>
[LexerRegex("regular_char", @"[^""\\\n\r\u0085\u2028\u2029]|(\\.)")]
[LexerRegex("regular_literal", @"\""{regular_char}*\""")]
[LexerRegex("verbatim_char", @"[^""]|\""\""")]
[LexerRegex("verbatim_literal", @"@\""{verbatim_char}*\""")]
[LexerSymbol("{regular_literal}|{verbatim_literal}", Kind = Str.Str)]
public partial class TestStrLexer : LexerController<Str>
{
}

