using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using Cyjb.IO;

namespace Cyjb.Compiler.RegularExpressions
{
	/// <summary>
	/// 表示正则表达式的分析器。
	/// </summary>
	internal sealed class RegexParser : IDisposable
	{
		/// <summary>
		/// 整数的最大值除以 10。
		/// </summary>
		private const int MaxValueDiv10 = int.MaxValue / 10;
		/// <summary>
		/// 整数的最大值模 10。
		/// </summary>
		private const int MaxValueMod10 = int.MaxValue % 10;
		/// <summary>
		/// 正则表达式的模式字符串。
		/// </summary>
		private readonly string pattern;
		/// <summary>
		/// 原字符串的读取器。
		/// </summary>
		private SourceReader reader;
		/// <summary>
		/// 正则表达式的定义。
		/// </summary>
		private IDictionary<string, Regex> regexDefinition;
		/// <summary>
		/// 语言区域性信息。
		/// </summary>
		private readonly CultureInfo culture;
		/// <summary>
		/// 正则表达式的选项。
		/// </summary>
		private RegexOptions options;
		/// <summary>
		/// 正则表达式的选项堆栈。
		/// </summary>
		private readonly Stack<RegexOptions> optionsStack = new Stack<RegexOptions>();
		/// <summary>
		/// 当前"或"的正则表达式。
		/// </summary>
		private List<Regex> alternation = new List<Regex>();
		/// <summary>
		/// 当前"连接"的正则表达式。
		/// </summary>
		private List<Regex> concatenate = new List<Regex>();
		/// <summary>
		/// 正则表达式的组的堆栈。
		/// </summary>
		private readonly Stack<Tuple<List<Regex>, List<Regex>>> groupStack =
			new Stack<Tuple<List<Regex>, List<Regex>>>();
		/// <summary>
		/// 当前的正则表达式。
		/// </summary>
		private Regex current;
		/// <summary>
		/// 向前看正则表达式。
		/// </summary>
		private Regex trailing = null;
		/// <summary>
		/// 是否正在处理向前看正则表达式。
		/// </summary>
		private bool inTrailing = false;
		/// <summary>
		/// 根据给定的源文件解析正则表达式。
		/// </summary>
		/// <param name="reader">正则表达式的源文件读取器。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		/// <returns>解析得到的正则表达式。</returns>
		internal static Regex ParseRegex(SourceReader reader, RegexOptions option, IDictionary<string, Regex> regexDef)
		{
			RegexParser parser = new RegexParser(reader, option, regexDef);
			return parser.StartScanRegex();
		}
		/// <summary>
		/// 根据给定的字符串解析正则表达式。
		/// </summary>
		/// <param name="pattern">正则表达式的模式字符串。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		/// <returns>解析得到的正则表达式。</returns>
		internal static Regex ParseRegex(string pattern, RegexOptions option, IDictionary<string, Regex> regexDef)
		{
			RegexParser parser = new RegexParser(pattern, option, regexDef);
			return parser.StartScanRegex();
		}
		/// <summary>
		/// 根据给定的字符串解析正则表达式的字符类。
		/// </summary>
		/// <param name="pattern">正则表达式的模式字符串。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <param name="closeBracket">是否要求关闭括号。</param>
		/// <returns>解析得到的正则表达式的字符类。</returns>
		internal static RegexCharClass ParseCharClass(string pattern, RegexOptions option, bool closeBracket)
		{
			RegexParser parser = new RegexParser(pattern, option, null);
			return parser.ScanCharClass(closeBracket);
		}
		/// <summary>
		/// 使用指定原文件读取器和选项初始化 <see cref="RegexParser"/> 类的新实例。
		/// </summary>
		/// <param name="reader">正则表达式的源文件读取器。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		private RegexParser(SourceReader reader, RegexOptions option, IDictionary<string, Regex> regexDef)
			: this(option, regexDef)
		{
			this.pattern = null;
			this.reader = reader;
		}
		/// <summary>
		/// 使用指定模式字符串和选项初始化 <see cref="RegexParser"/> 类的新实例。
		/// </summary>
		/// <param name="pattern">正则表达式的模式字符串。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		private RegexParser(string pattern, RegexOptions option, IDictionary<string, Regex> regexDef)
			: this(option, regexDef)
		{
			this.pattern = pattern;
			this.reader = new SourceReader(new StringReader(pattern));
		}
		/// <summary>
		/// 使用指定的选项和定义初始化 <see cref="RegexParser"/> 类的新实例。
		/// </summary>
		/// <param name="option">正则表达式的选项。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		private RegexParser(RegexOptions option, IDictionary<string, Regex> regexDef)
		{
			if (regexDef == null)
			{
				this.regexDefinition = new Dictionary<string, Regex>();
			}
			else
			{
				this.regexDefinition = regexDef;
			}
			options = option;
			culture = option.HasFlag(RegexOptions.CultureInvariant) ?
				CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;
		}

		#region IDisposable 成员

		/// <summary>
		/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
		/// </summary>
		public void Dispose()
		{
			if (pattern != null)
			{
				// 仅当 SourceReader 是由自己创建的时候才去释放。
				reader.Dispose();
			}
			GC.SuppressFinalize(this);
		}

		#endregion // IDisposable 成员

		/// <summary>
		/// 扫描正则表达式并转换为 <see cref="Regex"/> 对象。
		/// </summary>
		/// <returns><see cref="Regex"/> 对象。</returns>
		private Regex StartScanRegex()
		{
			// <<EOF>> 的特殊判断。
			int idx = reader.Index;
			if (reader.Read() == '<' && reader.Read() == '<' &&
				reader.Read() == 'E' && reader.Read() == 'O' && reader.Read() == 'F' &&
				reader.Read() == '>' && reader.Read() == '>' && EndOfPattern)
			{
				return Regex.EndOfFile();
			}
			else
			{
				reader.Unget(reader.Index - idx);
			}
			// ^ 仅在开头表示行起始，否则表示普通的符号。
			bool beginningOfLine = (reader.Peek() == '^');
			// 跳过 ^ 符号。
			if (beginningOfLine)
			{
				reader.Read();
			}
			Regex tempRegex = ScanRegex();
			if (beginningOfLine)
			{
				tempRegex = tempRegex.BeginningOfLine();
			}
			return tempRegex;
		}
		/// <summary>
		/// 扫描正则表达式并转换为 <see cref="Regex"/> 对象。
		/// </summary>
		/// <returns><see cref="Regex"/> 对象。</returns>
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private Regex ScanRegex()
		{
			// 是否需要读取向前看符号。
			bool readTrailing = false;
			// 非特殊的字符，标识开始。
			int ich = '@';
			// 是否是数量词。
			bool isQuantifier = false;
			while (!EndOfPattern)
			{
				bool wasPrevQuantifier = isQuantifier;
				isQuantifier = false;
				ScanBlank();
				string normalChars = ScanNormalChars();
				ScanBlank();
				if (EndOfPattern)
				{
					// 非特殊的字符，表示结束。
					ich = '!';
				}
				else if (IsSpecial(ich = reader.Peek()))
				{
					isQuantifier = IsQuantifier(ich);
					// 排除 {name} 情况。
					if (isQuantifier && ich == '{' && !IsTrueQuantifier())
					{
						isQuantifier = false;
					}
					reader.Read();
				}
				else
				{
					// 非特殊的字符，表示普通的字符。
					ich = ' ';
				}
				if (normalChars.Length > 0)
				{
					wasPrevQuantifier = false;
					// 如果之后是量词的话，量词只针对最后一个字符。
					if (isQuantifier)
					{
						if (normalChars.Length > 1)
						{
							AddLiteral(normalChars.Substring(0, normalChars.Length - 1));
						}
						AddSymbol(normalChars[normalChars.Length - 1]);
					}
					else
					{
						AddLiteral(normalChars);
					}
				}
				switch (ich)
				{
					case '!':
						goto BreakOuterScan;
					case ' ':
						goto ContinueOuterScan;
					case '[':
						AddCharClass(ScanCharClass(true).ToStringClass());
						break;
					case '(':
						PushOptions();
						if (ScanGroupOpen())
						{
							PushGroup();
						}
						else
						{
							PopKeepOptions();
						}
						continue;
					case '|':
						AddAlternate();
						goto ContinueOuterScan;
					case ')':
						if (EmptyStack)
						{
							ThrowTooManyParens();
						}
						AddGroup();
						PopGroup();
						if (current == null)
						{
							goto ContinueOuterScan;
						}
						break;
					case '\\':
						AddRegex(ScanBackslash());
						break;
					case '/':
						if (inTrailing)
						{
							ThrowNestedTrailing();
						}
						readTrailing = true;
						goto BreakOuterScan;
					case '$':
						// $ 仅在结尾表示行结尾，否则表示普通的符号。
						// 在 Trailing 中，最后的 $ 不再认为是行结束符。
						if (EndOfPattern && !inTrailing)
						{
							trailing = AnchorExp.EndOfLine;
							goto BreakOuterScan;
						}
						else
						{
							AddSymbol('$');
						}
						break;
					case '.':
						AddRegex(Regex.AnyChar(UseOptionSingleLine));
						break;
					case '{':
					case '*':
					case '+':
					case '?':
						if (current == null)
						{
							if (ich == '{')
							{
								// {regexName} 情况。
								reader.Drop();
								SourceLocation start = reader.StartLocation;
								string name = ScanCapname();
								SourceLocation end = reader.StartLocation;
								if (reader.Read() == '}')
								{
									if (!regexDefinition.TryGetValue(name, out current))
									{
										ThrowUndefinedRegex(name, start, end);
									}
									break;
								}
								else
								{
									ThrowIncompleteRegexReference();
								}
							}
							else if (wasPrevQuantifier)
							{
								ThrowNestedQuantify(((char)ich).ToString());
							}
							else
							{
								ThrowQuantifyAfterNothing();
							}
						}
						reader.Unget();
						break;
					case '"':
						// 字符串。
						string str = ScanLiteral();
						if (UseOptionIgnoreCase)
						{
							AddRegex(Regex.LiteralIgnoreCase(str, culture));
						}
						else
						{
							AddRegex(Regex.Literal(str));
						}
						break;
					default:
						Debug.Fail("内部的 ScanRegex 错误");
						break;
				}
				ScanBlank();
				if (EndOfPattern || !(isQuantifier = IsTrueQuantifier()))
				{
					AddConcatenate();
					goto ContinueOuterScan;
				}
				ScanRepeat();
			ContinueOuterScan:
				;
			}
		BreakOuterScan:
			if (!EmptyStack)
			{
				ThrowNotEnoughParens();
			}
			AddGroup();
			Regex tempRegex = current;
			if (readTrailing)
			{
				inTrailing = true;
				trailing = ScanRegex();
				inTrailing = false;
			}
			if (trailing != null)
			{
				tempRegex = tempRegex.Trailing(trailing);
			}
			return tempRegex;
		}
		/// <summary>
		/// 扫描字符类并转换为 <see cref="RegexCharClass"/> 对象。
		/// </summary>
		/// <param name="closeBracket">是否要求关闭括号。</param>
		/// <returns><see cref="RegexCharClass"/> 对象。</returns>
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private RegexCharClass ScanCharClass(bool closeBracket)
		{
			RegexCharClass rcc = new RegexCharClass();
			// 是否正在定义字符范围。
			bool inRange = false;
			// 是否是首字符。
			bool firstChar = true;
			// [] 集合是否闭合。
			bool closed = false;
			// 否定的字符类。
			if (reader.Peek() == '^')
			{
				reader.Read();
				rcc.Negate = true;
			}
			int ich = '\0';
			char chPrev = '\0';
			for (; (ich = reader.Read()) >= 0; firstChar = false)
			{
				// 当前字符时候是由转义得到的。
				bool escapedChar = false;
				if (ich == ']')
				{
					// [ 之后的第一个 ] 认为是字符 ]。
					if (!firstChar)
					{
						// 闭合字符类。
						closed = true;
						break;
					}
				}
				else if (ich == '\\' && reader.Peek() >= 0)
				{
					// 判断 '\' 字符的转义。
					ich = ScanBackslash(rcc, inRange);
					if (ich == -1)
					{
						continue;
					}
				}
				if (inRange)
				{
					inRange = false;
					if (ich == '[' && !escapedChar && !firstChar)
					{
						// 本来认为在定义字符范围，但实际在定义一个减去范围。
						// 所以，需要将 chPrev 添加到字符类中，跳过 [ 符号，并递归的扫描新的字符类。
						rcc.AddChar(chPrev);
						rcc.AddSubtraction(ScanCharClass(true));
						if (reader.Peek() >= 0 && reader.Peek() != ']')
						{
							ThrowSubtractionMustBeLast();
						}
					}
					else
					{
						// 规则的字符范围，类似于 a-z。
						if (chPrev > ich)
						{
							ThrowReversedCharRange();
						}
						rcc.AddRange(chPrev, (char)ich);
					}
				}
				else
				{
					if (reader.Peek() == '-')
					{
						reader.Read();
						if (reader.Peek() >= 0 && reader.Peek() != ']')
						{
							// -] 中的，'-' 会按连字符处理。
							// 否则的话，认为是字符范围的起始。
							chPrev = (char)ich;
							inRange = true;
						}
						else
						{
							reader.Unget();
						}
					}
					if (!inRange)
					{
						if (ich == '-' && reader.Peek() == '[' && !escapedChar && !firstChar)
						{
							// 不在字符范围中，并且开始一个减去范围的定义，
							// 一般是由于减去范围紧跟着一个字符范围，例如 [a-z-[b]]。
							reader.Read();
							rcc.AddSubtraction(ScanCharClass(true));
							if (reader.Peek() >= 0 && reader.Peek() != ']')
							{
								ThrowSubtractionMustBeLast();
							}
						}
						else
						{
							rcc.AddRange((char)ich, (char)ich);
						}
					}
				}
			}
			reader.Drop();
			// 模式字符串已到达结尾也按闭合处理。
			if (!closed && closeBracket)
			{
				ThrowUnterminatedBracket();
			}
			if (UseOptionIgnoreCase)
			{
				rcc.AddLowercase(culture);
			}
			return rcc;
		}

		#region 辅助方法

		/// <summary>
		///  扫描普通字符。
		/// </summary>
		/// <returns>得到的普通字符。</returns>
		private string ScanNormalChars()
		{
			reader.Drop();
			int ich;
			// 跳过普通的字符，直到控制字符，或 IgnorePatternWhitespace 状态下的空白。
			if (UseOptionX)
			{
				while (!EndOfPattern &&
					(!IsStopperX(ich = reader.Peek()) || (ich == '{' && !IsTrueQuantifier() && !IsRegexName())))
				{
					reader.Read();
				}
			}
			else
			{
				while (!EndOfPattern &&
					(!IsSpecial(ich = reader.Peek()) || (ich == '{' && !IsTrueQuantifier() && !IsRegexName())))
				{
					reader.Read();
				}
			}
			// 读取的普通字符串。
			return reader.Accept();
		}
		/// <summary>
		/// 扫描数量词。
		/// </summary>
		private void ScanRepeat()
		{
			int ich = reader.Read();
			while (current != null)
			{
				int min = 0;
				int max = 0;
				switch (ich)
				{
					case '*':
						min = 0;
						max = int.MaxValue;
						break;
					case '?':
						min = 0;
						max = 1;
						break;
					case '+':
						min = 1;
						max = int.MaxValue;
						break;
					case '{':
						int startIdx = reader.Index;
						max = min = ScanDecimal();
						if (startIdx < reader.Index)
						{
							if (reader.Peek() == ',')
							{
								reader.Read();
								if (reader.Peek() == -1 || reader.Peek() == '}')
								{
									max = int.MaxValue;
								}
								else
								{
									max = ScanDecimal();
								}
							}
						}
						if (startIdx == reader.Index || reader.Peek() == 0 || reader.Read() != '}')
						{
							AddConcatenate();
							reader.Unget(reader.Index - startIdx + 1);
							return;
						}
						break;
					default:
						Debug.Fail("内部的 ScanRegex 错误");
						break;
				}
				ScanBlank();
				if (min > max)
				{
					ThrowIllegalRange();
				}
				AddConcatenate(min, max);
			}
		}
		/// <summary>
		/// 扫描字符串文本并转换为字符串。
		/// </summary>
		/// <returns>字符串。</returns>
		private string ScanLiteral()
		{
			StringBuilder builder = new StringBuilder();
			int ich;
			while ((ich = reader.Read()) >= 0)
			{
				if (ich == '"')
				{
					// 退出字符串。
					break;
				}
				else if (ich == '\\' && reader.Peek() > 0)
				{
					// 转义字符。
					builder.Append(ScanCharEscape());
				}
				else
				{
					// 普通字符。
					builder.Append((char)ich);
				}
			}
			reader.Drop();
			return builder.ToString();
		}
		/// <summary>
		/// 扫描 '(' 符号之后（不含）的字符，并返回受否作为分组处理。
		/// 如果分组只是简单的更改选项 (?isx-isx) 或者是注释 ($...)，则不作为分组处理。
		/// </summary>
		/// <returns>是否作为分组处理。</returns>
		private bool ScanGroupOpen()
		{
			// 直接作为简单的分组返回：
			// 1. "(" 之后为空。
			// 2. "(x" 但 x != ?
			// 3. "(?)"
			if (reader.Read() != '?' || reader.Peek() == ')')
			{
				reader.Unget();
				return true;
			}
			while (true)
			{
				if (EndOfPattern) { break; }
				ScanOptions();
				if (EndOfPattern) { break; }
				int ich = reader.Read();
				if (ich == ')')
				{
					return false;
				}
				if (ich != ':') { break; }
				return true;
			}
			ThrowUnrecognizedGrouping();
			return false;
		}
		/// <summary>
		/// 扫描 isx-isx 选项字符串，并在第一个未识别的字符位置停止。
		/// </summary>
		private void ScanOptions()
		{
			bool off;
			RegexOptions opt;
			int ich;
			for (off = false; (ich = reader.Peek()) >= 0; reader.Read())
			{
				if (ich == '-')
				{
					off = true;
				}
				else if (ich == '+')
				{
					off = false;
				}
				else
				{
					opt = OptionFromCode(ich);
					if (opt == RegexOptions.None)
					{
						return;
					}
					if (off)
					{
						options &= ~opt;
					}
					else
					{
						options |= opt;
					}
				}
			}
		}
		/// <summary>
		/// 获取单个的 (?isx) 字符的选项位。
		/// </summary>
		/// <param name="ich">要获取选项位的字符。</param>
		/// <returns>字符的选项位。</returns>
		private static RegexOptions OptionFromCode(int ich)
		{
			// 不区分大小写。
			if (ich >= 'A' && ich <= 'Z')
			{
				ich += (char)('a' - 'A');
			}
			switch (ich)
			{
				case 'i':
					return RegexOptions.IgnoreCase;
				case 's':
					return RegexOptions.Singleline;
				case 'x':
					return RegexOptions.IgnorePatternWhiteSpace;
				default:
					return RegexOptions.None;
			}
		}
		/// <summary>
		/// 读入并返回 \p{X} 或 \P{X} 转义符中的 {X}。
		/// </summary>
		/// <returns>读入的 X。</returns>
		private string ScanProperty()
		{
			int ich = reader.Read();
			if (ich == -1)
			{
				ThrowIncompleteSlashP();
			}
			else if (ich != '{')
			{
				ThrowMalformedSlashP();
			}
			reader.Drop();
			while ((ich = reader.Read()) != -1)
			{
				if (!(RegexCharClass.IsWordChar((char)ich) || ich == '-'))
				{
					reader.Unget();
					break;
				}
			}
			string capname = reader.Accept();
			if (capname.Length == 0 || reader.Read() != '}')
			{
				ThrowIncompleteSlashP();
			}
			return capname;
		}
		/// <summary>
		/// 跳过所有空白。
		/// </summary>
		private void ScanBlank()
		{
			int ich;
			if (this.UseOptionP)
			{
				while (true)
				{
					ich = reader.Peek();
					if (ich == -1 || IsSpace(ich))
					{
						return;
					}
					int index = reader.Index;
					if (reader.Read() != '(' || reader.Read() != '?' || reader.Read() != '#')
					{
						reader.Unget(reader.Index - index);
						return;
					}
					while ((ich = reader.Peek()) >= 0 && ich != ')')
					{
						reader.Read();
					}
					if (ich == -1)
					{
						ThrowUnterminatedComment();
					}
					reader.Read();
				}
			}
			else if (this.UseOptionX)
			{
				while (true)
				{
					while (IsSpace(reader.Peek()))
					{
						reader.Read();
					}
					if (reader.Peek() == -1)
					{
						return;
					}
					int index = reader.Index;
					if (reader.Read() != '(' || reader.Read() != '?' || reader.Read() != '#')
					{
						reader.Unget(reader.Index - index);
						return;
					}
					while ((ich = reader.Peek()) >= 0 && ich != ')')
					{
						reader.Read();
					}
					if (ich == -1)
					{
						ThrowUnterminatedComment();
					}
					reader.Read();
				}
			}
			else
			{
				while (true)
				{
					// 内联注释 (?#注释体)。
					int index = reader.Index;
					if (reader.Read() != '(' || reader.Read() != '?' || reader.Read() != '#')
					{
						reader.Unget(reader.Index - index);
						return;
					}
					while ((ich = reader.Peek()) >= 0 && ich != ')')
					{
						reader.Read();
					}
					if (ich == -1)
					{
						ThrowUnterminatedComment();
					}
					reader.Read();
				}
			}
		}
		/// <summary>
		/// 扫描 '\'（不包含 '\' 本身）之后的字符，并返回相应的字符。
		/// </summary>
		/// <param name="rcc">要添加到的字符类。</param>
		/// <param name="inRange">当前是否正在定义字符范围。</param>
		/// <returns>相应的字符，如果不是字符，则为 <c>-1</c>。</returns>
		private int ScanBackslash(RegexCharClass rcc, bool inRange)
		{
			// 判断 '\' 字符的转义。
			int ich = reader.Read();
			switch (ich)
			{
				case 'D':
				case 'd':
					if (inRange)
					{
						ThrowBadClassInCharRange((char)ich);
					}
					rcc.AddDigit(UseOptionEcma, ich == 'D', pattern);
					return -1;
				case 'S':
				case 's':
					if (inRange)
					{
						ThrowBadClassInCharRange((char)ich);
					}
					rcc.AddSpace(UseOptionEcma, ich == 'S');
					return -1;
				case 'W':
				case 'w':
					if (inRange)
					{
						ThrowBadClassInCharRange((char)ich);
					}
					rcc.AddWord(UseOptionEcma, ich == 'W');
					return -1;
				case 'p':
				case 'P':
					if (inRange)
					{
						ThrowBadClassInCharRange((char)ich);
					}
					rcc.AddCategoryFromName(ScanProperty(), (ich != 'p'), UseOptionIgnoreCase, pattern);
					return -1;
				default:
					reader.Unget();
					// 读入转义字符。
					return ScanCharEscape();
			}
		}
		/// <summary>
		/// 扫描 '\'（不包含 '\' 本身）之后的字符，并返回相应的正则表达式。
		/// </summary>
		/// <returns>相应的正则表达式。</returns>
		private Regex ScanBackslash()
		{
			RegexCharClass cc;
			int ich;
			switch (ich = reader.Read())
			{
				case 'w':
					return Regex.CharClass(
						UseOptionEcma ? RegexCharClass.EcmaWordClass : RegexCharClass.WordClass);
				case 'W':
					return Regex.CharClass(
						UseOptionEcma ? RegexCharClass.NotEcmaWordClass : RegexCharClass.NotWordClass);
				case 's':
					return Regex.CharClass(
						UseOptionEcma ? RegexCharClass.EcmaSpaceClass : RegexCharClass.SpaceClass);
				case 'S':
					return Regex.CharClass(
						UseOptionEcma ? RegexCharClass.NotEcmaSpaceClass : RegexCharClass.NotSpaceClass);
				case 'd':
					return Regex.CharClass(
						UseOptionEcma ? RegexCharClass.EcmaDigitClass : RegexCharClass.DigitClass);
				case 'D':
					return Regex.CharClass(
						UseOptionEcma ? RegexCharClass.NotEcmaDigitClass : RegexCharClass.NotDigitClass);
				case 'p':
				case 'P':
					cc = new RegexCharClass();
					cc.AddCategoryFromName(ScanProperty(), ich != 'p', UseOptionIgnoreCase, pattern);
					if (UseOptionIgnoreCase)
					{
						cc.AddLowercase(culture);
					}
					return Regex.CharClass(cc.ToStringClass());
				case -1:
					ThrowIllegalEndEscape();
					return null;
				default:
					reader.Unget();
					char ch = ScanCharEscape();
					if (UseOptionIgnoreCase)
					{
						return Regex.SymbolIgnoreCase(ch, culture);
					}
					else
					{
						return Regex.Symbol(ch);
					}
			}
		}
		/// <summary>
		/// 读入一个捕获名称。
		/// </summary>
		/// <returns>读入的捕获名称。</returns>
		private string ScanCapname()
		{
			reader.Drop();
			int ich;
			while ((ich = reader.Read()) > 0)
			{
				if (!RegexCharClass.IsWordChar((char)ich))
				{
					reader.Unget();
					break;
				}
			}
			return reader.Accept();
		}
		/// <summary>
		/// 读入并返回 \ 转义符表示的单个字符。
		/// </summary>
		/// <returns>转义符表示的单个字符。</returns>
		private char ScanCharEscape()
		{
			int ich = reader.Read();
			if (ich >= '0' && ich <= '7')
			{
				// 八进制。
				reader.Unget();
				return ScanOctal();
			}
			switch (ich)
			{
				case 'x':
					return ScanHex(2);
				case 'u':
					return ScanHex(4);
				case 'a':
					return '\u0007';
				case 'b':
					return '\b';
				case 'e':
					return '\u001B';
				case 'f':
					return '\f';
				case 'n':
					return '\n';
				case 'r':
					return '\r';
				case 't':
					return '\t';
				case 'v':
					return '\u000B';
				case 'c':
					return ScanControl();
				default:
					char ch = (char)ich;
					if (!UseOptionEcma && RegexCharClass.IsWordChar(ch))
					{
						ThrowUnrecognizedEscape(ch);
					}
					return ch;
			}
		}
		/// <summary>
		/// 读入并返回三位八进制数。
		/// </summary>
		/// <returns>读入的八进制数的值。</returns>
		private char ScanOctal()
		{
			int i, d;
			// 八进制数最多有 3 个字符（例如 377）
			int c = 3;
			for (i = 0; c > 0 && (uint)(d = (reader.Peek() - '0')) <= 7; c--)
			{
				reader.Read();
				i *= 8;
				i += d;
				if (UseOptionEcma && i >= 0x20)
				{
					break;
				}
			}
			// 八进制数最大为 255，高位将被除去。
			i &= 0xFF;
			return (char)i;
		}
		/// <summary>
		/// 读入并返回十进制数。
		/// </summary>
		/// <returns>读入的十进制数的值。</returns>
		private int ScanDecimal()
		{
			int i = 0;
			int d;
			while ((uint)(d = (char)(reader.Peek() - '0')) <= 9)
			{
				reader.Read();
				if (i > MaxValueDiv10 || (i == MaxValueDiv10 && d > MaxValueMod10))
				{
					throw ExceptionHelper.OverflowInt32();
				}
				i *= 10;
				i += d;
			}
			return i;
		}
		/// <summary>
		/// 读入并返回 <paramref name="c"/> 个十六进制数字。
		/// </summary>
		/// <param name="c">要读入的十六进制数字的个数。</param>
		/// <returns>读取的十六进制数字的值。</returns>
		private char ScanHex(int c)
		{
			int i = 0, d;
			// 将字符转换为数字。
			for (; c > 0 && ((d = HexDigit(reader.Read())) >= 0); c--)
			{
				i *= 0x10;
				i += d;
			}
			if (c > 0)
			{
				ThrowTooFewHex();
			}
			reader.Drop();
			return (char)i;
		}
		/// <summary>
		/// 将十六进制字符转换为相应的数值。
		/// </summary>
		/// <param name="ch">十六进制字符。</param>
		/// <returns>相应的数字，不存在则返回 <c>-1</c>。</returns>
		private static int HexDigit(int ch)
		{
			if (ch < 0) { return ch; }
			int d;
			if ((uint)(d = ch - '0') <= 9)
			{
				return d;
			}
			if ((uint)(d = ch - 'a') <= 5)
			{
				return d + 0xA;
			}
			if ((uint)(d = ch - 'A') <= 5)
			{
				return d + 0xA;
			}
			return -1;
		}
		/// <summary>
		/// 读入并返回 ASCII 控制字符。
		/// </summary>
		/// <returns>读取的 ASCII 控制字符。</returns>
		private char ScanControl()
		{
			int ich = reader.Read();
			if (ich == -1)
			{
				ThrowMissingControl();
			}
			// \ca 解释为 \cA。
			if (ich >= 'a' && ich <= 'z')
			{
				// 小写字母转为大小字母。
				ich -= 'a' - 'A';
			}
			// ch 对应的是控制字符。
			if ((ich -= '@') < ' ')
			{
				return (char)ich;
			}
			ThrowUnrecognizedControl();
			return '\0';
		}

		#endregion // 辅助方法

		#region 生成正则表达式

		/// <summary>
		/// 结束当前的连接（例如 | 符号）。
		/// </summary>
		private void AddAlternate()
		{
			int cnt = concatenate.Count;
			if (cnt > 0)
			{
				Regex regex = concatenate[0];
				for (int i = 1; i < cnt; i++)
				{
					regex = regex.Concat(concatenate[i]);
				}
				alternation.Add(regex);
				concatenate.Clear();
			}
		}
		/// <summary>
		/// 结束当前的正则表达式单元（没有找到数量词）。
		/// </summary>
		private void AddConcatenate()
		{
			if (current != null)
			{
				concatenate.Add(current);
				current = null;
			}
		}
		/// <summary>
		/// 结束当前的正则表达式单元（找到数量词）。
		/// </summary>
		/// <param name="min">最小重复次数。</param>
		/// <param name="max">最大重复次数。</param>
		private void AddConcatenate(int min, int max)
		{
			Debug.Assert(current != null);
			concatenate.Add(current.Repeat(min, max));
			current = null;
		}
		/// <summary>
		/// 将字符串添加到正则表达式的连接中。
		/// </summary>
		/// <param name="str">要添加的字符串。</param>
		private void AddLiteral(string str)
		{
			Regex regex = null;
			if (str.Length > 1)
			{
				if (UseOptionIgnoreCase)
				{
					regex = Regex.LiteralIgnoreCase(str, culture);
				}
				else
				{
					regex = Regex.Literal(str);
				}
			}
			else
			{
				if (UseOptionIgnoreCase)
				{
					regex = Regex.SymbolIgnoreCase(str[0], culture);
				}
				else
				{
					regex = Regex.Symbol(str[0]);
				}
			}
			concatenate.Add(regex);
		}
		/// <summary>
		/// 设置当前的正则表达式为一个单独的字符。
		/// </summary>
		/// <param name="ch">正则表达式包含的字符。</param>
		private void AddSymbol(char ch)
		{
			if (UseOptionIgnoreCase)
			{
				current = Regex.SymbolIgnoreCase(ch, culture);
			}
			else
			{
				current = Regex.Symbol(ch);
			}
		}
		/// <summary>
		/// 设置当前的正则表达式为一个字符类。
		/// </summary>
		/// <param name="cc">正则表达式包含的字符类。</param>
		private void AddCharClass(string cc)
		{
			current = Regex.CharClass(cc);
		}
		/// <summary>
		/// 设置当前的正则表达式。
		/// </summary>
		/// <param name="regex">要设置的正则表达式。</param>
		private void AddRegex(Regex regex)
		{
			current = regex;
		}
		/// <summary>
		/// 结束当前的组（例如 ) 或到达模式结尾）。
		/// </summary>
		private void AddGroup()
		{
			AddAlternate();
			int cnt = alternation.Count;
			if (cnt > 0)
			{
				Regex regex = alternation[0];
				for (int i = 1; i < cnt; i++)
				{
					regex = regex.Union(alternation[i]);
				}
				alternation.Clear();
				current = regex;
			}
			else
			{
				current = null;
			}
		}
		/// <summary>
		/// 将当前的组添加到堆栈中。
		/// </summary>
		private void PushGroup()
		{
			groupStack.Push(new Tuple<List<Regex>, List<Regex>>(alternation, concatenate));
			alternation = new List<Regex>();
			concatenate = new List<Regex>();
			PushOptions();
		}
		/// <summary>
		/// 弹出堆栈中的组（例如 ')'）。
		/// </summary>
		private void PopGroup()
		{
			Tuple<List<Regex>, List<Regex>> group = groupStack.Pop();
			alternation = group.Item1;
			concatenate = group.Item2;
			PopOptions();
		}
		/// <summary>
		/// 返回分组堆栈是否为空。
		/// </summary>
		private bool EmptyStack
		{
			get { return groupStack.Count == 0; }
		}
		/// <summary>
		/// 返回时候到达了模式的结尾。
		/// </summary>
		private bool EndOfPattern
		{
			get
			{
				int ich = reader.Peek();
				if (ich == -1)
				{
					return true;
				}
				// 判断是否以空白作为模式的结束。
				if ((options & RegexOptions.EndPatternByWhiteSpace) == RegexOptions.EndPatternByWhiteSpace &&
					EmptyStack && IsSpace(ich))
				{
					return true;
				}
				return false;
			}
		}

		#endregion // 生成正则表达式

		#region 正则表达式选项

		/// <summary>
		/// 如果不区分大小写，则返回 <c>true</c>；否则返回 <c>false</c>。
		/// </summary>
		/// <value>是否区不分大小写。</value>
		private bool UseOptionIgnoreCase
		{
			get { return (options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase; }
		}
		/// <summary>
		/// 如果指定单行模式，则返回 <c>true</c>；否则返回 <c>false</c>。
		/// </summary>
		/// <value>是否消除标记中的非转移空白。</value>
		private bool UseOptionSingleLine
		{
			get { return (options & RegexOptions.Singleline) == RegexOptions.Singleline; }
		}
		/// <summary>
		/// 如果消除标记中的非转义空白，则返回 <c>true</c>；否则返回 <c>false</c>。
		/// </summary>
		/// <value>是否消除标记中的非转移空白。</value>
		private bool UseOptionX
		{
			get
			{
				return (options & RegexOptions.IgnorePatternWhiteSpace) == RegexOptions.IgnorePatternWhiteSpace;
			}
		}
		/// <summary>
		/// 如果遇到非转义空白就结束模式，则返回 <c>true</c>；否则返回 <c>false</c>。
		/// </summary>
		/// <value>是否以非转移空白结束模式。</value>
		private bool UseOptionP
		{
			get
			{
				return (options & RegexOptions.EndPatternByWhiteSpace) == RegexOptions.EndPatternByWhiteSpace;
			}
		}
		/// <summary>
		/// 如果为表达式启用符合 ECMAScript 的行为，则返回 <c>true</c>；否则返回 <c>false</c>。
		/// </summary>
		/// <value>是否为表达式启用符合 ECMAScript 的行为。</value>
		private bool UseOptionEcma
		{
			get { return (options & RegexOptions.EcmaScript) == RegexOptions.EcmaScript; }
		}
		/// <summary>
		/// 在堆栈中保存当前的正则表达式选项。
		/// </summary>
		private void PushOptions()
		{
			optionsStack.Push(options);
		}
		/// <summary>
		/// 弹出栈顶的正则表达式选项，并设置为当前的选项。
		/// </summary>
		private void PopOptions()
		{
			options = optionsStack.Pop();
		}
		/// <summary>
		/// 弹出栈顶的正则表达式选项，但保持当前的选项不变。
		/// </summary>
		private void PopKeepOptions()
		{
			optionsStack.Pop();
		}

		#endregion // 正则表达式选项

		#region 字符类型

		/// <summary>
		/// 表示数量的类型。
		/// </summary>
		private const byte Q = 5;
		/// <summary>
		/// 普通的正则表达式符号。
		/// </summary>
		private const byte S = 4;
		/// <summary>
		/// 空白扫描的符号。
		/// </summary>
		private const byte Z = 3;
		/// <summary>
		/// 空白。
		/// </summary>
		private const byte X = 2;
		/// <summary>
		/// 应该被转义。
		/// </summary>
		private const byte E = 1;
		/// <summary>
		/// ASCII 字符的类型。
		/// </summary>
		private static readonly byte[] category = new byte[] {
		//  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F 
			0, 0, 0, 0, 0, 0, 0, 0, 0, X, X, 0, X, X, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		//     !  "  #  $  %  &  '  (  )  *  +  ,  -  .  /  0  1  2  3  4  5  6  7  8  9  :  ;  <  =  >  ?  
			X, 0, S, Z, S, 0, 0, 0, S, S, Q, Q, 0, 0, S, S, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, Q, 
		//  @  A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P  Q  R  S  T  U  V  W  X  Y  Z  [  \  ]  ^  _
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, S, S, 0, 0, 0, 
		//  '  a  b  c  d  e  f  g  h  i  j  k  l  m  n  o  p  q  r  s  t  u  v  w  x  y  z  {  |  }  ~  
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, Q, S, 0, 0, 0};
		/// <summary>
		/// 返回指定的字符是否是正则表达式的特殊字符。
		/// </summary>
		/// <param name="ich">要判断的字符。</param>
		/// <returns>如果字符是正则表达式的特殊字符，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private static bool IsSpecial(int ich)
		{
			return (ich >= 0 && ich <= '|' && category[ich] >= S);
		}
		/// <summary>
		/// 返回指定的字符是否是正则表达式的普通字符。
		/// </summary>
		/// <param name="ich">要判断的字符。</param>
		/// <returns>如果字符是正则表达式的普通字符，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private static bool IsStopperX(int ich)
		{
			return (ich >= 0 && ich <= '|' && category[ich] >= X);
		}
		/// <summary>
		/// 返回指定的字符是否是正则表达式数量的起始字符。
		/// </summary>
		/// <param name="ich">要判断的字符。</param>
		/// <returns>如果字符是正则表达式数量的起始字符，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private static bool IsQuantifier(int ich)
		{
			return (ich >= 0 && ich <= '{' && category[ich] >= Q);
		}
		/// <summary>
		/// 返回之后的字符是否是正则表达式数量字符。
		/// </summary>
		/// <returns>如果是正则表达式数量字符，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private bool IsTrueQuantifier()
		{
			int startIdx = reader.Index;
			int ich = reader.Read();
			bool result = false;
			if (ich == '{')
			{
				// 跳过数字。
				while ((ich = reader.Read()) >= '0' && ich <= '9') ;
				if (ich == -1 || reader.Index == startIdx + 2)
				{
					result = false;
				}
				else if (ich == '}')
				{
					// {n}
					result = true;
				}
				else if (ich != ',')
				{
					result = false;
				}
				else
				{
					// 跳过数字。
					while ((ich = reader.Read()) >= '0' && ich <= '9') ;
					// {n,m}
					result = (ich == '}');
				}
			}
			else
			{
				result = IsQuantifier(ich);
			}
			reader.Unget(reader.Index - startIdx);
			return result;
		}
		/// <summary>
		/// 返回之后的字符是否是 {name}。
		/// </summary>
		/// <returns>如果是 {name}，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private bool IsRegexName()
		{
			int startIdx = reader.Index;
			int ich = reader.Read();
			if (ich == '{')
			{
				// 跳过单词。
				while ((ich = reader.Read()) >= 0 && RegexCharClass.IsWordChar((char)ich)) ;
			}
			int cnt = reader.Index - startIdx;
			reader.Unget(cnt);
			return (ich == '}' && cnt > 2);
		}
		/// <summary>
		/// 返回指定的字符是否是空白字符。
		/// </summary>
		/// <param name="ch">要判断的字符。</param>
		/// <returns>如果字符是空白字符，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private static bool IsSpace(int ch)
		{
			return (ch >= 0 && ch <= ' ' && category[ch] == X);
		}

		#endregion // 字符类型

		#region 异常信息

		/// <summary>
		/// 抛出不能在字符范围中包括类的异常。
		/// </summary>
		/// <param name="charClass">出现异常的的字符类。</param>
		private void ThrowBadClassInCharRange(char charClass)
		{
			ThrowParsingException("BadClassInCharRange", charClass);
		}
		/// <summary>
		/// 抛出非法的结尾转义的异常。
		/// </summary>
		private void ThrowIllegalEndEscape()
		{
			ThrowParsingException("IllegalEndEscape");
		}
		/// <summary>
		/// 抛出非法的范围的异常。
		/// </summary>
		private void ThrowIllegalRange()
		{
			ThrowParsingException("IllegalRange");
		}
		/// <summary>
		/// 抛出不完整正则表达式引用的异常。
		/// </summary>
		private void ThrowIncompleteRegexReference()
		{
			ThrowParsingException("IncompleteRegexReference");
		}
		/// <summary>
		/// 抛出不完整的 \p{X} 字符转义的异常。
		/// </summary>
		private void ThrowIncompleteSlashP()
		{
			ThrowParsingException("IncompleteSlashP");
		}
		/// <summary>
		/// 抛出格式不正确的 \p{X} 字符转义的异常。
		/// </summary>
		private void ThrowMalformedSlashP()
		{
			ThrowParsingException("MalformedSlashP");
		}
		/// <summary>
		/// 抛出缺少控制字符的异常。
		/// </summary>
		private void ThrowMissingControl()
		{
			ThrowParsingException("MissingControl");
		}
		/// <summary>
		/// 抛出嵌套的数量词的异常。
		/// </summary>
		/// <param name="quantify">被嵌套的数量词。</param>
		private void ThrowNestedQuantify(string quantify)
		{
			ThrowParsingException("NestedQuantify", quantify);
		}
		/// <summary>
		/// 抛出嵌套的向前看的异常。
		/// </summary>
		private void ThrowNestedTrailing()
		{
			ThrowParsingException("NestedTrailing");
		}
		/// <summary>
		/// 抛出右括号不足的异常。
		/// </summary>
		private void ThrowNotEnoughParens()
		{
			ThrowParsingException("NotEnoughParens");
		}
		/// <summary>
		/// 抛出数量词之前没有字符的异常。
		/// </summary>
		private void ThrowQuantifyAfterNothing()
		{
			ThrowParsingException("QuantifyAfterNothing");
		}
		/// <summary>
		/// 抛出字符范围顺序相反的异常。
		/// </summary>
		private void ThrowReversedCharRange()
		{
			ThrowParsingException("ReversedCharRange");
		}
		/// <summary>
		/// 抛出排除的字符范围错误的异常。
		/// </summary>
		private void ThrowSubtractionMustBeLast()
		{
			ThrowParsingException("SubtractionMustBeLast");
		}
		/// <summary>
		/// 抛出十六进制数字位数不足的异常。
		/// </summary>
		private void ThrowTooFewHex()
		{
			ThrowParsingException("TooFewHex");
		}
		/// <summary>
		/// 抛出太多的右括号的异常。
		/// </summary>
		private void ThrowTooManyParens()
		{
			ThrowParsingException("TooManyParens");
		}
		/// <summary>
		/// 抛出未定义的正则表达式的异常。
		/// </summary>
		/// <param name="name">未定义的正则表达式的名字。</param>
		/// <param name="start">异常的起始位置。</param>
		/// <param name="end">异常的结束位置。</param>
		private void ThrowUndefinedRegex(string name, SourceLocation start, SourceLocation end)
		{
			string message = ExceptionResources.GetString("UndefinedRegex", name);
			throw CompilerExceptionHelper.ParsingException(pattern, message, start, end);
		}
		/// <summary>
		/// 抛出未识别的控制字符的异常。
		/// </summary>
		private void ThrowUnrecognizedControl()
		{
			ThrowParsingException("UnrecognizedControl");
		}
		/// <summary>
		/// 抛出无法识别的转义序列的异常。
		/// </summary>
		/// <param name="ch">无法被识别的转义序列。</param>
		private void ThrowUnrecognizedEscape(char ch)
		{
			ThrowParsingException("UnrecognizedEscape", ch);
		}
		/// <summary>
		/// 抛出未识别的分组的异常。
		/// </summary>
		private void ThrowUnrecognizedGrouping()
		{
			ThrowParsingException("UnrecognizedGrouping");
		}
		/// <summary>
		/// 抛出未终止的 [] 集合的异常。
		/// </summary>
		private void ThrowUnterminatedBracket()
		{
			ThrowParsingException("UnterminatedBracket");
		}
		/// <summary>
		/// 抛出未终止的内联注释的异常。
		/// </summary>
		private void ThrowUnterminatedComment()
		{
			ThrowParsingException("UnterminatedComment");
		}
		/// <summary>
		/// 抛出分析异常。
		/// </summary>
		/// <param name="resName">异常信息的资源名称。</param>
		private void ThrowParsingException(string resName)
		{
			reader.Unget();
			reader.Drop();
			string message = ExceptionResources.GetString(resName);
			throw CompilerExceptionHelper.ParsingException(pattern, message, reader.StartLocation, SourceLocation.Invalid);
		}
		/// <summary>
		/// 抛出分析异常。
		/// </summary>
		/// <param name="resName">异常信息的资源名称。</param>
		/// <param name="args">异常的格式化信息。</param>
		private void ThrowParsingException(string resName, params object[] args)
		{
			reader.Unget();
			reader.Drop();
			string message = ExceptionResources.GetString(resName, args);
			throw CompilerExceptionHelper.ParsingException(pattern, message, reader.StartLocation, SourceLocation.Invalid);
		}

		#endregion // 异常信息

	}
}