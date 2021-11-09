using System.IO;
using Parser.CanonicalLR;
using NUnit.Framework;

namespace CMinusMinus.Test {
	public class ParserTests {
		public static readonly CMinusMinus Language = new();

		[TestCase(@"int func(){}", ExpectedResult = true)]
		[TestCase(@"'\'", ExpectedResult = false)]
		public bool LiteralTest(string code) {
			var lexemes = Language.Lexer.Tokenize(code);
			try {
				var tree = Language.Parser.Parse(lexemes);
				return true;
			}
			catch {
				return false;
			}
		}

		[TestCase(@"samples/0.cmm", ExpectedResult = true)]
		[TestCase(@"samples/a+b.cmm", ExpectedResult = true)]
		[TestCase(@"samples/hello.cmm", ExpectedResult = true)]
		[TestCase(@"samples/literal.cmm", ExpectedResult = true)]
		public bool FileTest(string filePath) => LiteralTest(File.ReadAllText(filePath));
	}
}