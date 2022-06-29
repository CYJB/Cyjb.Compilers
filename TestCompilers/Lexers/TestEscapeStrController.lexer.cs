//------------------------------------------------------------------------------
// <auto-generated>
// �˴����ɹ������ɡ�
//
// �Դ��ļ��ĸ��Ŀ��ܻᵼ�²���ȷ����Ϊ���������
// �������ɴ��룬��Щ���Ľ��ᶪʧ��
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Text;
using Cyjb.Compilers.Lexers;
using System;
using System.Collections.Generic;
using System.Globalization;
using Cyjb.Text;

namespace TestCompilers.Lexers;

public partial class TestEscapeStrController 
{
	/// <summary>
	/// �ʷ��������Ĺ�����
	/// </summary>
	public static readonly ILexerFactory<Str> Factory = CreateLexerFactory();

	/// <summary>
	/// �����ʷ��������Ĺ�����
	/// </summary>
	[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
	private static ILexerFactory<Str> CreateLexerFactory()
	{
		// ����������
		Dictionary<string, ContextData<Str>> contexts = new()
		{
			{ "Initial", new ContextData<Str>(0, "Initial") },
			{ "str", new ContextData<Str>(1, "str") },
			{ "vstr", new ContextData<Str>(2, "vstr") }
		};
		// �ս������
		TerminalData<Str>[] terminals = new[]
		{
			new TerminalData<Str>(action: (TestEscapeStrController c) => c.BeginStrAction()),
			new TerminalData<Str>(action: (TestEscapeStrController c) => c.BeginVstrAction()),
			new TerminalData<Str>(Str.Str, (TestEscapeStrController c) => c.EndAction()),
			new TerminalData<Str>(action: (TestEscapeStrController c) => c.HexEscapeAction()),
			new TerminalData<Str>(action: (TestEscapeStrController c) => c.HexEscapeAction()),
			new TerminalData<Str>(action: (TestEscapeStrController c) => c.EscapeLFAction()),
			new TerminalData<Str>(action: (TestEscapeStrController c) => c.EscapeQuoteAction()),
			new TerminalData<Str>(action: (TestEscapeStrController c) => c.EscapeCRAction()),
			new TerminalData<Str>(action: (TestEscapeStrController c) => c.CopyAction()),
			new TerminalData<Str>(action: (TestEscapeStrController c) => c.VstrQuoteAction())
		};
		// �ַ�������
		int[] indexes = new[]
		{
			10551295
		};
		// �ַ����б�
		int[] classes = new[]
		{
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 0, 9, 9, 0, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 1, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
			5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 9, 9, 9, 9, 9, 9, 2, 9, 9, 9, 9, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 3, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 7, 9, 9, 9, 8, 9, 9, 4, 9, 9,
			6, 9, 9, 9, 9, 9, 9, 9, 9
		};
		// �ַ��� Unicode ���
		Dictionary<UnicodeCategory, int> categories = new()
		{
			{ UnicodeCategory.Control, 9 }
		};
		// ״̬�б�
		DfaStateData[] states = new[]
		{
			new DfaStateData(-1, 2),
			new DfaStateData(1, 2),
			new DfaStateData(4, -1),
			new DfaStateData(2, -1, 2, 8),
			new DfaStateData(int.MinValue, -1, 8),
			new DfaStateData(int.MinValue, -1, 9),
			new DfaStateData(int.MinValue, -1, 2, 8),
			new DfaStateData(13, -1, 8),
			new DfaStateData(int.MinValue, -1, 6),
			new DfaStateData(10, -1),
			new DfaStateData(11, -1),
			new DfaStateData(int.MinValue, -1, 5),
			new DfaStateData(int.MinValue, -1, 7),
			new DfaStateData(13, -1),
			new DfaStateData(int.MinValue, -1, 4),
			new DfaStateData(17, -1),
			new DfaStateData(18, -1),
			new DfaStateData(19, -1),
			new DfaStateData(int.MinValue, -1, 3),
			new DfaStateData(int.MinValue, -1, 0, 8),
			new DfaStateData(24, -1, 8),
			new DfaStateData(int.MinValue, -1, 1)
		};
		// ���״̬�б�
		int[] next = new[]
		{
			19, 20, 6, 5, 7, 3, 4, 4, 4, 4, 4, 4, 4, 4, 8, 15, 13, 9, 14, 10, 11, 12, 16, 17,
			18, 21
		};
		// ״̬����б�
		int[] check = new[]
		{
			0, 0, 1, 3, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7, 9, 10, 7, 13, 7, 7, 7, 15, 16,
			17, 20
		};
		// �ʷ�������������
		LexerData<Str> lexerData = new(contexts,
			terminals,
			new CharClassMap(indexes, classes, categories),
			states,
			next,
			check,
			TrailingType.None,
			false,
			false,
			typeof(TestEscapeStrController));
		return new LexerFactory<Str, TestEscapeStrController>(lexerData);
	}

}