using System.Collections.Generic;
using System.Linq;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Text
{
	/// <summary>
	/// <see cref="Token{T}"/> 类的单元测试。
	/// </summary>
	[TestClass]
	public class UnitTestToken
	{
		/// <summary>
		/// 对 <see cref="Token{T}"/> 的构造函数进行测试。
		/// </summary>
		[TestMethod]
		public void TestToken()
		{
			Token<int> token = new(10, "test", new TextSpan(10, 30));
			Assert.AreEqual(10, token.Kind);
			Assert.AreEqual("test", token.Text);
			Assert.AreEqual(new TextSpan(10, 30), token.Span);
			Assert.IsNull(token.Value);

			token = new(11, "test2", new TextSpan(20, 40), "test2");
			Assert.AreEqual(11, token.Kind);
			Assert.AreEqual("test2", token.Text);
			Assert.AreEqual(new TextSpan(20, 40), token.Span);
			Assert.AreEqual("test2", token.Value);
		}

		/// <summary>
		/// 对 <see cref="Token{T}.Combine"/> 方法进行测试。
		/// </summary>
		[TestMethod]
		public void TestCombine()
		{
			Assert.AreEqual(new Token<int>(1, "abcdef", 0..7, "aaa"), Token<int>.Combine(
				new Token<int>(1, "abc", 0..1, "aaa"),
				new Token<int>(3, "def", 5..7, "bbb")
			));

			LineLocator locator1 = new();
			LineLocator locator2 = new();
			Assert.AreEqual(Token<int>.GetEndOfFile(0), Token<int>.Combine());
			Assert.AreEqual(new Token<int>(1, "abc", 0..1, locator1, "aaa"), Token<int>.Combine(
				new Token<int>(1, "abc", 0..1, locator1, "aaa")
				));
			Assert.AreEqual(new Token<int>(1, "abcdef123", 0..5, locator1, "aaa"), Token<int>.Combine(
				new Token<int>(1, "abc", 0..1, locator1, "aaa"),
				new Token<int>(3, "def", 5..7, locator2, "bbb"),
				new Token<int>(8, "123", 2..5, "ccc")
				));

			Assert.AreEqual(Token<int>.GetEndOfFile(0), Token<int>.Combine(Enumerable.Empty<Token<int>>()));
			Assert.AreEqual(new Token<int>(1, "abc", 0..1, locator1, "aaa"), Token<int>.Combine(
				(IEnumerable<Token<int>>)new Token<int>[] {
				new Token<int>(1, "abc", 0..1, locator1, "aaa")
				}));
			Assert.AreEqual(new Token<int>(1, "abcdef123", 0..5, locator1, "aaa"), Token<int>.Combine(
				(IEnumerable<Token<int>>)new Token<int>[] {
				new Token<int>(1, "abc", 0..1, locator1, "aaa"),
				new Token<int>(3, "def", 5..7, locator2, "bbb"),
				new Token<int>(8, "123", 2..5, "ccc")
				}));
		}
	}
}
