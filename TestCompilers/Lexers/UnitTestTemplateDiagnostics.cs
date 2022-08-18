using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Lexers;

/// <summary>
/// 模板生成的诊断信息单元测试。
/// </summary>
[TestClass]
public class UnitTestTemplateDiagnostics
{
	/// <summary>
	/// 对特性参数的诊断信息进行测试。
	/// </summary>
	[TestMethod]
	public void TestDiagnostics()
	{
		string filePath = @"Lexers\UnitTestTemplateDiagnostics.txt";
		string runtime = Environment.Is64BitOperatingSystem ? "x64" : "x86";
		using Process myProcess = new();
		myProcess.StartInfo.UseShellExecute = false;
		myProcess.StartInfo.RedirectStandardOutput = true;
		myProcess.StartInfo.FileName = $@"..\..\..\..\Design\Tools\win-{runtime}\Generator.exe";
		myProcess.StartInfo.Arguments = Path.GetFullPath(filePath);
		myProcess.StartInfo.CreateNoWindow = true;
		myProcess.Start();
		string content = myProcess.StandardOutput.ReadToEnd();
		myProcess.WaitForExit();

		Assert.AreEqual(string.Join("\r\n", new string[]
		{
			"UnitTestTemplateDiagnostics.txt(1,2): CS1729: “LexerSymbolAttribute”不包含采用 5 个参数的构造函数。",
			"UnitTestTemplateDiagnostics.txt(2,14): CS1739: “LexerSymbolAttribute”的最佳重载没有名为“Regex”的参数。",
			"UnitTestTemplateDiagnostics.txt(3,19): CS1744: 命名参数“regex”指定的形参已被赋予位置参数。",
			"UnitTestTemplateDiagnostics.txt(4,14): CS8323: 命名参数“options”的使用位置不当，但后跟一个未命名参数。",
			"UnitTestTemplateDiagnostics.txt(5,19): CS1525: 表达式项“Test”无效。",
			"UnitTestTemplateDiagnostics.txt(6,31): CS0643: “Kind”重复命名特性参数。",
			"UnitTestTemplateDiagnostics.txt(7,2): CS7036: 未提供与“LexerSymbolAttribute.LexerSymbolAttribute(String, RegexOptions)”的必需形参“regex”对应的实参。",
			"UnitTestTemplateDiagnostics.txt(8,14): CS0030: 无法将类型“System.Int32”转换为“System.String”。",
			"UnitTestTemplateDiagnostics.txt(9,32): CS0117: “RegexOptions”未包含“Test”的定义。",
			"UnitTestTemplateDiagnostics.txt(10,19): CS0030: 无法将类型“System.Double”转换为“System.Text.RegularExpressions.RegexOptions”。",
			"UnitTestTemplateDiagnostics.txt(11,19): CS0030: 无法将类型“System.String”转换为“System.Text.RegularExpressions.RegexOptions”。",
			"UnitTestTemplateDiagnostics.txt(12,15): 无效的词法分析上下文“”。",
			"UnitTestTemplateDiagnostics.txt(16,2): 无效的词法分析符号动作“InvalidAction”，形参必须为空或可选的。",
			"UnitTestTemplateDiagnostics.txt(13,2): 不完整的词法分析上下文。",
		}).Trim(), content.Trim());
	}
}
