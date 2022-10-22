using Cyjb.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cyjb.Compilers;

/// <summary>
/// 词法/语法分析控制器。
/// </summary>
internal abstract class Controller
{
	/// <summary>
	/// 使用指定的控制器名称和标识符类型初始化 <see cref="Controller"/> 类的新实例。
	/// </summary>
	/// <param name="context">模板上下文。</param>
	/// <param name="syntax">控制器语法节点。</param>
	/// <param name="kindType">标识符类型。</param>
	protected Controller(TransformationContext context, ClassDeclarationSyntax syntax, string kindType)
	{
		Context = context;
		Name = syntax.Identifier.ToString();
		KindType = kindType;
		Format = new SyntaxFormat(syntax).IncDepth();
	}

	/// <summary>
	/// 获取控制器的名称。
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// 获取模板上下文。
	/// </summary>
	protected TransformationContext Context { get; }
	/// <summary>
	/// 获取标识符的类型。
	/// </summary>
	protected string KindType { get; }
	/// <summary>
	/// 获取语法的格式化信息。
	/// </summary>
	protected SyntaxFormat Format { get; }

	/// <summary>
	/// 解析控制器语法节点。
	/// </summary>
	/// <param name="controllerSyntax">控制器语法节点。</param>
	public abstract void Parse(ClassDeclarationSyntax controllerSyntax);

	/// <summary>
	/// 生成控制器的成员。
	/// </summary>
	/// <returns>控制器的成员。</returns>
	public abstract IEnumerable<MemberDeclarationSyntax> Generate();
}
