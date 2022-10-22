using System.Reflection;
using System.Text;
using Cyjb.Reflection;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// 表示特性的模型。
/// </summary>
internal sealed class AttributeModel
{
	/// <summary>
	/// 公共实例成员的绑定标志位。
	/// </summary>
	private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

	/// <summary>
	/// 通过指定的特性类型构造其模型。
	/// </summary>
	/// <typeparam name="T">特性类型。</typeparam>
	/// <remarks>只支持单个构造函数。</remarks>
	/// <returns>特性的模型。</returns>
	public static AttributeModel From<T>()
	{
		Type attributeType = typeof(T);
		StringBuilder signature = new($"{attributeType.Name}.{attributeType.Name}(");
		// 选择参数最多的构造函数。
		ParameterInfo[] parameters = attributeType.GetConstructors()
			.OrderByDescending((ctor) => ctor.GetParametersNoCopy().Length)
			.First()
			.GetParametersNoCopy();
		AttributeParameterInfo[] ctorParams = new AttributeParameterInfo[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ctorParams[i] = new AttributeParameterInfo(parameters[i]);
			if (i > 0)
			{
				signature.Append(", ");
			}
			signature.Append(parameters[i].ParameterType.Name);
		}
		signature.Append(')');
		HashSet<string> namedArguments = new();
		foreach (FieldInfo field in attributeType.GetFields(PublicInstance))
		{
			if (field.Attributes != FieldAttributes.Literal)
			{
				namedArguments.Add(field.Name);
			}
		}
		foreach (PropertyInfo property in attributeType.GetProperties(PublicInstance))
		{
			if (property.CanWrite)
			{
				namedArguments.Add(property.Name);
			}
		}
		return new AttributeModel(signature.ToString(), ctorParams, namedArguments);
	}

	/// <summary>
	/// 使用指定的参数信息初始化 <see cref="AttributeModel"/> 类的新实例。
	/// </summary>
	/// <param name="signature">构造函数的签名。</param>
	/// <param name="ctorParams">构造函数参数名称列表。</param>
	/// <param name="namedParams">命名参数列表。</param>
	private AttributeModel(string signature, AttributeParameterInfo[] ctorParams, IReadOnlySet<string> namedParams)
	{
		Signature = signature;
		ConstructorParameters = ctorParams;
		HasParamsParameter = ctorParams.Length > 0 && ctorParams[^1].IsParamArray;
		NamedParameter = namedParams;
	}

	/// <summary>
	/// 构造函数的签名。
	/// </summary>
	public string Signature { get; }
	/// <summary>
	/// 获取构造函数参数信息列表。
	/// </summary>
	public AttributeParameterInfo[] ConstructorParameters { get; }
	/// <summary>
	/// 获取是否包含 params 参数。
	/// </summary>
	public bool HasParamsParameter { get; }
	/// <summary>
	/// 获取命名参数列表。
	/// </summary>
	public IReadOnlySet<string> NamedParameter { get; }

	/// <summary>
	/// 返回指定构造函数参数名称的索引。
	/// </summary>
	/// <param name="name">要查找的参数名称。</param>
	/// <returns>参数的索引。</returns>
	public int IndexOfConstructorArgument(string name)
	{
		for (int i = 0; i < ConstructorParameters.Length; i++)
		{
			if (ConstructorParameters[i].Name == name)
			{
				return i;
			}
		}
		return -1;
	}
}
