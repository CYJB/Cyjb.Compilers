using System.Diagnostics;
using System.Text;

namespace Cyjb.Compilers.RegularExpressions;

/// <summary>
/// 表示字符类的正则表达式。
/// </summary>
/// <seealso cref="RegexCharClass"/>
public sealed class CharClassExp : LexRegex
{

	#region 预定义的字符类正则表达式

	/// <summary>
	/// 与任何非换行字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? anyCharExp;

	/// <summary>
	/// 获取与任何非换行字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何非换行字符匹配的字符类正则表达式。</value>
	internal static CharClassExp AnyCharExp
	{
		get
		{
			if (anyCharExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddChar('\r');
				charClass.AddChar('\n');
				charClass.SetNegated();
				anyCharExp = new(charClass);
			}
			return anyCharExp;
		}
	}

	/// <summary>
	/// 与任何字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? singleLineAnyCharExp;

	/// <summary>
	/// 获取与任何字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何字符匹配的字符类正则表达式。</value>
	internal static CharClassExp SingleLineAnyCharExp
	{
		get
		{
			if (singleLineAnyCharExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddRange('\0', char.MaxValue);
				singleLineAnyCharExp = new(charClass);
			}
			return singleLineAnyCharExp;
		}
	}

	/// <summary>
	/// 与任何空白字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? spaceClassExp;

	/// <summary>
	/// 获取与任何空白字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何空白字符匹配的字符类正则表达式。</value>
	internal static CharClassExp SpaceClassExp
	{
		get
		{
			if (spaceClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddSpace(false, false);
				spaceClassExp = new(charClass);
			}
			return spaceClassExp;
		}
	}

	/// <summary>
	/// 与任何非空白字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? notSpaceClassExp;

	/// <summary>
	/// 获取与任何非空白字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何非空白字符匹配的字符类正则表达式。</value>
	internal static CharClassExp NotSpaceClassExp
	{
		get
		{
			if (notSpaceClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddSpace(false, true);
				notSpaceClassExp = new(charClass);
			}
			return notSpaceClassExp;
		}
	}

	/// <summary>
	/// 与任何 ECMA 空白字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? ecmaSpaceClassExp;

	/// <summary>
	/// 获取与任何 ECMA 空白字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何 ECMA 空白字符匹配的字符类正则表达式。</value>
	internal static CharClassExp ECMASpaceClassExp
	{
		get
		{
			if (ecmaSpaceClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddSpace(true, false);
				ecmaSpaceClassExp = new(charClass);
			}
			return ecmaSpaceClassExp;
		}
	}

	/// <summary>
	/// 与任何 ECMA 非空白字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? ecmaNotSpaceClassExp;

	/// <summary>
	/// 获取与任何 ECMA 非空白字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何 ECMA 非空白字符匹配的字符类正则表达式。</value>
	internal static CharClassExp ECMANotSpaceClassExp
	{
		get
		{
			if (ecmaNotSpaceClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddSpace(true, true);
				ecmaNotSpaceClassExp = new(charClass);
			}
			return ecmaNotSpaceClassExp;
		}
	}

	/// <summary>
	/// 与任何数字字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? digitClassExp;

	/// <summary>
	/// 获取与任何数字字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何数字字符匹配的字符类正则表达式。</value>
	internal static CharClassExp DigitClassExp
	{
		get
		{
			if (digitClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddDigit(false, false);
				digitClassExp = new(charClass);
			}
			return digitClassExp;
		}
	}

	/// <summary>
	/// 与任何非数字字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? notDigitClassExp;

	/// <summary>
	/// 获取与任何非数字字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何非数字字符匹配的字符类正则表达式。</value>
	internal static CharClassExp NotDigitClassExp
	{
		get
		{
			if (notDigitClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddDigit(false, true);
				notDigitClassExp = new(charClass);
			}
			return notDigitClassExp;
		}
	}

	/// <summary>
	/// 与任何 ECMA 数字字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? ecmaDigitClassExp;

	/// <summary>
	/// 获取与任何 ECMA 数字字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何 ECMA 数字字符匹配的字符类正则表达式。</value>
	internal static CharClassExp ECMADigitClassExp
	{
		get
		{
			if (ecmaDigitClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddDigit(true, false);
				ecmaDigitClassExp = new(charClass);
			}
			return ecmaDigitClassExp;
		}
	}

	/// <summary>
	/// 与任何 ECMA 非数字字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? ecmaNotDigitClassExp;

	/// <summary>
	/// 获取与任何 ECMA 非数字字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何 ECMA 非数字字符匹配的字符类正则表达式。</value>
	internal static CharClassExp ECMANotDigitClassExp
	{
		get
		{
			if (ecmaNotDigitClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddDigit(true, true);
				ecmaNotDigitClassExp = new(charClass);
			}
			return ecmaNotDigitClassExp;
		}
	}

	/// <summary>
	/// 与任何单词字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? wordClassExp;

	/// <summary>
	/// 获取与任何单词字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何单词字符匹配的字符类正则表达式。</value>
	internal static CharClassExp WordClassExp
	{
		get
		{
			if (wordClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddWord(false, false);
				wordClassExp = new(charClass);
			}
			return wordClassExp;
		}
	}

	/// <summary>
	/// 与任何非单词字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? notWordClassExp;

	/// <summary>
	/// 获取与任何非单词字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何非单词字符匹配的字符类正则表达式。</value>
	internal static CharClassExp NotWordClassExp
	{
		get
		{
			if (notWordClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddWord(false, true);
				notWordClassExp = new(charClass);
			}
			return notWordClassExp;
		}
	}

	/// <summary>
	/// 与任何 ECMA 单词字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? ecmaWordClassExp;

	/// <summary>
	/// 获取与任何 ECMA 单词字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何 ECMA 单词字符匹配的字符类正则表达式。</value>
	internal static CharClassExp ECMAWordClassExp
	{
		get
		{
			if (ecmaWordClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddWord(true, false);
				ecmaWordClassExp = new(charClass);
			}
			return ecmaWordClassExp;
		}
	}

	/// <summary>
	/// 与任何 ECMA 非单词字符匹配的字符类正则表达式。
	/// </summary>
	private static CharClassExp? ecmaNotWordClassExp;

	/// <summary>
	/// 获取与任何 ECMA 非单词字符匹配的字符类正则表达式。
	/// </summary>
	/// <value>任何 ECMA 非单词字符匹配的字符类正则表达式。</value>
	internal static CharClassExp ECMANotWordClassExp
	{
		get
		{
			if (ecmaNotWordClassExp == null)
			{
				RegexCharClass charClass = new();
				charClass.AddWord(true, true);
				ecmaNotWordClassExp = new(charClass);
			}
			return ecmaNotWordClassExp;
		}
	}

	#endregion // 预定义的字符类正则表达式

	/// <summary>
	/// 正则表达式表示的字符类。
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly RegexCharClass charClass;

	/// <summary>
	/// 使用表示的字符类初始化 <see cref="CharClassExp"/> 类的新实例。
	/// </summary>
	/// <param name="charClass">正则表达式表示的字符类。</param>
	internal CharClassExp(RegexCharClass charClass)
	{
		this.charClass = charClass;
	}

	/// <summary>
	/// 获取正则表达式表示的字符类。
	/// </summary>
	public RegexCharClass CharClass => charClass;

	/// <summary>
	/// 获取当前正则表达式匹配的字符串长度。
	/// </summary>
	/// <value>当前正则表达式匹配的字符串长度。如果可以匹配不同长度的字符串，则为 <c>null</c>。</value>
	public override int? Length => 1;

	/// <summary>
	/// 返回当前对象是否等于同一类型的另一对象。
	/// </summary>
	/// <param name="other">要比较的对象。</param>
	/// <returns>如果当前对象等于 <paramref name="other"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
	public override bool Equals(LexRegex? other)
	{
		if (other is CharClassExp exp)
		{
			return charClass == exp.charClass;
		}
		return false;
	}

	/// <summary>
	/// 返回当前对象的哈希值。
	/// </summary>
	/// <returns>当前对象的哈希值。</returns>
	public override int GetHashCode()
	{
		return charClass.GetHashCode();
	}

	/// <summary>
	/// 返回当前对象的字符串表示形式。
	/// </summary>
	/// <param name="builder">字符串构造器。</param>
	internal override void ToString(StringBuilder builder)
	{
		charClass.ToString(builder);
	}
}
