using System.Reflection;
using Cyjb.Reflection;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 特性的参数信息。
/// </summary>
internal struct AttributeParameterInfo
{
	/// <summary>
	/// 参数名称。
	/// </summary>
	public string Name;
	/// <summary>
	/// 是否是可选参数。
	/// </summary>
	public bool IsOptional;
	/// <summary>
	/// 是否是 params 参数。
	/// </summary>
	public bool IsParamArray;

	/// <summary>
	/// 使用指定的 <see cref="ParameterInfo"/> 初始化。
	/// </summary>
	/// <param name="param">参数信息。</param>
	public AttributeParameterInfo(ParameterInfo param)
	{
		Name = param.Name!;
		IsOptional = param.IsOptional;
		IsParamArray = param.IsParamArray();
	}
}
