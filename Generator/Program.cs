using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.Compilers;

class Program
{
	static int Main(string[] args)
	{
		// 使用 UTF-8 输出，确保产生式的定点可以正常输出。
		Console.OutputEncoding = Encoding.UTF8;
		if (args.Length == 0)
		{
			Console.WriteLine("根据已声明的词法/语法分析控制器生成相应的预编译代码");
			Console.WriteLine("使用方式: Generator <file>");
			Console.WriteLine("参数:");
			Console.WriteLine("  <file> 已声明的词法/语法分析控制器文件。");
			return 0;
		}
		string file = args[0];
		string fileName = Path.GetFileNameWithoutExtension(file);
		// 找到所有相关文件。
		var files = Directory.EnumerateFiles(Path.GetDirectoryName(file) ?? "")
			.Where((path) =>
			{
				if (Path.GetExtension(path) != ".cs")
				{
					return false;
				}
				string curName = Path.GetFileNameWithoutExtension(path);
				if (curName == fileName)
				{
					return true;
				}
				return curName.StartsWith(fileName) && IsSeparator(curName[fileName.Length]);
			})
			.OrderBy(Path.GetFileNameWithoutExtension);
		ControllerVisitor visitor = new();
		bool isFirst = true;
		foreach (string filePath in files)
		{
			string content;
			try
			{
				content = File.ReadAllText(filePath);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{fileName}(0,0): {ex.Message}");
				return 0;
			}
			visitor.IsMain = isFirst;
			isFirst = false;
			// 如果基于语义模型解析，可能需要引入项目中的其它文件和其它引用依赖库，整体更为复杂。
			// 因此这里仅根据语法模型进行解析。
			var root = CSharpSyntaxTree.ParseText(content, CSharpParseOptions.Default, filePath)
				.GetCompilationUnitRoot();
			// 检查是否存在任何语法错误。
			if (root.ContainsDiagnostics)
			{
				foreach (Diagnostic diagnostic in root.GetDiagnostics())
				{
					visitor.Context.AddError(diagnostic.GetMessage(), diagnostic.Location);
				}
				break;
			}
			root.Accept(visitor);
			if (visitor.Context.HasError)
			{
				break;
			}
		}
		Console.WriteLine(visitor.Generate());
		return 0;
	}

	/// <summary>
	/// 返回指定字符是否是支持的文件名分隔符。
	/// </summary>
	/// <param name="ch">要检查的字符。</param>
	/// <returns>指定字符是否是支持的文件名分隔符。</returns>
	private static bool IsSeparator(char ch)
	{
		return ch == ' ' || ch == '-' || ch == '-' || ch == '.';
	}
}
