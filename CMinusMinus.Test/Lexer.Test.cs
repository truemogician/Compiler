using System;
using System.IO;
using Lexer;
using NUnit.Framework;

namespace CMinusMinus.Test {
	public class LexerTests {
		public static readonly Lexer<TokenType> Lexer = new CMinusMinus().Lexer;

		[TestCase(@"int func(){}", ExpectedResult = true)]
		[TestCase(@"'\'", ExpectedResult = false)]
		public bool LiteralTest(string code, bool checkAmbiguity = false) {
			try {
				var lexemes = Lexer.Tokenize(code, checkAmbiguity);
				foreach (var lexeme in lexemes)
					Console.WriteLine(lexeme.ToString());
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