using System;
using Cyjb.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCompilers.Text;

/// <summary>
/// <see cref="MappedTokenizer{T}"/> 类的单元测试。
/// </summary>
[TestClass]
public class UnitTestMappedTokenizer
{
	/// <summary>
	/// 对 <see cref="MappedTokenizer{T}"/> 类进行测试。
	/// </summary>
	[TestMethod]
	public void TestMap()
	{
		ITokenizer<int> tokenizer = new EnumerableTokenizer<int>(
			new Token<int>(0, "", 0..1),
			new Token<int>(0, "", 1..3),
			new Token<int>(0, "", 3..7),
			new Token<int>(0, "", 8..20)
		);
		tokenizer = new MappedTokenizer<int>(tokenizer, new Tuple<int, int>[]
		{
			new Tuple<int, int>(1, 10),
			new Tuple<int, int>(5, 100),
			new Tuple<int, int>(8, 101),
			new Tuple<int, int>(10, 103),
			new Tuple<int, int>(15, 108),
		});
		Assert.AreEqual(0..10, tokenizer.Read().Span);
		Assert.AreEqual(10..12, tokenizer.Read().Span);
		Assert.AreEqual(12..100, tokenizer.Read().Span);
		Assert.AreEqual(101..113, tokenizer.Read().Span);
	}
}
