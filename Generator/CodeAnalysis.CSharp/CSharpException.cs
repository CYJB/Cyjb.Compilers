using System.ComponentModel;
using System.Resources;
using Cyjb.Compilers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cyjb.CodeAnalysis.CSharp;

/// <summary>
/// C# 的错误信息。
/// </summary>
internal class CSharpException : Exception
{
	private static ResourceManager? resourceManager;

	/// <summary>
	/// 获取此类使用的缓存的 <see cref="ResourceManager"/> 实例。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			resourceManager ??= new ResourceManager("Microsoft.CodeAnalysis.CSharp.CSharpResources",
					typeof(CSharpSyntaxTree).Assembly);
			return resourceManager;
		}
	}

	/// <summary>
	/// 使用指定的错误码和异常消息初始化 <see cref="CSharpException"/> 类的新实例。
	/// </summary>
	/// <param name="code">错误码。</param>
	/// <param name="message">异常消息。</param>
	/// <param name="location">异常相关的代码位置。</param>
	private CSharpException(int code, string message, Location location) : base(message)
	{
		Code = code;
		Location = location;
	}

	/// <summary>
	/// 获取错误码。
	/// </summary>
	public int Code { get; }
	/// <summary>
	/// 获取异常相关的代码位置。
	/// </summary>
	public Location Location { get; }

	/// <summary>
	/// 返回表示 <c>CS0030: Cannot convert type '{0}' to '{1}'.</c> 的异常对象。
	/// </summary>
	internal static CSharpException NoExplicitConv(object fromType, object toType, Location location)
	{
		string message = string.Format(null, ResourceManager.GetString("ERR_NoExplicitConv")!, fromType, toType);
		return new CSharpException(30, message, location);
	}

	/// <summary>
	/// 返回表示 <c>CS0117: '{0}' does not contain a definition for '{1}'.</c> 的异常对象。
	/// </summary>
	internal static CSharpException NoSuchMember(string typeName, string memberName, Location location)
	{
		string message = string.Format(null, ResourceManager.GetString("ERR_NoSuchMember")!, typeName, memberName);
		return new CSharpException(117, message, location);
	}

	/// <summary>
	/// 返回表示 <c>CS0617: '{0}' is not a valid named attribute argument. Named attribute arguments must be fields which are not readonly, static, or const, or read-write properties which are public and not static.</c> 的异常对象。
	/// </summary>
	internal static CSharpException BadNamedAttributeArgument(string name, Location location)
	{
		string message = string.Format(null, ResourceManager.GetString("ERR_BadNamedAttributeArgument")!, name);
		return new CSharpException(617, message, location);
	}

	/// <summary>
	/// 返回表示 <c>CS0643: '{0}' duplicate named attribute argument.</c> 的异常对象。
	/// </summary>
	internal static CSharpException DuplicateNamedAttributeArgument(string name, Location location)
	{
		string message = string.Format(null, ResourceManager.GetString("ERR_DuplicateNamedAttributeArgument")!, name);
		return new CSharpException(643, message, location);
	}

	/// <summary>
	/// 返回表示 <c>CS1016: Named attribute argument expected.</c> 的异常对象。
	/// </summary>
	internal static CSharpException NamedArgumentExpected(Location location)
	{
		return new CSharpException(1016, ResourceManager.GetString("ERR_NamedArgumentExpected")!, location);
	}

	/// <summary>
	/// 返回表示 <c>CS1525: Invalid expression term '{0}'.</c> 的异常对象。
	/// </summary>
	internal static CSharpException InvalidExprTerm(string name, Location location)
	{
		string message = string.Format(null, ResourceManager.GetString("ERR_InvalidExprTerm")!, name);
		return new CSharpException(1525, message, location);
	}

	/// <summary>
	/// 返回表示 <c>CS1729: '{0}' does not contain a constructor that takes {1} arguments.</c> 的异常对象。
	/// </summary>
	internal static CSharpException BadCtorArgCount(string name, int argCount, Location location)
	{
		string message = string.Format(null, ResourceManager.GetString("ERR_BadCtorArgCount")!, name, argCount);
		return new CSharpException(1729, message, location);
	}

	/// <summary>
	/// 返回表示 <c>CS1739: The best overload for '{0}' does not have a parameter named '{1}'.</c> 的异常对象。
	/// </summary>
	internal static CSharpException BadNamedArgument(string attrName, string argName, Location location)
	{
		string message = string.Format(null, ResourceManager.GetString("ERR_BadNamedArgument")!, attrName, argName);
		return new CSharpException(1739, message, location);
	}

	/// <summary>
	/// 返回表示 <c>CS1744: Named argument '{0}' specifies a parameter for which a positional argument has already been given.</c> 的异常对象。
	/// </summary>
	internal static CSharpException NamedArgumentUsedInPositional(string argName, Location location)
	{
		string message = string.Format(null, ResourceManager.GetString("ERR_NamedArgumentUsedInPositional")!, argName);
		return new CSharpException(1744, message, location);
	}

	/// <summary>
	/// 返回表示 <c>CS7036: There is no argument given that corresponds to the required formal parameter '{0}' of '{1}'.</c> 的异常对象。
	/// </summary>
	internal static CSharpException NoCorrespondingArgument(string name, string signature, Location location)
	{
		string message = string.Format(null, ResourceManager.GetString("ERR_NoCorrespondingArgument")!, name, signature);
		return new CSharpException(7036, message, location);
	}

	/// <summary>
	/// 返回表示 <c>CS8323: Named argument '{0}' is used out-of-position but is followed by an unnamed argument.</c> 的异常对象。
	/// </summary>
	internal static CSharpException BadNonTrailingNamedArgument(string name, Location location)
	{
		string message = string.Format(null, ResourceManager.GetString("ERR_BadNonTrailingNamedArgument")!, name);
		return new CSharpException(8323, message, location);
	}

	/// <summary>
	/// 返回当前异常的字符串表示形式。
	/// </summary>
	/// <returns>当前异常的字符串表示形式。</returns>
	public override string ToString()
	{
		return Resources.CSharpError(Code.ToString().PadLeft(4, '0'), Message);
	}
}
