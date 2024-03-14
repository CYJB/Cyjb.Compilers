using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

/// <summary>
/// 模板生成的诊断信息单元测试。
/// </summary>
[TestClass]
public class UnitTestTemplateDiagnostics
{
	private static readonly string[] ExpectedZH = new string[]
	{
		"UnitTestTemplateDiagnostics.template.cs(1,2): CS1729: “LexerSymbolAttribute”不包含采用 5 个参数的构造函数。",
		"UnitTestTemplateDiagnostics.template.cs(2,14): CS1739: “LexerSymbolAttribute”的最佳重载没有名为“Regex”的参数。",
		"UnitTestTemplateDiagnostics.template.cs(3,19): CS1744: 命名参数“regex”指定的形参已被赋予位置参数。",
		"UnitTestTemplateDiagnostics.template.cs(4,14): CS8323: 命名参数“options”的使用位置不当，但后跟一个未命名参数。",
		"UnitTestTemplateDiagnostics.template.cs(5,19): CS1525: 表达式项“Test”无效。",
		"UnitTestTemplateDiagnostics.template.cs(6,31): CS0643: “Kind”重复命名特性参数。",
		"UnitTestTemplateDiagnostics.template.cs(7,2): CS7036: 未提供与“LexerSymbolAttribute.LexerSymbolAttribute(String, RegexOptions)”的所需参数“regex”对应的参数。",
		"UnitTestTemplateDiagnostics.template.cs(8,14): CS0030: 无法将类型“System.Int32”转换为“System.String”。",
		"UnitTestTemplateDiagnostics.template.cs(9,32): CS0117: “RegexOptions”未包含“Test”的定义。",
		"UnitTestTemplateDiagnostics.template.cs(10,19): CS0030: 无法将类型“System.Double”转换为“System.Text.RegularExpressions.RegexOptions”。",
		"UnitTestTemplateDiagnostics.template.cs(11,19): CS0030: 无法将类型“System.String”转换为“System.Text.RegularExpressions.RegexOptions”。",
		"UnitTestTemplateDiagnostics.template.cs(12,15): 无效的词法分析上下文“”。",
		"UnitTestTemplateDiagnostics.template.cs(16,2): 无效的词法分析符号动作“InvalidAction”，形参必须为空或可选的。",
	};
	private static readonly string[] ExpectedOther = new string[]
	{
		"UnitTestTemplateDiagnostics.template.cs(1,2): CS1729: 'LexerSymbolAttribute' does not contain a constructor that takes 5 arguments.",
		"UnitTestTemplateDiagnostics.template.cs(2,14): CS1739: The best overload for 'LexerSymbolAttribute' does not have a parameter named 'Regex'.",
		"UnitTestTemplateDiagnostics.template.cs(3,19): CS1744: Named argument 'regex' specifies a parameter for which a positional argument has already been given.",
		"UnitTestTemplateDiagnostics.template.cs(4,14): CS8323: Named argument 'options' is used out-of-position but is followed by an unnamed argument.",
		"UnitTestTemplateDiagnostics.template.cs(5,19): CS1525: Invalid expression term 'Test'.",
		"UnitTestTemplateDiagnostics.template.cs(6,31): CS0643: 'Kind' duplicate named attribute argument.",
		"UnitTestTemplateDiagnostics.template.cs(7,2): CS7036: There is no argument given that corresponds to the required formal parameter 'regex' of 'LexerSymbolAttribute.LexerSymbolAttribute(String, RegexOptions)'.",
		"UnitTestTemplateDiagnostics.template.cs(8,14): CS0030: Cannot convert type 'System.Int32' to 'System.String'.",
		"UnitTestTemplateDiagnostics.template.cs(9,32): CS0117: 'RegexOptions' does not contain a definition for 'Test'.",
		"UnitTestTemplateDiagnostics.template.cs(10,19): CS0030: Cannot convert type 'System.Double' to 'System.Text.RegularExpressions.RegexOptions'.",
		"UnitTestTemplateDiagnostics.template.cs(11,19): CS0030: Cannot convert type 'System.String' to 'System.Text.RegularExpressions.RegexOptions'.",
		"UnitTestTemplateDiagnostics.template.cs(12,15): Invalid lexer context ''.",
		"UnitTestTemplateDiagnostics.template.cs(16,2): Invalid lexer symbol action 'InvalidAction', parameter must empty or optional.",
	};

	/// <summary>
	/// 对特性参数的诊断信息进行测试。
	/// </summary>
	[TestMethod]
	public void TestDiagnostics()
	{
		string filePath = @"Lexers\UnitTestTemplateDiagnostics.template.cs";
		using Process myProcess = new();
		myProcess.StartInfo.UseShellExecute = false;
		myProcess.StartInfo.RedirectStandardOutput = true;
		myProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
		myProcess.StartInfo.FileName = "dotnet";
		myProcess.StartInfo.Arguments = @$"""..\..\..\..\Design\Tools\Generator.dll"" ""{Path.GetFullPath(filePath)}""";
		myProcess.StartInfo.CreateNoWindow = true;
		myProcess.Start();
		string content = myProcess.StandardOutput.ReadToEnd();
		myProcess.WaitForExit();

		string[] expected = CultureInfo.CurrentUICulture.Name.StartsWith("zh-") ? ExpectedZH : ExpectedOther;
		Assert.AreEqual(string.Join("\r\n", expected).Trim(), content.Trim());
	}
}
