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
			new Token<int>(0, "", 7..20)
		);
		tokenizer = new MappedTokenizer<int>(tokenizer, new Tuple<int, int>[]
		{
			new Tuple<int, int>(1, 10),
			new Tuple<int, int>(5, 100),
		});
		Assert.AreEqual(0..10, tokenizer.Read().Span);
		Assert.AreEqual(10..12, tokenizer.Read().Span);
		Assert.AreEqual(12..102, tokenizer.Read().Span);
		Assert.AreEqual(102..115, tokenizer.Read().Span);

	}
}
