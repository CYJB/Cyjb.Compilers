﻿//------------------------------------------------------------------------------
// <auto-generated>
// 此代码由工具生成。
//
// 对此文件的更改可能会导致不正确的行为，并且如果
// 重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cyjb.Compilers.Parsers;
namespace Cyjb.Compilers.Parsers.Production;


/// <summary>
/// 产生式语法分析器。
/// </summary>
internal partial class ProductionParser 
{
	/// <summary>
	/// 语法分析器的工厂。
	/// </summary>
	public static readonly IParserFactory<ProductionKind> Factory = CreateParserFactory();

	/// <summary>
	/// 创建语法分析器的工厂。
	/// </summary>
	[CompilerGeneratedAttribute]
	private static IParserFactory<ProductionKind> CreateParserFactory()
	{
		// 临时符号
		ProductionKind endOfFile = (ProductionKind)(-1);
		ProductionKind symbol_1 = (ProductionKind)(-2);
		// 产生式数据
		ProductionData<ProductionKind>[] productions = new[]
		{
			new ProductionData<ProductionKind>(ProductionKind.Expression,
				(ProductionParser c) => c.ExpressionAction(),
				ProductionKind.Expression,
				ProductionKind.Repeat),
			new ProductionData<ProductionKind>(ProductionKind.Expression,
				(ProductionParser c) => c.CopyAction(),
				ProductionKind.Repeat),
			new ProductionData<ProductionKind>(ProductionKind.Repeat,
				(ProductionParser c) => c.CopyAction(),
				ProductionKind.Item),
			new ProductionData<ProductionKind>(ProductionKind.Item,
				(ProductionParser c) => c.CopyAction(),
				ProductionKind.Id),
			new ProductionData<ProductionKind>(ProductionKind.Item,
				(ProductionParser c) => c.BraceAction(),
				ProductionKind.LBrace,
				ProductionKind.Expression,
				ProductionKind.RBrace),
			new ProductionData<ProductionKind>(ProductionKind.Repeat,
				(ProductionParser c) => c.RepeatAction(),
				ProductionKind.Item,
				ProductionKind.Plus),
			new ProductionData<ProductionKind>(ProductionKind.Repeat,
				(ProductionParser c) => c.RepeatAction(),
				ProductionKind.Item,
				ProductionKind.Star),
			new ProductionData<ProductionKind>(ProductionKind.Repeat,
				(ProductionParser c) => c.RepeatAction(),
				ProductionKind.Item,
				ProductionKind.Question),
			new ProductionData<ProductionKind>(symbol_1,
				null,
				ProductionKind.Expression)
		};
		// 状态数据
		ParserStateData<ProductionKind>[] states = new ParserStateData<ProductionKind>[12];
		// 0: 8 Expression' -> •Expression
		//    
		//    Id -> s4
		//    LBrace -> s5
		// 
		//    Expression -> 1
		//    Item -> 3
		//    Repeat -> 2
		Dictionary<ProductionKind, ParserAction> action_1 = new()
		{
			 { ProductionKind.Id, ParserAction.Shift(4) },
			 { ProductionKind.LBrace, ParserAction.Shift(5) }
		};
		HashSet<ProductionKind> expecting_1 = new()
		{
			ProductionKind.Id,
			ProductionKind.LBrace
		};
		states[0] = new ParserStateData<ProductionKind>(action_1,
			ParserAction.Error,
			expecting_1,
			productions[8],
			0,
			-2);
		// 1: 8 Expression' -> Expression•
		//    0 Expression -> Expression •Repeat
		//    
		//    Id -> s4
		//    LBrace -> s5
		//    EOF -> acc
		// 
		//    Item -> 3
		//    Repeat -> 6
		Dictionary<ProductionKind, ParserAction> action_2 = new()
		{
			 { endOfFile, ParserAction.Accept },
			 { ProductionKind.Id, ParserAction.Shift(4) },
			 { ProductionKind.LBrace, ParserAction.Shift(5) }
		};
		HashSet<ProductionKind> expecting_2 = new()
		{
			endOfFile,
			ProductionKind.Id,
			ProductionKind.LBrace
		};
		states[1] = new ParserStateData<ProductionKind>(action_2,
			ParserAction.Error,
			expecting_2,
			productions[8],
			1,
			1);
		// 2: 1 Expression -> Repeat•
		//    
		//    Id -> r1
		//    LBrace -> r1
		//    RBrace -> r1
		//    EOF -> r1
		Dictionary<ProductionKind, ParserAction> action_3 = new();
		HashSet<ProductionKind> expecting_3 = new()
		{
			ProductionKind.RBrace,
			ProductionKind.Id,
			ProductionKind.LBrace,
			endOfFile
		};
		states[2] = new ParserStateData<ProductionKind>(action_3,
			ParserAction.Reduce(1),
			expecting_3,
			productions[1],
			1,
			int.MinValue);
		// 3: 2 Repeat -> Item•
		//    5 Repeat -> Item •Plus
		//    6 Repeat -> Item •Star
		//    7 Repeat -> Item •Question
		//    
		//    Id -> r2
		//    LBrace -> r2
		//    Plus -> s7
		//    Question -> s9
		//    RBrace -> r2
		//    Star -> s8
		//    EOF -> r2
		Dictionary<ProductionKind, ParserAction> action_4 = new()
		{
			 { ProductionKind.Plus, ParserAction.Shift(7) },
			 { ProductionKind.Star, ParserAction.Shift(8) },
			 { ProductionKind.Question, ParserAction.Shift(9) }
		};
		HashSet<ProductionKind> expecting_4 = new()
		{
			ProductionKind.RBrace,
			ProductionKind.Id,
			ProductionKind.LBrace,
			endOfFile,
			ProductionKind.Plus,
			ProductionKind.Star,
			ProductionKind.Question
		};
		states[3] = new ParserStateData<ProductionKind>(action_4,
			ParserAction.Reduce(2),
			expecting_4,
			productions[2],
			1,
			int.MinValue);
		// 4: 3 Item -> Id•
		//    
		//    Id -> r3
		//    LBrace -> r3
		//    Plus -> r3
		//    Question -> r3
		//    RBrace -> r3
		//    Star -> r3
		//    EOF -> r3
		states[4] = new ParserStateData<ProductionKind>(action_3,
			ParserAction.Reduce(3),
			expecting_4,
			productions[3],
			1,
			int.MinValue);
		// 5: 4 Item -> LBrace •Expression RBrace
		//    
		//    Id -> s4
		//    LBrace -> s5
		// 
		//    Expression -> 10
		//    Item -> 3
		//    Repeat -> 2
		states[5] = new ParserStateData<ProductionKind>(action_1,
			ParserAction.Error,
			expecting_1,
			productions[4],
			1,
			5);
		// 6: 0 Expression -> Expression Repeat•
		//    
		//    Id -> r0
		//    LBrace -> r0
		//    RBrace -> r0
		//    EOF -> r0
		states[6] = new ParserStateData<ProductionKind>(action_3,
			ParserAction.Reduce(0),
			expecting_3,
			productions[0],
			2,
			int.MinValue);
		// 7: 5 Repeat -> Item Plus•
		//    
		//    Id -> r5
		//    LBrace -> r5
		//    RBrace -> r5
		//    EOF -> r5
		states[7] = new ParserStateData<ProductionKind>(action_3,
			ParserAction.Reduce(5),
			expecting_3,
			productions[5],
			2,
			int.MinValue);
		// 8: 6 Repeat -> Item Star•
		//    
		//    Id -> r6
		//    LBrace -> r6
		//    RBrace -> r6
		//    EOF -> r6
		states[8] = new ParserStateData<ProductionKind>(action_3,
			ParserAction.Reduce(6),
			expecting_3,
			productions[6],
			2,
			int.MinValue);
		// 9: 7 Repeat -> Item Question•
		//    
		//    Id -> r7
		//    LBrace -> r7
		//    RBrace -> r7
		//    EOF -> r7
		states[9] = new ParserStateData<ProductionKind>(action_3,
			ParserAction.Reduce(7),
			expecting_3,
			productions[7],
			2,
			int.MinValue);
		// 10: 4 Item -> LBrace Expression •RBrace
		//     0 Expression -> Expression •Repeat
		//     
		//     Id -> s4
		//     LBrace -> s5
		//     RBrace -> s11
		// 
		//     Item -> 3
		//     Repeat -> 6
		Dictionary<ProductionKind, ParserAction> action_5 = new()
		{
			 { ProductionKind.RBrace, ParserAction.Shift(11) },
			 { ProductionKind.Id, ParserAction.Shift(4) },
			 { ProductionKind.LBrace, ParserAction.Shift(5) }
		};
		HashSet<ProductionKind> expecting_5 = new()
		{
			ProductionKind.RBrace,
			ProductionKind.Id,
			ProductionKind.LBrace
		};
		states[10] = new ParserStateData<ProductionKind>(action_5,
			ParserAction.Error,
			expecting_5,
			productions[4],
			2,
			8);
		// 11: 4 Item -> LBrace Expression RBrace•
		//     
		//     Id -> r4
		//     LBrace -> r4
		//     Plus -> r4
		//     Question -> r4
		//     RBrace -> r4
		//     Star -> r4
		//     EOF -> r4
		states[11] = new ParserStateData<ProductionKind>(action_3,
			ParserAction.Reduce(4),
			expecting_4,
			productions[4],
			3,
			int.MinValue);
		// 转移数据
		Dictionary<ProductionKind, int> gotoMap = new()
		{
			 { ProductionKind.Expression, 0 },
			 { ProductionKind.Repeat, 1 },
			 { ProductionKind.Item, 3 },
			 { ProductionKind.Id, 9 },
			 { ProductionKind.LBrace, 15 },
			 { ProductionKind.RBrace, 8 },
			 { ProductionKind.Plus, 4 },
			 { ProductionKind.Star, 9 },
			 { ProductionKind.Question, 14 }
		};
		// 转移的目标
		int[] gotoNext = new[]
		{
			1, 2, 6, 3, 3, 10, 2, 7, 3, 4, 4, 6, 8, 3, 4, 5, 5, 9, 11, 4, 5, -1, -1, -1,
			-1, 5
		};
		// 转移的检查
		ProductionKind[] gotoCheck = new[]
		{
			ProductionKind.Expression,
			ProductionKind.Repeat,
			ProductionKind.Repeat,
			ProductionKind.Item,
			ProductionKind.Item,
			ProductionKind.Expression,
			ProductionKind.Repeat,
			ProductionKind.Plus,
			ProductionKind.Item,
			ProductionKind.Id,
			ProductionKind.Id,
			ProductionKind.Repeat,
			ProductionKind.Star,
			ProductionKind.Item,
			ProductionKind.Id,
			ProductionKind.LBrace,
			ProductionKind.LBrace,
			ProductionKind.Question,
			ProductionKind.RBrace,
			ProductionKind.Id,
			ProductionKind.LBrace,
			endOfFile,
			endOfFile,
			endOfFile,
			endOfFile,
			ProductionKind.LBrace
		};
		// 后继状态的目标
		int[] followNext = new[]
		{
			1, 2, 3, 3, 6, 3, 3, 10, 2, 3, 3, 6, 3, 3, -1, 3, -1, -1, -1, 3
		};
		// 后继状态的检查
		int[] followCheck = new[]
		{
			0, 0, 0, 0, 1, 1, 1, 5, 5, 5, 5, 10, 10, 10, -1, 5, -1, -1, -1, 10
		};
		// 语法分析器的数据
		ParserData<ProductionKind> parserData = new(productions,
			null,
			states,
			gotoMap,
			gotoNext,
			gotoCheck,
			followNext,
			followCheck);
		return new ParserFactory<ProductionKind, ProductionParser>(parserData);
	}
}



