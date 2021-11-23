using System;
using System.IO;
using Lexer;
using NUnit.Framework;

namespace CMinusMinus.Test {
	public class LexerTests {
		public static readonly Lexer.Lexer Lexer = new CMinusMinus().Lexer;

		[TestCase(@"int func(){}", ExpectedResult = true)]
		[TestCase(@"'\'", ExpectedResult = false)]
		public bool LiteralTest(string code, bool checkAmbiguity = false) {
			try {
				var tokens = Lexer.Tokenize(code, checkAmbiguity);
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
		public bool FileTest(string filePath, bool checkAmbiguity = false) => LiteralTest(File.ReadAllText(filePath), checkAmbiguity);
	}
}