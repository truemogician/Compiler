using System;
using System.IO;
using NUnit.Framework;
using Parser;
using Parser.LR;

#nullable enable
namespace CMinusMinus.Test {
	public class CompiledParserTests {
		[Test]
		public void SaveTest() {
			var language = new CMinusMinus();
			var compiled = language.Parser.Compile();
			compiled.Save("cmm.ptb");
		}

		[TestCase("int main(){}", ExpectedResult = null)]
		public Type? LiteralTest(string code) {
			var lexer = new Lexer.Lexer(CMinusMinus.Lexicon);
			var parser = CompiledParser.Load("cmm.ptb");
			try {
				var tokens = lexer.Tokenize(code);
				Console.WriteLine(parser.Parse(tokens).ToString());
				return null;
			}
			catch (ParserException ex) {
				return ex.GetType();
			}
		}

		[TestCase(@"samples/0.cmm", ExpectedResult = null)]
		[TestCase(@"samples/a+b.cmm", ExpectedResult = null)]
		[TestCase(@"samples/hello.cmm", ExpectedResult = null)]
		[TestCase(@"samples/literal.cmm", ExpectedResult = null)]
		public Type? FileTest(string filePath) => LiteralTest(File.ReadAllText(filePath));
	}
}