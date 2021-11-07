using System;
using System.IO;
using Lexer;
using NUnit.Framework;

namespace CMinusMinus.Test {
	public class LexerTests {
		public static readonly Lexer<TokenType> Lexer = new CMinusMinus().Lexer;

		[TestCase(@"int func(){}", false, ExpectedResult = true)]
		[TestCase(@"'\'", true, ExpectedResult = false)]
		public bool LiteralTest(string code, bool checkAmbiguity) {
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

		[TestCase()]
		public bool FileTest(string filePath, bool checkAmbiguity) {
			string code = File.ReadAllText(filePath);
			try {
				var lexemes = Lexer.Tokenize(code, checkAmbiguity);
				foreach (var lexeme in lexemes)
					Console.Write(lexeme.ToString());
				return true;
			}
			catch (LexerException) {
				return false;
			}
		}
	}
}