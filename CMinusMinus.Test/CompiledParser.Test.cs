﻿using System;
using System.IO;
using NUnit.Framework;
using Parser;

namespace CMinusMinus.Test {
	public class CompiledParserTests {
		[Test]
		public void SaveTest() {
			var language = new CMinusMinus();
			var compiled = language.RawParser!.Compile();
			compiled.Save("cmm.ptb");
		}

		[TestCase("int main(){}", ExpectedResult = null)]
		public Type? CompileAndRunLiteralTest(string code) {
			var language = new CMinusMinus();
			language.UseCompiledParser();
			try {
				Console.WriteLine(language.Parse(code).ToString());
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
		public Type? CompileAndRunFileTest(string filePath) => CompileAndRunLiteralTest(File.ReadAllText(filePath));

		[TestCase("int main(){}", ExpectedResult = null)]
		public Type? LoadAndRunLiteralTest(string code) {
			var language = new CMinusMinus("cmm.ptb");
			try {
				Console.WriteLine(language.Parse(code).ToString());
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
		public Type? LoadAndRunFileTest(string filePath) => LoadAndRunLiteralTest(File.ReadAllText(filePath));
	}
}