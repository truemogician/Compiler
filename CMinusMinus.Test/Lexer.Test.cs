using System;
using System.IO;
using Lexer;
using NUnit.Framework;

namespace CMinusMinus.Test {
	public class LexerTests {
		public static readonly Lexer.Lexer Lexer = new(CMinusMinus.Lexicon);

		[TestCase(@"int func(){}", ExpectedResult = true)]
		[TestCase(@"'\'", ExpectedResult = false)]
		public bool LiteralTest(string code) {
			try {
				var tokens = Lexer.Tokenize(code);
				foreach (var token in tokens)
					Console.WriteLine(token.ToString());
				return true;
			}
			catch (LexerException) {
				return false;
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.FileSource))]
		public bool FileTest(string filePath) => LiteralTest(File.ReadAllText(filePath));
	}
}