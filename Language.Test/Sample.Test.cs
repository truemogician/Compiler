using System;
using System.Diagnostics.CodeAnalysis;
using Lexer;
using NUnit.Framework;
using Parser;
using Parser.LR;

#nullable enable
namespace Language.Test {
	public class SampleTests {
		public Sample Language { get; } = new();

		[Test]
		public void ParsingTableTest() {
			var table = Language.Parser.ParsingTable;
			Assert.AreEqual(table.ItemSets.InitialState.Count, 6);
			Assert.AreEqual(table.ItemSets.Count, 10);
			Assert.AreEqual(table[table[table.ItemSets.InitialState, "S"]!, Terminal.Terminator]!.Type, ActionType.Accept);
		}

		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		[TestCase("abab", ExpectedResult = null)]
		[TestCase("aba", ExpectedResult = typeof(NotRecognizedException))]
		[TestCase("ac", ExpectedResult = typeof(LexemeNotMatchedException))]
		public Type? ParseTest(string code) {
			try {
				Console.Write(Language.Parse(code).ToString());
				return null;
			}
			catch (ParserException ex) {
				return ex.GetType();
			}
			catch (LexerException ex) {
				return ex.GetType();
			}
		}
	}
}