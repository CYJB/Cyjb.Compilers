using System.Reflection;
using System.Text.RegularExpressions;
using Cyjb.Reflection;

namespace Cyjb.Compilers.Lexers;

/// <summary>
/// 提供根据特性构造词法分析器的功能。
/// </summary>
public class Lexer
{
	/// <summary>
	/// 方法的标志。
	/// </summary>
	private const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	/// <summary>
	/// 返回词法分析的数据。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
	/// <typeparam name="TController">词法分析控制器的类型。</typeparam>
	/// <returns>词法分析的数据。</returns>
	/// <exception cref="CompileException">创建词法分析器出现异常。</exception>
	/// <remarks>
	/// <para>泛型参数 <typeparamref name="T"/> 一般是一个枚举类型，用于标识词法单元。</para>
	/// <para>对于词法分析中的冲突，总是选择最长的词素。如果最长的词素可以与多个模式匹配，
	/// 则选择 <see cref="LexerSymbolAttribute.Priority"/> 更高的模式。</para></remarks>
	public static LexerData<T> GetData<T, TController>()
		where T : struct
		where TController : LexerController<T>, new()
	{
		Type controllerType = typeof(TController);
		if (!controllerType.IsAssignableTo(typeof(LexerController<T>)))
		{
			throw new CompileException(Resources.NotExtendsLexerController(controllerType));
		}
		Lexer<T, TController> lexer = new();
		List<LexerSymbolAttrInfo> symbolInfos = new();
		bool rejectable = false;
		// 提取类特性。
		foreach (CustomAttributeData attr in controllerType.CustomAttributes)
		{
			Type attrType = attr.AttributeType;
			if (attrType == typeof(LexerRejectableAttribute))
			{
				rejectable = true;
			}
			else if (attrType == typeof(LexerContextAttribute))
			{
				string? label = (attr.ConstructorArguments[0].Value as string);
				if (!label.IsNullOrEmpty())
				{
					lexer.DefineContext(label);
				}
			}
			else if (attrType == typeof(LexerInclusiveContextAttribute))
			{
				string? label = (attr.ConstructorArguments[0].Value as string);
				if (!label.IsNullOrEmpty())
				{
					lexer.DefineInclusiveContext(label);
				}
			}
			else if (attrType == typeof(LexerRegexAttribute))
			{
				string? name = attr.ConstructorArguments[0].Value as string;
				string? regex = attr.ConstructorArguments[1].Value as string;
				if (!name.IsNullOrEmpty() && !regex.IsNullOrEmpty())
				{
					RegexOptions options = RegexOptions.None;
					if (attr.ConstructorArguments.Count > 2)
					{
						options = (RegexOptions)attr.ConstructorArguments[2].Value!;
					}
					lexer.DefineRegex(name, regex, options);
				}
			}
			else if (attrType == typeof(LexerSymbolAttribute))
			{
				if (LexerSymbolAttrInfo.TryGetInfo(attr, typeof(T), out var info))
				{
					symbolInfos.Add(info);
				}
			}
		}
		// 提取方法特性。
		foreach (MethodInfo method in controllerType.GetMethods(MethodFlags))
		{
			foreach (CustomAttributeData attr in method.CustomAttributes)
			{
				if (attr.AttributeType == typeof(LexerSymbolAttribute) &&
					LexerSymbolAttrInfo.TryGetInfo(attr, typeof(T), out var info))
				{
					ParameterInfo[] parameters = method.GetParametersNoCopy();
					// 检查是否是可选或 params 参数
					foreach (ParameterInfo param in parameters)
					{
						if (!param.IsParamArray() && !param.IsOptional)
						{
							throw new CompileException(Resources.InvalidLexerSymbolAction(method.Name));
						}
					}
					info.Method = method;
					symbolInfos.Add(info);
				}
			}
		}
		// 按照优先级排序。
		symbolInfos.Sort((LexerSymbolAttrInfo left, LexerSymbolAttrInfo right) => right.Priority - left.Priority);
		// 添加终结符定义
		foreach (LexerSymbolAttrInfo info in symbolInfos)
		{
			string regex = info.Regex;
			// 解析 symbol 开头的 Context
			string[] contexts = Array.Empty<string>();
			if (regex[0] == '<' && !regex.StartsWith("<<EOF>>"))
			{
				// 处理上下文。
				int idx = regex.IndexOf('>');
				if (idx == -1)
				{
					throw new CompileException(Resources.IncompleteLexerContext);
				}
				string context = regex[1..idx];
				regex = regex[(idx + 1)..];
				if (context == "*")
				{
					contexts = lexer.Contexts;
				}
				else
				{
					contexts = new string(context).Split(',');
				}
			}
			var builder = lexer.DefineSymbol(regex, info.RegexOptions);
			if (contexts.Length > 0)
			{
				builder.Context(contexts);
			}
			if (info.Kind != null)
			{
				builder.Kind((T)info.Kind);
			}
			if (info.Method != null)
			{
				builder.Action(info.Method.PowerDelegate<Action<TController>>()!);
			}
		}
		return lexer.GetData(rejectable);
	}

	/// <summary>
	/// 返回词法分析的工厂。
	/// </summary>
	/// <typeparam name="T">词法单元标识符的类型，一般是一个枚举类型。</typeparam>
	/// <typeparam name="TController">词法分析控制器的类型。</typeparam>
	/// <returns>词法分析器的工厂。</returns>
	public static LexerFactory<T, TController> GetFactory<T, TController>()
		where T : struct
		where TController : LexerController<T>, new()
	{
		return new LexerFactory<T, TController>(GetData<T, TController>());
	}
}

