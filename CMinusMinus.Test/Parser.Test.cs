using System;
using System.IO;
using NUnit.Framework;
using Parser;

namespace CMinusMinus.Test {
	public class ParserTests {
		private static CMinusMinus? _language;

		public static CMinusMinus Language {
			get {
				if (_language is null) {
					_language = new CMinusMinus();
					_language.InitializeRawParser();
				}
				return _language;
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.LiteralSource))]
		public Type? LiteralTest(string code) {
			try {
				Console.WriteLine(Language.Parse(code).ToString());
				return null;
			}
			catch (ParserException ex) {
				return ex.GetType();
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.FileSource))]
		public Type? FileTest(string filePath) => LiteralTest(File.ReadAllText(filePath));
	}
}