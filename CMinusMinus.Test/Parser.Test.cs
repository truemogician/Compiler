using System;
using System.IO;
using NUnit.Framework;
using Parser;

namespace CMinusMinus.Test {
	public class ParserTests {
		public static readonly CMinusMinus Language = new();

		[TestCase(@"int main(){}", ExpectedResult = null)]
		public Type? LiteralTest(string code) {
			try {
				Console.WriteLine(Language.Parse(code).ToString());
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