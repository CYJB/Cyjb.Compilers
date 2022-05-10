using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Cyjb.Compilers.RegularExpressions
{
	/// <summary>
	/// 表示正则表达式的分析器。
	/// </summary>
	internal sealed class RegexParser
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
		/// 正则表达式的模式。
		/// </summary>
		private readonly string pattern;
		/// <summary>
		/// 语言区域性信息。
		/// </summary>
		private readonly CultureInfo culture;
		/// <summary>
		/// 正则表达式的选项。
		/// </summary>
		private RegexOptions options;
		/// <summary>
		/// 正则表达式的定义。
		/// </summary>
		private readonly IDictionary<string, LexRegex> regexDefinition;
		/// <summary>
		/// 即将解析的模式位置。
		/// </summary>
		private int currentPos = 0;
		/// <summary>
		/// 正则表达式的选项堆栈。
		/// </summary>
		private readonly Stack<RegexOptions> optionsStack = new();
		/// <summary>
		/// 当前"或"的正则表达式。
		/// </summary>
		private List<LexRegex> alternation = new();
		/// <summary>
		/// 当前"连接"的正则表达式。
		/// </summary>
		private List<LexRegex> concatenate = new();
		/// <summary>
		/// 正则表达式的组的堆栈。
		/// </summary>
		private readonly Stack<Tuple<List<LexRegex>, List<LexRegex>>> groupStack = new();
		/// <summary>
		/// 当前的正则表达式。
		/// </summary>
		private LexRegex? current;
		/// <summary>
		/// 向前看正则表达式。
		/// </summary>
		private LexRegex? trailing = null;
		/// <summary>
		/// 是否正在处理向前看正则表达式。
		/// </summary>
		private bool inTrailing = false;

		/// <summary>
		/// 根据给定的源文件解析正则表达式。
		/// </summary>
		/// <param name="pattern">正则表达式的模式。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		/// <returns>解析得到的正则表达式。</returns>
		internal static LexRegex ParseRegex(string pattern, RegexOptions option, IDictionary<string, LexRegex>? regexDef)
		{
			RegexParser parser = new(pattern, option, regexDef);
			return parser.StartScanRegex();
		}

		/// <summary>
		/// 根据给定的源文件解析正则表达式的字符类。
		/// </summary>
		/// <param name="pattern">正则表达式的模式源文件。</param>
		/// <param name="option">正则表达式的选项。</param>
		/// <param name="closeBracket">是否要求关闭括号。</param>
		/// <returns>解析得到的正则表达式的字符类。</returns>
		internal static RegexCharClass ParseCharClass(string pattern, RegexOptions option, bool closeBracket)
		{
			RegexParser parser = new(pattern, option, null);
			return parser.ScanCharClass(closeBracket);
		}

		/// <summary>
		/// 使用指定的模式、选项和定义初始化 <see cref="RegexParser"/> 类的新实例。
		/// </summary>
		/// <param name="pattern">正则表达式的模式。</param>
		/// <param name="options">正则表达式的选项。</param>
		/// <param name="regexDef">正则表达式的定义。</param>
		private RegexParser(string pattern, RegexOptions options, IDictionary<string, LexRegex>? regexDef)
		{
			this.pattern = pattern;
			this.options = options;
			culture = options.HasFlag(RegexOptions.CultureInvariant) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;
			if (regexDef == null)
			{
				regexDefinition = new Dictionary<string, LexRegex>();
			}
			else
			{
				regexDefinition = regexDef;
			}
		}

		/// <summary>
		/// 扫描正则表达式并转换为 <see cref="LexRegex"/> 对象。
		/// </summary>
		/// <returns><see cref="LexRegex"/> 对象。</returns>
		private LexRegex StartScanRegex()
		{
			// <<EOF>> 的特殊判断。
			if (Scan("<<EOF>>"))
			{
				return LexRegex.EndOfFile();
			}
			// ^ 仅在开头表示行起始，否则表示普通的符号。
			bool beginningOfLine = Scan('^');
			LexRegex tempRegex = ScanRegex();
			if (beginningOfLine)
			{
				tempRegex = tempRegex.BeginningOfLine();
			}
			return tempRegex;
		}

		/// <summary>
		/// 扫描正则表达式并转换为 <see cref="LexRegex"/> 对象。
		/// </summary>
		/// <returns><see cref="LexRegex"/> 对象。</returns>
		private LexRegex ScanRegex()
		{
			// 是否需要读取向前看符号。
			bool readTrailing = false;
			char ch;
			// 是否是数量词。
			bool isQuantifier = false;
			while (CharsRight() > 0)
			{
				bool wasPrevQuantifier = isQuantifier;
				isQuantifier = false;
				ScanBlank();
				ReadOnlySpan<char> normalChars = ScanNormalChars();
				ScanBlank();
				if (CharsRight() == 0)
				{
					// 非特殊的字符，表示结束。
					ch = '!';
				}
				else if (IsSpecial(ch = Peek()))
				{
					isQuantifier = IsQuantifier(ch);
					// 排除 {name} 情况。
					if (isQuantifier && ch == '{' && !IsTrueQuantifier())
					{
						isQuantifier = false;
					}
					MoveRight();
				}
				else
				{
					// 非特殊的字符，表示普通的字符。
					ch = ' ';
				}
				if (normalChars.Length > 0)
				{
					wasPrevQuantifier = false;
					// 如果之后是量词的话，量词只针对最后一个字符。
					if (isQuantifier)
					{
						if (normalChars.Length > 1)
						{
							AddLiteral(new string(normalChars[0..^1]));
						}
						AddSymbol(normalChars[^1]);
					}
					else
					{
						AddLiteral(new string(normalChars));
					}
				}
				switch (ch)
				{
					case '!':
						goto BreakOuterScan;
					case ' ':
						goto ContinueOuterScan;
					case '[':
						AddCharClass(ScanCharClass(true));
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
							throw CreateException(RegexParseError.InsufficientOpeningParentheses, Resources.TooManyParens);
						}
						AddGroup();
						PopGroup();
						if (current == null)
						{
							goto ContinueOuterScan;
						}
						break;
					case '\\':
						if (CharsRight() == 0)
						{
							throw CreateException(RegexParseError.UnescapedEndingBackslash, Resources.IllegalEndEscape);
						}
						AddRegex(ScanBackslash());
						break;
					case '/':
						if (inTrailing)
						{
							throw CreateException(RegexParseError.Unknown, Resources.NestedTrailing);
						}
						readTrailing = true;
						goto BreakOuterScan;
					case '$':
						// $ 仅在结尾表示行结尾，否则表示普通的符号。
						// 在 Trailing 中，最后的 $ 不再认为是行结束符。
						if (CharsRight() == 0 && !inTrailing)
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
						AddRegex(LexRegex.AnyChar(IsSingleLine));
						break;
					case '{':
					case '*':
					case '+':
					case '?':
						if (current == null)
						{
							if (ch == '{')
							{
								// {regexName} 情况。
								string name = ScanRegexName();
								if (CharsRight() > 0 && Peek() == '}')
								{
									MoveRight();
									if (!regexDefinition.TryGetValue(name, out current))
									{
										throw CreateException(RegexParseError.Unknown, Resources.UndefinedRegex, name);
									}
								}
								else
								{
									throw CreateException(RegexParseError.Unknown, Resources.IncompleteRegexReference);
								}
							}
							else if (wasPrevQuantifier)
							{
								throw CreateException(RegexParseError.Unknown, Resources.NestedQuantify, ch.ToString());
							}
							else
							{
								throw CreateException(RegexParseError.Unknown, Resources.QuantifyAfterNothing);
							}
						}
						MoveLeft();
						break;
					case '"':
						// 字符串。
						string str = ScanLiteral();
						if (IsIgnoreCase)
						{
							AddRegex(LexRegex.LiteralIgnoreCase(str, culture));
						}
						else
						{
							AddRegex(LexRegex.Literal(str));
						}
						break;
					default:
						Debug.Fail("内部的 ScanRegex 错误");
						break;
				}
				ScanBlank();
				if (CharsRight() == 0 || !(isQuantifier = IsTrueQuantifier()))
				{
					AddConcatenate();
					goto ContinueOuterScan;
				}
				ScanQuantifier();
			ContinueOuterScan:
				;
			}
		BreakOuterScan:
			if (!EmptyStack)
			{
				throw CreateException(RegexParseError.InsufficientClosingParentheses, Resources.NotEnoughParens);
			}
			AddGroup();
			LexRegex tempRegex = current!;
			current = null;
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
		private RegexCharClass ScanCharClass(bool closeBracket)
		{
			RegexCharClass charClass = new();
			// 是否正在定义字符范围。
			bool inRange = false;
			// 是否是首字符。
			bool firstChar = true;
			// [] 集合是否闭合。
			bool closed = false;
			// 否定的字符类。
			if (Scan('^'))
			{
				charClass.SetNegated();
			}
			char chPrev = '\0';
			for (; CharsRight() > 0; firstChar = false)
			{
				char ch = Read();
				// 当前字符时候是由转义得到的。
				bool escapedChar = false;
				if (ch == ']')
				{
					// [ 之后的第一个 ] 认为是字符 ]。
					if (!firstChar)
					{
						// 闭合字符类。
						closed = true;
						break;
					}
				}
				else if (ch == '\\' && CharsRight() > 0)
				{
					// 判断 '\' 字符的转义。
					int result = ScanBackslash(charClass, inRange);
					if (result == -1)
					{
						continue;
					}
					else
					{
						ch = (char)result;
					}
				}
				if (inRange)
				{
					inRange = false;
					if (ch == '[' && !escapedChar && !firstChar)
					{
						// 本来认为在定义字符范围，但实际在定义一个减去范围。
						// 所以，需要将 chPrev 添加到字符类中，跳过 [ 符号，并递归的扫描新的字符类。
						charClass.AddChar(chPrev, IsIgnoreCase, culture);
						charClass.AddSubtraction(ScanCharClass(true));
						if (CharsRight() > 0 && Peek() != ']')
						{
							throw CreateException(RegexParseError.ExclusionGroupNotLast, Resources.SubtractionMustBeLast);
						}
					}
					else
					{
						// 规则的字符范围，类似于 a-z。
						if (chPrev > ch)
						{
							throw CreateException(RegexParseError.ReversedCharacterRange, Resources.ReversedCharRange);
						}
						charClass.AddRange(chPrev, ch, IsIgnoreCase, culture);
					}
				}
				else
				{
					if (Scan('-'))
					{
						if (CharsRight() > 0 && Peek() != ']')
						{
							// -] 中的，'-' 会按连字符处理。
							// 否则的话，认为是字符范围的起始。
							chPrev = ch;
							inRange = true;
						}
						else
						{
							MoveLeft();
						}
					}
					if (!inRange)
					{
						if (ch == '-' && CharsRight() > 0 && Peek() == '[' && !escapedChar && !firstChar)
						{
							// 不在字符范围中，并且开始一个减去范围的定义，
							// 一般是由于减去范围紧跟着一个字符范围，例如 [a-z-[b]]。
							MoveRight();
							charClass.AddSubtraction(ScanCharClass(true));
							if (CharsRight() > 0 && Peek() != ']')
							{
								throw CreateException(RegexParseError.ExclusionGroupNotLast, Resources.SubtractionMustBeLast);
							}
						}
						else
						{
							charClass.AddChar(ch, IsIgnoreCase, culture);
						}
					}
				}
			}
			// 模式字符串已到达结尾也按闭合处理。
			if (!closed && closeBracket)
			{
				throw CreateException(RegexParseError.UnterminatedBracket, Resources.UnterminatedBracket);
			}
			// 解析完毕后统一简化字符类
			charClass.Simplify();
			return charClass;
		}

		#region 辅助方法

		/// <summary>
		/// 返回未解析的剩余字符个数。
		/// </summary>
		private int CharsRight()
		{
			return pattern.Length - currentPos;
		}

		/// <summary>
		/// 读取之后的字符但不使用。
		/// </summary>
		/// <param name="count">要读取的字符位置。</param>
		private char Peek(int count = 0)
		{
			return pattern[currentPos + count];
		}

		/// <summary>
		/// 返回指定位置的字符。
		/// </summary>
		/// <param name="index">要读取的字符位置。</param>
		/// <returns>指定位置的字符。</returns>
		private char CharAt(int index)
		{
			return pattern[index];
		}

		/// <summary>
		/// 左移多个字符。
		/// </summary>
		/// <param name="count">要左移的字符个数。</param>
		private void MoveLeft(int count = 1)
		{
			currentPos -= count;
		}

		/// <summary>
		/// 右移多个字符。
		/// </summary>
		/// <param name="count">要右移的字符个数。</param>
		private void MoveRight(int count = 1)
		{
			currentPos += count;
		}

		/// <summary>
		/// 读取下一字符。
		/// </summary>
		private char Read()
		{
			return pattern[currentPos++];
		}

		/// <summary>
		/// 扫描指定字符。
		/// </summary>
		/// <param name="ch">要扫描的字符。</param>
		/// <returns>是否成功找到指定字符。</returns>
		private bool Scan(char ch)
		{
			if (CharsRight() > 0 && Peek() == ch)
			{
				MoveRight();
				return true;
			}
			return false;
		}

		/// <summary>
		/// 尝试扫描指定字符串。
		/// </summary>
		/// <param name="text">要扫描的字符串。</param>
		/// <returns>是否成功找到指定字符串。</returns>
		private bool Scan(string text)
		{
			if (string.Compare(pattern, currentPos, text, 0, text.Length) == 0)
			{
				MoveRight(text.Length);
				return true;
			}
			return false;
		}

		/// <summary>
		/// 持续扫描直到到找到指定字符。
		/// </summary>
		/// <param name="ch">要找到的字符。</param>
		/// <returns>是否成功找到指定字符。</returns>
		private bool ScanTo(char ch)
		{
			while (CharsRight() > 0 && Peek() != ch)
			{
				MoveRight();
			}
			return CharsRight() > 0;
		}

		/// <summary>
		///  扫描普通字符。
		/// </summary>
		/// <returns>得到的普通字符。</returns>
		private ReadOnlySpan<char> ScanNormalChars()
		{
			int start = currentPos;
			char ch;
			Func<char, bool> checkChar = IsIgnoreWhitespace ? IsStopperX : IsSpecial;
			// 跳过普通的字符，直到控制字符
			while (CharsRight() > 0 &&
				(!checkChar(ch = Peek()) || (ch == '{' && !IsTrueQuantifier() && !IsRegexName())))
			{
				MoveRight();
			}
			return pattern.AsSpan(start, currentPos - start);
		}

		/// <summary>
		/// 扫描数量词。
		/// </summary>
		private void ScanQuantifier()
		{
			char ch = Read();
			while (current != null)
			{
				int min = 0;
				int max = 0;
				switch (ch)
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
						int startIdx = currentPos;
						max = min = ScanDecimal();
						if (startIdx < currentPos && Scan(','))
						{
							if (CharsRight() == 0 || Peek() == '}')
							{
								max = int.MaxValue;
							}
							else
							{
								max = ScanDecimal();
							}
						}
						if (startIdx == currentPos || CharsRight() == 0 || Read() != '}')
						{
							AddConcatenate();
							currentPos = startIdx - 1;
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
					throw CreateException(RegexParseError.ReversedQuantifierRange, Resources.ReversedQuantifierRange);
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
			StringBuilder builder = new();
			char ch;
			while (CharsRight() > 0)
			{
				ch = Read();
				if (ch == '"')
				{
					// 退出字符串。
					break;
				}
				else if (ch == '\\' && CharsRight() > 0)
				{
					// 转义字符。
					builder.Append(ScanCharEscape());
				}
				else
				{
					// 普通字符。
					builder.Append(ch);
				}
			}
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
			if (CharsRight() <= 1 || Peek() != '?' || Peek(1) == ')')
			{
				return true;
			}
			MoveRight();
			while (true)
			{
				if (CharsRight() == 0) { break; }
				ScanOptions();
				if (CharsRight() == 0) { break; }
				char ch = Read();
				if (ch == ')')
				{
					return false;
				}
				if (ch != ':') { break; }
				return true;
			}
			throw CreateException(RegexParseError.InvalidGroupingConstruct, Resources.UnrecognizedGrouping);
		}

		/// <summary>
		/// 扫描 isx-isx 选项字符串，并在第一个未识别的字符位置停止。
		/// </summary>
		private void ScanOptions()
		{
			bool off;
			RegexOptions opt;
			char ch;
			for (off = false; CharsRight() > 0; MoveRight())
			{
				ch = Peek();
				if (ch == '-')
				{
					off = true;
				}
				else if (ch == '+')
				{
					off = false;
				}
				else
				{
					opt = OptionFromCode(ch);
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
		/// <param name="ch">要获取选项位的字符。</param>
		/// <returns>字符的选项位。</returns>
		private static RegexOptions OptionFromCode(char ch)
		{
			// 不区分大小写。
			if (ch >= 'A' && ch <= 'Z')
			{
				ch += (char)('a' - 'A');
			}
			return ch switch
			{
				'i' => RegexOptions.IgnoreCase,
				's' => RegexOptions.Singleline,
				'x' => RegexOptions.IgnorePatternWhitespace,
				_ => RegexOptions.None,
			};
		}

		/// <summary>
		/// 读入并返回 \p{X} 或 \P{X} 转义符中的 {X}。
		/// </summary>
		/// <returns>读入的 X。</returns>
		private string ScanProperty()
		{
			if (CharsRight() == 0)
			{
				throw CreateException(RegexParseError.InvalidUnicodePropertyEscape, Resources.InvalidUnicodePropertyEscape);
			}
			if (!Scan('{'))
			{
				throw CreateException(RegexParseError.MalformedUnicodePropertyEscape, Resources.MalformedUnicodePropertyEscape);
			}
			int start = currentPos;
			char ch;
			while (CharsRight() > 0)
			{
				ch = Read();
				if (!ch.IsWord() && ch != '-')
				{
					MoveLeft();
					break;
				}
			}
			string capname = pattern[start..currentPos];
			if (capname.Length == 0 || !Scan('}'))
			{
				throw CreateException(RegexParseError.InvalidUnicodePropertyEscape, Resources.InvalidUnicodePropertyEscape);
			}
			return capname;
		}

		/// <summary>
		/// 跳过所有空白。
		/// </summary>
		private void ScanBlank()
		{
			bool ignoreWhiteSpace = IsIgnoreWhitespace;
			while (CharsRight() > 0)
			{
				if (ignoreWhiteSpace && IsSpace(Peek()))
				{
					MoveRight();
					continue;
				}
				if (!Scan("(?#"))
				{
					return;
				}
				if (ScanTo(')'))
				{
					// 跳过 )
					MoveRight();
				}
				else
				{
					throw CreateException(RegexParseError.UnterminatedComment, Resources.UnterminatedComment);
				}
			}
		}

		/// <summary>
		/// 扫描 '\'（不包含 '\' 本身）之后的字符，并返回相应的字符。
		/// </summary>
		/// <param name="charClass">要添加到的字符类。</param>
		/// <param name="inRange">当前是否正在定义字符范围。</param>
		/// <returns>相应的字符，如果不是字符，则为 <c>-1</c>。</returns>
		private int ScanBackslash(RegexCharClass charClass, bool inRange)
		{
			// 判断 '\' 字符的转义。
			char ch = Read();
			switch (ch)
			{
				case 'D':
				case 'd':
					if (!inRange)
					{
						charClass.AddDigit(IsECMA, ch == 'D');
						return -1;
					}
					break;
				case 'S':
				case 's':
					if (!inRange)
					{
						charClass.AddSpace(IsECMA, ch == 'S');
						return -1;
					}
					break;
				case 'W':
				case 'w':
					if (!inRange)
					{
						charClass.AddWord(IsECMA, ch == 'W');
						return -1;
					}
					break;
				case 'p':
				case 'P':
					if (!inRange)
					{
						charClass.AddCategoryFromName(ScanProperty(), ch != 'p', IsIgnoreCase, culture);
						return -1;
					}
					break;
				default:
					// 读入转义字符。
					MoveLeft();
					return ScanCharEscape();
			}
			throw CreateException(RegexParseError.ShorthandClassInCharacterRange, Resources.BadClassInCharRange, ch);
		}

		/// <summary>
		/// 扫描 '\'（不包含 '\' 本身）之后的字符，并返回相应的正则表达式。
		/// </summary>
		/// <returns>相应的正则表达式。</returns>
		private LexRegex ScanBackslash()
		{
			char ch;
			switch (ch = Read())
			{
				case 'w':
					return IsECMA ? CharClassExp.ECMAWordClassExp : CharClassExp.WordClassExp;
				case 'W':
					return IsECMA ? CharClassExp.ECMANotWordClassExp : CharClassExp.NotWordClassExp;
				case 's':
					return IsECMA ? CharClassExp.ECMASpaceClassExp : CharClassExp.SpaceClassExp;
				case 'S':
					return IsECMA ? CharClassExp.ECMANotSpaceClassExp : CharClassExp.NotSpaceClassExp;
				case 'd':
					return IsECMA ? CharClassExp.ECMADigitClassExp : CharClassExp.DigitClassExp;
				case 'D':
					return IsECMA ? CharClassExp.ECMANotDigitClassExp : CharClassExp.NotDigitClassExp;
				case 'p':
				case 'P':
					RegexCharClass charClass = new();
					charClass.AddCategoryFromName(ScanProperty(), ch != 'p', IsIgnoreCase, culture);
					return new CharClassExp(charClass);
				default:
					MoveLeft();
					ch = ScanCharEscape();
					if (IsIgnoreCase)
					{
						return LexRegex.SymbolIgnoreCase(ch, culture);
					}
					else
					{
						return LexRegex.Symbol(ch);
					}
			}
		}

		/// <summary>
		/// 扫描正则表达式的名称。
		/// </summary>
		/// <returns>正则表达式的名称。</returns>
		private string ScanRegexName()
		{
			int start = currentPos;
			while (CharsRight() > 0)
			{
				if (Peek().IsWord())
				{
					MoveRight();
				}
				else
				{
					break;
				}
			}
			return pattern[start..currentPos];
		}

		/// <summary>
		/// 读入并返回 \ 转义符表示的单个字符。
		/// </summary>
		/// <returns>转义符表示的单个字符。</returns>
		private char ScanCharEscape()
		{
			char ch = Read();
			if (ch >= '0' && ch <= '7')
			{
				// 八进制。
				MoveLeft();
				return ScanOctal();
			}
			switch (ch)
			{
				case 'x':
					return ScanHex(2);
				case 'u':
					return ScanHex(4);
				case 'a':
					return '\x07';
				case 'b':
					return '\x08';
				case 'e':
					return '\x1B';
				case 'f':
					return '\x0C';
				case 'n':
					return '\x0A';
				case 'r':
					return '\x0D';
				case 't':
					return '\x09';
				case 'v':
					return '\x0B';
				case 'c':
					return ScanControl();
				default:
					if (!IsECMA && ch.IsWord())
					{
						throw CreateException(RegexParseError.UnrecognizedEscape, Resources.UnrecognizedEscape, ch);
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
			int result, value;
			// 八进制数最多有 3 个字符（例如 377）
			int c = 3;
			for (result = 0; c > 0 && CharsRight() > 0 && (value = Peek().GetBaseValue(8)) >= 0; c--)
			{
				MoveRight();
				result *= 8;
				result += value;
				if (IsECMA && result >= 0x20)
				{
					break;
				}
			}
			// 八进制数最大为 255，高位将被除去。
			result &= 0xFF;
			return (char)result;
		}

		/// <summary>
		/// 读入并返回十进制数。
		/// </summary>
		/// <returns>读入的十进制数的值。</returns>
		private int ScanDecimal()
		{
			int result = 0;
			int value;
			while (CharsRight() > 0 && (value = Peek().GetBaseValue(10)) >= 0)
			{
				MoveRight();
				if (result > MaxValueDiv10 || (result == MaxValueDiv10 && value > MaxValueMod10))
				{
					throw CreateException(RegexParseError.QuantifierOrCaptureGroupOutOfRange, Resources.QuantifierOutOfRange);
				}
				result *= 10;
				result += value;
			}
			return result;
		}

		/// <summary>
		/// 读入并返回 <paramref name="count"/> 个十六进制数字。
		/// </summary>
		/// <param name="count">要读入的十六进制数字的个数。</param>
		/// <returns>读取的十六进制数字的值。</returns>
		private char ScanHex(int count)
		{
			int result = 0;
			// 将字符转换为数字。
			for (; count > 0 && CharsRight() > 0; count--)
			{
				int value = Read().GetBaseValue(16);
				if (value < 0)
				{
					throw CreateException(RegexParseError.InsufficientOrInvalidHexDigits, Resources.TooFewHex);
				}
				result *= 0x10;
				result += value;
			}
			return (char)result;
		}

		/// <summary>
		/// 读入并返回 ASCII 控制字符。
		/// </summary>
		/// <returns>读取的 ASCII 控制字符。</returns>
		private char ScanControl()
		{
			if (CharsRight() <= 0)
			{
				throw CreateException(RegexParseError.MissingControlCharacter, Resources.MissingControl);
			}
			char ch = Read();
			// \ca 解释为 \cA。
			if (ch >= 'a' && ch <= 'z')
			{
				// 小写字母转为大小字母。
				ch -= (char)('a' - 'A');
			}
			// ch 对应的是控制字符。
			if ((ch -= '@') >= ' ')
			{
				throw CreateException(RegexParseError.UnrecognizedControlCharacter, Resources.UnrecognizedControl);
			}
			return ch;
		}

		#endregion // 辅助方法

		#region 生成正则表达式

		/// <summary>
		/// 结束当前的连接（例如 | 符号）。
		/// </summary>
		private void AddAlternate()
		{
			if (concatenate.Count > 0)
			{
				alternation.Add(LexRegex.Concat(concatenate.ToArray()));
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
			LexRegex regex;
			if (IsIgnoreCase)
			{
				regex = LexRegex.LiteralIgnoreCase(str, culture);
			}
			else
			{
				regex = LexRegex.Literal(str);
			}
			concatenate.Add(regex);
		}

		/// <summary>
		/// 设置当前的正则表达式为一个单独的字符。
		/// </summary>
		/// <param name="ch">正则表达式包含的字符。</param>
		private void AddSymbol(char ch)
		{
			if (IsIgnoreCase)
			{
				current = LexRegex.SymbolIgnoreCase(ch, culture);
			}
			else
			{
				current = LexRegex.Symbol(ch);
			}
		}

		/// <summary>
		/// 设置当前的正则表达式为一个字符类。
		/// </summary>
		/// <param name="charClass">正则表达式包含的字符类。</param>
		private void AddCharClass(RegexCharClass charClass)
		{
			current = new CharClassExp(charClass);
		}

		/// <summary>
		/// 设置当前的正则表达式。
		/// </summary>
		/// <param name="regex">要设置的正则表达式。</param>
		private void AddRegex(LexRegex regex)
		{
			current = regex;
		}

		/// <summary>
		/// 结束当前的组（例如 ) 或到达模式结尾）。
		/// </summary>
		private void AddGroup()
		{
			AddAlternate();
			if (alternation.Count > 0)
			{
				current = LexRegex.Alternate(alternation.ToArray());
				alternation.Clear();
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
			groupStack.Push(new Tuple<List<LexRegex>, List<LexRegex>>(alternation, concatenate));
			alternation = new List<LexRegex>();
			concatenate = new List<LexRegex>();
		}

		/// <summary>
		/// 弹出堆栈中的组（例如 ')'）。
		/// </summary>
		private void PopGroup()
		{
			Tuple<List<LexRegex>, List<LexRegex>> group = groupStack.Pop();
			alternation = group.Item1;
			concatenate = group.Item2;
			PopOptions();
		}

		/// <summary>
		/// 返回分组堆栈是否为空。
		/// </summary>
		private bool EmptyStack => groupStack.Count == 0;

		#endregion // 生成正则表达式

		#region 正则表达式选项

		/// <summary>
		/// 获取是否不区分大小写。
		/// </summary>
		/// <value>是否区不分大小写。</value>
		private bool IsIgnoreCase => options.HasFlag(RegexOptions.IgnoreCase);

		/// <summary>
		/// 获取是否单行模式。
		/// </summary>
		/// <value>是否是单行模式。</value>
		private bool IsSingleLine => options.HasFlag(RegexOptions.Singleline);

		/// <summary>
		/// 获取是否消除模式中的非转义空白。
		/// </summary>
		/// <value>是否消除模式中的非转移空白。</value>
		private bool IsIgnoreWhitespace => options.HasFlag(RegexOptions.IgnorePatternWhitespace);

		/// <summary>
		/// 获取是否为表达式启用符合 ECMAScript 的行为。
		/// </summary>
		/// <value>是否为表达式启用符合 ECMAScript 的行为。</value>
		private bool IsECMA => options.HasFlag(RegexOptions.ECMAScript);

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
		private static bool IsSpecial(char ich)
		{
			return ich <= '|' && category[ich] >= S;
		}

		/// <summary>
		/// 返回指定的字符是否是正则表达式的普通字符。
		/// </summary>
		/// <param name="ich">要判断的字符。</param>
		/// <returns>如果字符是正则表达式的普通字符，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private static bool IsStopperX(char ich)
		{
			return ich <= '|' && category[ich] >= X;
		}

		/// <summary>
		/// 返回指定的字符是否是正则表达式数量的起始字符。
		/// </summary>
		/// <param name="ch">要判断的字符。</param>
		/// <returns>如果字符是正则表达式数量的起始字符，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private static bool IsQuantifier(char ch)
		{
			return ch <= '{' && category[ch] >= Q;
		}

		/// <summary>
		/// 返回之后的字符是否是正则表达式数量字符。
		/// </summary>
		/// <returns>如果是正则表达式数量字符，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private bool IsTrueQuantifier()
		{
			char ch = Peek();
			if (ch != '{')
			{
				return IsQuantifier(ch);
			}
			// 识别最小重复次数。
			int idx = currentPos;
			int len = CharsRight();
			while (--len > 0 && (ch = CharAt(++idx)) >= '0' && ch <= '9') ;
			if (len == 0 || idx - currentPos == 1)
			{
				return false;
			}
			else if (ch == '}')
			{
				// {n}
				return true;
			}
			else if (ch != ',')
			{
				return false;
			}
			// {n,m}
			// 识别最大重复次数。
			while (--len > 0 && (ch = CharAt(++idx)) >= '0' && ch <= '9') ;
			if (len > 0)
			{
				return ch == '}';
			}
			return false;
		}

		/// <summary>
		/// 返回之后的字符是否是 {name}。
		/// </summary>
		/// <returns>如果是 {name}，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private bool IsRegexName()
		{
			char ch = Peek();
			int idx = currentPos;
			if (ch != '{')
			{
				return false;
			}
			// 跳过单词。
			int len = CharsRight();
			while (--len > 0 && CharAt(++idx).IsWord()) ;
			return ch == '}' && idx - currentPos > 2;
		}

		/// <summary>
		/// 返回指定的字符是否是空白字符。
		/// </summary>
		/// <param name="ch">要判断的字符。</param>
		/// <returns>如果字符是空白字符，则为 <c>true</c>；否则为 <c>false</c>。</returns>
		private static bool IsSpace(char ch)
		{
			return ch <= ' ' && category[ch] == X;
		}

		#endregion // 字符类型

		#region 异常信息

		/// <summary>
		/// 创建 <see cref="RegexParseException"/> 新实例的委托类型。
		/// </summary>
		/// <param name="error">解析错误。</param>
		/// <param name="offset">错误所在的偏移。</param>
		/// <param name="message">错误消息。</param>
		/// <returns>创建的 <see cref="RegexParseException"/> 实例。</returns>
		private delegate RegexParseException ParseExceptionCreator(RegexParseError error, int offset, string message);

		/// <summary>
		/// 创建 <see cref="RegexParseException"/> 的新实例。
		/// </summary>
		private static readonly ParseExceptionCreator CreateParseException = typeof(RegexParseException)
			.PowerDelegate<ParseExceptionCreator>(TypeUtil.ConstructorName)!;

		/// <summary>
		/// 创建指定的正则表达式解析错误。
		/// </summary>
		/// <param name="error">正则表达式解析错误。</param>
		/// <param name="message">错误消息。</param>
		/// <param name="args">错误参数。</param>
		/// <returns>正则表达式解析错误。</returns>
		private RegexParseException CreateException(RegexParseError error, string message, params object?[] args)
		{
			if (args != null && args.Length > 0)
			{
				message = ResourcesUtil.Format(message, args);
			}
			string formatedMessage = ResourcesUtil.Format(Resources.InvalidRegexPattern, pattern, currentPos, message);
			return CreateParseException(error, currentPos, formatedMessage);
		}

		#endregion // 异常信息

	}
}
