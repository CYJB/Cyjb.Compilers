using System.Text.RegularExpressions;
using Cyjb.Compilers.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.RegularExpressions;

/// <summary>
/// <see cref="RegexCharClass"/> 类的单元测试。
/// </summary>
[TestClass]
public class UnitTestLexRegex
{
	/// <summary>
	/// 对 <see cref="LexRegex.Parse"/> 方法进行测试。
	/// </summary>
	[DataTestMethod]
	[DataRow("abc", RegexOptions.None, "\"abc\"")]
	[DataRow("a|b|c", RegexOptions.None, "\"a\"|\"b\"|\"c\"")]
	[DataRow("(a|b)cd", RegexOptions.None, "(\"a\"|\"b\")\"cd\"")]
	[DataRow("^abc", RegexOptions.None, "^\"abc\"")]
	[DataRow("abc+", RegexOptions.None, "\"ab\"\"c\"+")]
	[DataRow("abc*d{3}e{4,}f{5,6}a", RegexOptions.None, "\"ab\"\"c\"*\"d\"{3}\"e\"{4,}\"f\"{5,6}\"a\"")]
	[DataRow("(a|b)+[cde]*f/ghi", RegexOptions.None, "(\"a\"|\"b\")+[c-e]*\"f\"/\"ghi\"")]
	[DataRow("abc((d|e)f(g|h(ij)))k", RegexOptions.None, "\"abc\"(\"d\"|\"e\")\"f\"(\"g\"|\"h\"\"ij\")\"k\"")]
	[DataRow("abc", RegexOptions.IgnoreCase, "(?i:abc)")]
	[DataRow("abc(?i:def)ghi", RegexOptions.None, "\"abc\"(?i:def)\"ghi\"")]
	[DataRow("abc(?i:de(?-i:f))ghi", RegexOptions.None, "\"abc\"(?i:de)\"f\"\"ghi\"")]
	[DataRow("a.+b", RegexOptions.None, "\"a\"[^\\n\\r]+\"b\"")]
	[DataRow("a.+b", RegexOptions.Singleline, "\"a\"[\\0-\\uFFFF]+\"b\"")]
	[DataRow("ab(?# comment)c", RegexOptions.None, "\"ab\"\"c\"")]
	[DataRow("a   b (?# comment) c", RegexOptions.IgnorePatternWhitespace, "\"a\"\"b\"\"c\"")]
	[DataRow(@"(\*\s*){0,3}", RegexOptions.None, "(\"*\"[\\t-\\r\\u0085\\p{Zs}\\p{Zl}\\p{Zp}]*){0,3}")]
	public void TestParse(string pattern, RegexOptions option, string expected)
	{
		Assert.AreEqual(expected, LexRegex.Parse(pattern, option).ToString());
	}
}
