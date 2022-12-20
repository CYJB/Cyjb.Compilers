using System;
using System.IO;
using System.Text;
using Cyjb.Compilers.Text;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Text
{
	/// <summary>
	/// <see cref="SourceReader"/> 类的单元测试。
	/// </summary>
	[TestClass]
	public class UnitTestSourceReader
	{
		/// <summary>
		/// 对 <see cref="SourceReader"/> 读取短文本进行测试。
		/// </summary>
		[TestMethod]
		public void TestShortText()
		{
			SourceReader reader = new(new StringReader("1234567890"));
			Assert.AreEqual('1', reader.Peek());
			Assert.AreEqual('1', reader.Peek());
			Assert.AreEqual('1', reader.Read());
			Assert.AreEqual('2', reader.Peek());
			Assert.AreEqual('2', reader.Read());
			Assert.AreEqual("12", reader.ReadedText());
			Assert.IsTrue(reader.Unget());
			Assert.IsTrue(reader.Unget());
			Assert.IsFalse(reader.Unget());

			Assert.AreEqual('2', reader.Read(1));
			Assert.AreEqual("12", reader.ReadedText());
			Assert.AreEqual(2, reader.Unget(5));

			Assert.AreEqual("", reader.ReadedText());
			Assert.AreEqual('3', reader.Read(2));
			Assert.AreEqual("123", reader.ReadedText());
			Assert.AreEqual("123", reader.Accept());

			Assert.AreEqual("", reader.ReadedText());
			Assert.AreEqual("", reader.Accept());
			Assert.IsFalse(reader.Unget());
			Assert.AreEqual(0, reader.Unget(5));

			Assert.AreEqual('6', reader.Read(2));
			Assert.AreEqual("456", reader.ReadedText());
			reader.Drop();

			Assert.AreEqual(SourceReader.InvalidCharacter, reader.Read(10));
			Assert.AreEqual("7890", reader.ReadedText());
			Token<int> token = reader.AcceptToken(11);
			Assert.AreEqual(11, token.Kind);
			Assert.AreEqual("7890", token.Text);
			Assert.AreEqual(new TextSpan(6, 10), token.Span);
		}

		/// <summary>
		/// 对 <see cref="SourceReader"/> 读取长文本进行测试。
		/// </summary>
		[TestMethod]
		public void TestLongText()
		{
			StringBuilder builder = new(2813);
			for (int i = 0; i < 2813; i++)
			{
				builder.Append((char)Random.Shared.Next(char.MaxValue));
			}
			string text = builder.ToString();
			SourceReader reader = new(new StringReader(text));

			Assert.AreEqual(text[0], reader.Peek());
			Assert.AreEqual(text[0], reader.Peek());
			Assert.AreEqual(text[0], reader.Read());
			Assert.AreEqual(text[1], reader.Peek());
			Assert.AreEqual(text[1], reader.Read());
			Assert.AreEqual(text[..2], reader.ReadedText());
			Assert.IsTrue(reader.Unget());
			Assert.IsTrue(reader.Unget());
			Assert.IsFalse(reader.Unget());

			Assert.AreEqual(text[521], reader.Read(521));
			Assert.AreEqual(text[..522], reader.ReadedText());
			Assert.AreEqual(522, reader.Unget(530));

			Assert.AreEqual("", reader.ReadedText());
			Assert.AreEqual(text[1482], reader.Read(1482));
			Assert.AreEqual(text[..1483], reader.ReadedText());
			Assert.AreEqual(text[..1483], reader.Accept());

			Assert.AreEqual("", reader.ReadedText());
			Assert.AreEqual("", reader.Accept());
			Assert.IsFalse(reader.Unget());
			Assert.AreEqual(0, reader.Unget(5));

			Assert.AreEqual(text[1483 + 561], reader.Read(561));
			Assert.AreEqual(text[1483..(1483 + 561 + 1)], reader.ReadedText());
			reader.Drop();

			Assert.AreEqual(SourceReader.InvalidCharacter, reader.Read(4000));
			Assert.AreEqual(text[(1483 + 561 + 1)..], reader.ReadedText());
			Token<int> token = reader.AcceptToken(11);
			Assert.AreEqual(11, token.Kind);
			Assert.AreEqual(text[(1483 + 561 + 1)..], token.Text);
			Assert.AreEqual(new TextSpan(1483 + 561 + 1, text.Length), token.Span);
		}

		/// <summary>
		/// 对 <see cref="SourceReader.IsLineStart"/> 进行测试。
		/// </summary>
		[TestMethod]
		public void TestIsLineStart()
		{
			const int BufferLength = 0x200;
			StringBuilder builder = new(1030);
			builder.Append("123\r\r456\n\n789\r\n\r\n123\r\r\n\n");
			builder.Append('1', BufferLength - builder.Length - 1);
			builder.Append("\r\n123");
			builder.Append('1', BufferLength * 2 - builder.Length - 1);
			builder.Append("\n123");
			SourceReader reader = new(new StringReader(builder.ToString()));
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
		/// 对 <see cref="SourceReader.End"/> 进行测试。
		/// </summary>
		[TestMethod]
		public void TestEnd()
		{
			SourceReader reader = new(new StringReader("1234567890"));
			Assert.AreEqual('1', reader.Peek());
			Assert.AreEqual('1', reader.Read());
			reader.End = 2;
			Assert.AreEqual('2', reader.Peek());
			Assert.AreEqual('2', reader.Read());
			Assert.AreEqual(SourceReader.InvalidCharacter, reader.Peek());
			Assert.AreEqual(SourceReader.InvalidCharacter, reader.Read());
			reader.End = 3;
			Assert.AreEqual('3', reader.Peek());
			Assert.AreEqual('3', reader.Read());
			Assert.AreEqual(SourceReader.InvalidCharacter, reader.Peek());
			Assert.AreEqual(SourceReader.InvalidCharacter, reader.Read());
		}

		/// <summary>
		/// 对 <see cref="SourceReader.Mark"/> 进行测试。
		/// </summary>
		[TestMethod]
		public void TestMark()
		{
			StringBuilder builder = new(1030);
			for (int i = 0; i < 103; i++)
			{
				builder.Append("0123456789");
			}
			SourceReader reader = new(new StringReader(builder.ToString()));
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

			string text = reader.ReadBlock(49, 13);
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
	}
}
