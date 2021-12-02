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

		[TestCase(@"samples/0.cmm", ExpectedResult = true)]
		[TestCase(@"samples/a+b.cmm", ExpectedResult = true)]
		[TestCase(@"samples/hello.cmm", ExpectedResult = true)]
		[TestCase(@"samples/literal.cmm", ExpectedResult = true)]
		[TestCase(@"samples/full.cmm", ExpectedResult = true)]
		public bool FileTest(string filePath) => LiteralTest(File.ReadAllText(filePath));
	}
}