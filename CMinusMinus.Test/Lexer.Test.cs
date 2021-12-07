using System;
using System.IO;
using System.Linq;
using Lexer;
using NUnit.Framework;

namespace CMinusMinus.Test {
	public class LexerTests {
		public static readonly Lexer.Lexer Lexer = new(CMinusMinus.Lexicon);

		public static TestCaseData[] FileSource { get; } = TestCases.Files.Select(path => new TestCaseData(path).Returns(true)).ToArray();

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
		[TestCaseSource(nameof(FileSource))]
		public bool FileTest(string filePath) => LiteralTest(File.ReadAllText(filePath));
	}
}