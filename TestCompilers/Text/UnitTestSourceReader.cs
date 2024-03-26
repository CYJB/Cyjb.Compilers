using System;
using System.IO;
using System.Text;
using Cyjb;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Text;

/// <summary>
/// <see cref="SourceReader"/> 类的单元测试。
/// </summary>
[TestClass]
public class UnitTestSourceReader
{
	private const int BufferLength = 0x100;

	/// <summary>
	/// 对 <see cref="SourceReader"/> 的字符串构造函数进行测试。
	/// </summary>
	[TestMethod]
	public void TestCreateString()
	{
		SourceReader reader = SourceReader.Create("1234567890");
		Assert.AreEqual('1', reader.Peek());
		Assert.AreEqual('1', reader.Peek());
		Assert.AreEqual('1', reader.Read());
		Assert.AreEqual('2', reader.Peek());
		Assert.AreEqual('2', reader.Read());
		Assert.AreEqual("12", reader.GetReadedText());
		Assert.IsTrue(reader.Unget());
		Assert.IsTrue(reader.Unget());
		Assert.IsFalse(reader.Unget());

		Assert.AreEqual('2', reader.Read(1));
		Assert.AreEqual("12", reader.GetReadedText());
		Assert.AreEqual(2, reader.Unget(5));

		Assert.AreEqual("", reader.GetReadedText());
		Assert.AreEqual('3', reader.Read(2));
		Assert.AreEqual("123", reader.GetReadedText());
		Assert.AreEqual("123", reader.Accept());

		Assert.AreEqual("", reader.GetReadedText());
		Assert.AreEqual("", reader.Accept());
		Assert.IsFalse(reader.Unget());
		Assert.AreEqual(0, reader.Unget(5));

		Assert.AreEqual('6', reader.Read(2));
		Assert.AreEqual("456", reader.GetReadedText());
		reader.Drop();

		Assert.AreEqual(SourceReader.InvalidCharacter, reader.Read(10));
		Assert.AreEqual("7890", reader.GetReadedText());
		Token<int> token = reader.AcceptToken(11);
		Assert.AreEqual(11, token.Kind);
		Assert.AreEqual("7890", token.Text);
		Assert.AreEqual(new TextSpan(6, 10), token.Span);
	}

	/// <summary>
	/// 对 <see cref="SourceReader"/> 的字符串视图构造函数进行测试。
	/// </summary>
	[TestMethod]
	public void TestCreateStringView()
	{
		SourceReader reader = SourceReader.Create("a1234567890b".AsView(1, 10));
		Assert.AreEqual('1', reader.Peek());
		Assert.AreEqual('1', reader.Peek());
		Assert.AreEqual('1', reader.Read());
		Assert.AreEqual('2', reader.Peek());
		Assert.AreEqual('2', reader.Read());
		Assert.AreEqual("12", reader.GetReadedText());
		Assert.IsTrue(reader.Unget());
		Assert.IsTrue(reader.Unget());
		Assert.IsFalse(reader.Unget());

		Assert.AreEqual('2', reader.Read(1));
		Assert.AreEqual("12", reader.GetReadedText());
		Assert.AreEqual(2, reader.Unget(5));

		Assert.AreEqual("", reader.GetReadedText());
		Assert.AreEqual('3', reader.Read(2));
		Assert.AreEqual("123", reader.GetReadedText());
		Assert.AreEqual("123", reader.Accept());

		Assert.AreEqual("", reader.GetReadedText());
		Assert.AreEqual("", reader.Accept());
		Assert.IsFalse(reader.Unget());
		Assert.AreEqual(0, reader.Unget(5));

		Assert.AreEqual('6', reader.Read(2));
		Assert.AreEqual("456", reader.GetReadedText());
		reader.Drop();

		Assert.AreEqual(SourceReader.InvalidCharacter, reader.Read(10));
		Assert.AreEqual("7890", reader.GetReadedText());
		Token<int> token = reader.AcceptToken(11);
		Assert.AreEqual(11, token.Kind);
		Assert.AreEqual("7890", token.Text);
		Assert.AreEqual(new TextSpan(6, 10), token.Span);
	}

	/// <summary>
	/// 对 <see cref="SourceReader"/> 读取短文本进行测试。
	/// </summary>
	[DataTestMethod]
	[DataRow("StringReader")]
	[DataRow("TextReader")]
	public void TestShortText(string type)
	{
		SourceReader reader = SourceReader.Create(CreateReader(type, "1234567890"), BufferLength);
		Assert.AreEqual('1', reader.Peek());
		Assert.AreEqual('1', reader.Peek());
		Assert.AreEqual('1', reader.Read());
		Assert.AreEqual('2', reader.Peek());
		Assert.AreEqual('2', reader.Read());
		Assert.AreEqual("12", reader.GetReadedText());
		Assert.IsTrue(reader.Unget());
		Assert.IsTrue(reader.Unget());
		Assert.IsFalse(reader.Unget());

		Assert.AreEqual('2', reader.Read(1));
		Assert.AreEqual("12", reader.GetReadedText());
		Assert.AreEqual(2, reader.Unget(5));

		Assert.AreEqual("", reader.GetReadedText());
		Assert.AreEqual('3', reader.Read(2));
		Assert.AreEqual("123", reader.GetReadedText());
		Assert.AreEqual("123", reader.Accept());

		Assert.AreEqual("", reader.GetReadedText());
		Assert.AreEqual("", reader.Accept());
		Assert.IsFalse(reader.Unget());
		Assert.AreEqual(0, reader.Unget(5));

		Assert.AreEqual('6', reader.Read(2));
		Assert.AreEqual("456", reader.GetReadedText());
		reader.Drop();

		Assert.AreEqual(SourceReader.InvalidCharacter, reader.Read(10));
		Assert.AreEqual("7890", reader.GetReadedText());
		Token<int> token = reader.AcceptToken(11);
		Assert.AreEqual(11, token.Kind);
		Assert.AreEqual("7890", token.Text);
		Assert.AreEqual(new TextSpan(6, 10), token.Span);
	}

	/// <summary>
	/// 对 <see cref="SourceReader"/> 读取长文本进行测试。
	/// </summary>
	[DataTestMethod]
	[DataRow("StringReader")]
	[DataRow("TextReader")]
	public void TestLongText(string type)
	{
		int len1 = BufferLength + 11;
		int len2 = BufferLength * 3 - 53;
		int len3 = BufferLength + 49;
		int len4 = len2 + len3;
		int length = len4 + 100;

		StringBuilder builder = new(length);
		for (int i = 0; i < length; i++)
		{
			builder.Append((char)Random.Shared.Next(char.MaxValue));
		}
		string text = builder.ToString();
		SourceReader reader = SourceReader.Create(CreateReader(type, text), BufferLength);

		Assert.AreEqual(text[0], reader.Peek());
		Assert.AreEqual(text[0], reader.Peek());
		Assert.AreEqual(text[0], reader.Read());
		Assert.AreEqual(text[1], reader.Peek());
		Assert.AreEqual(text[1], reader.Read());
		Assert.AreEqual(text[..2], reader.GetReadedText());
		Assert.IsTrue(reader.Unget());
		Assert.IsTrue(reader.Unget());
		Assert.IsFalse(reader.Unget());

		Assert.AreEqual(text[len1 - 1], reader.Read(len1 - 1));
		Assert.AreEqual(text[..len1], reader.GetReadedText());
		// 回退超过已读取字符个数时，只会回退到起始位置。
		Assert.AreEqual(len1, reader.Unget(len1 + 100));
		Assert.AreEqual("", reader.GetReadedText());

		Assert.AreEqual(text[len2 - 1], reader.Read(len2 - 1));
		Assert.AreEqual(text[..len2], reader.GetReadedText());
		Assert.AreEqual(text[..len2], reader.Accept());

		// Accept 之后，无法再向前回退。
		Assert.AreEqual("", reader.GetReadedText());
		Assert.AreEqual("", reader.Accept());
		Assert.IsFalse(reader.Unget());
		Assert.AreEqual(0, reader.Unget(5));

		Assert.AreEqual(text[len4 - 1], reader.Read(len3 - 1));
		Assert.AreEqual(text[len2..len4], reader.GetReadedText());
		reader.Drop();

		Assert.AreEqual(SourceReader.InvalidCharacter, reader.Read(length));
		Assert.AreEqual(text[len4..], reader.GetReadedText());
		Token<int> token = reader.AcceptToken(11);
		Assert.AreEqual(11, token.Kind);
		Assert.AreEqual(text[len4..], token.Text);
		Assert.AreEqual(new TextSpan(len4, text.Length), token.Span);
	}

	/// <summary>
	/// 对 <see cref="SourceReader.IsLineStart"/> 进行测试。
	/// </summary>
	[DataTestMethod]
	[DataRow("StringReader")]
	[DataRow("TextReader")]
	public void TestIsLineStart(string type)
	{
		StringBuilder builder = new(BufferLength * 2 + 3);
		builder.Append("123\r\r456\n\n789\r\n\r\n123\r\r\n\n");
		builder.Append('1', BufferLength - builder.Length - 1);
		builder.Append("\r\n123");
		builder.Append('1', BufferLength * 2 - builder.Length - 1);
		builder.Append("\n123");
		SourceReader reader = SourceReader.Create(CreateReader(type, builder.ToString()), BufferLength);
		Assert.IsTrue(reader.IsLineStart);

		Assert.AreEqual('3', reader.Read(2));
		Assert.IsFalse(reader.IsLineStart);

		Assert.AreEqual('\r', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		Assert.AreEqual('\r', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		Assert.AreEqual('6', reader.Read(2));
		Assert.IsFalse(reader.IsLineStart);

		Assert.AreEqual('\n', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		Assert.AreEqual('\n', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		Assert.AreEqual('9', reader.Read(2));
		Assert.IsFalse(reader.IsLineStart);

		Assert.AreEqual('\r', reader.Read());
		Assert.IsFalse(reader.IsLineStart);
		Assert.AreEqual('\n', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		Assert.AreEqual('\r', reader.Read());
		Assert.IsFalse(reader.IsLineStart);
		Assert.AreEqual('\n', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		Assert.AreEqual('3', reader.Read(2));
		Assert.IsFalse(reader.IsLineStart);

		Assert.AreEqual('\r', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		Assert.AreEqual('\r', reader.Read());
		Assert.IsFalse(reader.IsLineStart);
		Assert.AreEqual('\n', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		Assert.AreEqual('\n', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		reader.Index = BufferLength - 1;
		Assert.IsFalse(reader.IsLineStart);

		Assert.AreEqual('\r', reader.Read());
		Assert.IsFalse(reader.IsLineStart);
		Assert.AreEqual('\n', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		reader.Index = BufferLength * 2 - 1;
		Assert.IsFalse(reader.IsLineStart);

		Assert.AreEqual('\n', reader.Read());
		Assert.IsTrue(reader.IsLineStart);

		Assert.AreEqual('3', reader.Read(2));
		Assert.IsFalse(reader.IsLineStart);
	}

	/// <summary>
	/// 对 <see cref="SourceReader.UseLineLocator"/> 进行测试。
	/// </summary>
	[DataTestMethod]
	[DataRow("StringReader")]
	[DataRow("TextReader")]
	public void TestLocator(string type)
	{
		string text = "12\r34\n56\r\n78";
		// 一次性读入。
		SourceReader reader = SourceReader.Create(CreateReader(type, text), BufferLength);
		reader.UseLineLocator();

		Assert.AreEqual('8', reader.Read(11));
		Assert.AreEqual(new LinePosition(1, 0, 1), reader.GetPosition(0));
		Assert.AreEqual(new LinePosition(1, 1, 2), reader.GetPosition(1));
		Assert.AreEqual(new LinePosition(1, 2, 3), reader.GetPosition(2));
		Assert.AreEqual(new LinePosition(2, 0, 1), reader.GetPosition(3));
		Assert.AreEqual(new LinePosition(2, 1, 2), reader.GetPosition(4));
		Assert.AreEqual(new LinePosition(2, 2, 3), reader.GetPosition(5));
		Assert.AreEqual(new LinePosition(3, 0, 1), reader.GetPosition(6));
		Assert.AreEqual(new LinePosition(3, 1, 2), reader.GetPosition(7));
		Assert.AreEqual(new LinePosition(3, 2, 3), reader.GetPosition(8));
		Assert.AreEqual(new LinePosition(3, 3, 3), reader.GetPosition(9));
		Assert.AreEqual(new LinePosition(4, 0, 1), reader.GetPosition(10));
		Assert.AreEqual(new LinePosition(4, 1, 2), reader.GetPosition(11));

		// 分批读入。
		reader = SourceReader.Create(CreateReader(type, text), BufferLength);
		reader.UseLineLocator();

		Assert.AreEqual('2', reader.Read(1));
		Assert.AreEqual(new LinePosition(1, 0, 1), reader.GetPosition(0));
		Assert.AreEqual(new LinePosition(1, 1, 2), reader.GetPosition(1));

		Assert.AreEqual('\r', reader.Read());
		Assert.AreEqual(new LinePosition(1, 2, 3), reader.GetPosition(2));
		// 现在总是批量读入的，这时已经读入下一个字符，已确认 \r 就是换行。
		Assert.AreEqual(new LinePosition(2, 0, 1), reader.GetPosition(3));

		Assert.AreEqual('4', reader.Read(1));
		Assert.AreEqual(new LinePosition(2, 0, 1), reader.GetPosition(3));
		Assert.AreEqual(new LinePosition(2, 1, 2), reader.GetPosition(4));

		Assert.AreEqual('\n', reader.Read());
		Assert.AreEqual(new LinePosition(2, 2, 3), reader.GetPosition(5));
		Assert.AreEqual(new LinePosition(3, 0, 1), reader.GetPosition(6));

		Assert.AreEqual('6', reader.Read(1));
		Assert.AreEqual(new LinePosition(3, 0, 1), reader.GetPosition(6));
		Assert.AreEqual(new LinePosition(3, 1, 2), reader.GetPosition(7));

		Assert.AreEqual('\r', reader.Read());
		Assert.AreEqual(new LinePosition(3, 2, 3), reader.GetPosition(8));
		Assert.AreEqual(new LinePosition(3, 3, 3), reader.GetPosition(9));

		Assert.AreEqual('\n', reader.Read());
		Assert.AreEqual(new LinePosition(3, 3, 3), reader.GetPosition(9));
		Assert.AreEqual(new LinePosition(4, 0, 1), reader.GetPosition(10));

		Assert.AreEqual('8', reader.Read(1));
		Assert.AreEqual(new LinePosition(4, 0, 1), reader.GetPosition(10));
		Assert.AreEqual(new LinePosition(4, 1, 2), reader.GetPosition(11));
	}

	/// <summary>
	/// 对 <see cref="SourceReader.Mark"/> 进行测试。
	/// </summary>
	[DataTestMethod]
	[DataRow("StringReader")]
	[DataRow("TextReader")]
	public void TestMark(string type)
	{
		StringBuilder builder = new(1030);
		for (int i = 0; i < 103; i++)
		{
			builder.Append("0123456789");
		}
		SourceReader reader = SourceReader.Create(CreateReader(type, builder.ToString()), BufferLength);
		Assert.AreEqual('0', reader.Read());
		Assert.AreEqual("0", reader.ReadBlock(0, 1));
		Assert.AreEqual("", reader.ReadBlock(0, 0));
		Assert.AreEqual("", reader.ReadBlock(1, 0));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => reader.ReadBlock(-1, 0));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => reader.ReadBlock(0, 2));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => reader.ReadBlock(1, 1));

		SourceMark mark1 = reader.Mark();
		Assert.AreEqual('3', reader.Read(512));
		SourceMark mark2 = reader.Mark();
		reader.Drop();
		Assert.AreEqual('6', reader.Read(512));
		SourceMark mark3 = reader.Mark();
		reader.Drop();

		Assert.ThrowsException<ArgumentOutOfRangeException>(() => reader.ReadBlock(0, 1));
		Assert.AreEqual("", reader.ReadBlock(1, 0));
		Assert.AreEqual("1", reader.ReadBlock(1, 1));
		Assert.AreEqual("123", reader.ReadBlock(1, 3));

		StringView text = reader.ReadBlock(49, 13);
		Assert.AreEqual('9', text[0]);
		Assert.AreEqual('1', text[^1]);

		text = reader.ReadBlock(1, 1026);
		Assert.AreEqual('1', text[0]);
		Assert.AreEqual('6', text[^1]);
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => reader.ReadBlock(1, 1027));

		text = reader.ReadBlock(mark1, mark2);
		Assert.AreEqual('1', text[0]);
		Assert.AreEqual('3', text[^1]);

		text = reader.ReadBlock(mark2, mark3);
		Assert.AreEqual('4', text[0]);
		Assert.AreEqual('6', text[^1]);

		reader.Release(mark2);
		Assert.ThrowsException<ArgumentException>(() => reader.ReadBlock(mark2, mark3));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => reader.ReadBlock(mark3, mark1));
	}

	private static TextReader CreateReader(string type, string text)
	{
		if (type == "StringReader")
		{
			return new StringReader(text);
		}
		else
		{
			return new TestReader(text);
		}
	}

	/// <summary>
	/// 用于测试的文本读取器。
	/// </summary>
	private sealed class TestReader : TextReader
	{
		/// <summary>
		/// 要读取的文本。
		/// </summary>
		private readonly string text;
		/// <summary>
		/// 当前位置。
		/// </summary>
		private int pos;
		/// <summary>
		/// 文本的总长度。
		/// </summary>
		private readonly int length;

		public TestReader(string text)
		{
			this.text = text;
			length = text.Length;
		}

		public override int Peek()
		{
			if (pos == length)
			{
				return -1;
			}
			return text[pos];
		}
		public override int Read()
		{
			if (pos == length)
			{
				return -1;
			}
			return text[pos++];
		}
		public override int Read(char[] buffer, int index, int count)
		{
			int num = length - pos;
			if (num > 0)
			{
				if (num > count)
				{
					num = count;
				}
				text.CopyTo(pos, buffer, index, num);
				pos += num;
			}
			return num;
		}
		public override int Read(Span<char> buffer)
		{
			int num = length - pos;
			if (num > 0)
			{
				if (num > buffer.Length)
				{
					num = buffer.Length;
				}
				text.AsSpan(pos, num).CopyTo(buffer);
				pos += num;
			}
			return num;
		}
		public override int ReadBlock(Span<char> buffer)
		{
			return Read(buffer);
		}
		public override string ReadToEnd()
		{
			string result = (pos != 0) ? text[pos..length] : text;
			pos = length;
			return result;
		}
	}
}
