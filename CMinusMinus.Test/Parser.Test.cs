using System;
using System.IO;
using NUnit.Framework;
using Parser;

namespace CMinusMinus.Test {
	public class ParserTests {
		public static CMinusMinus Language { get; } = new();

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.LiteralSource), new object?[] {ParserAlgorithm.CanonicalLR, false}, Category = "CLR")]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.LiteralSource), new object?[] {ParserAlgorithm.GeneralizedLR, false}, Category = "GLR")]
		public Type? LiteralTest(string code, ParserAlgorithm algorithm, bool checkConflicts = false) {
			Language.CreateParser(algorithm);
			Language.InitializeParser(algorithm, checkConflicts);
			Language.SelectParser(algorithm, false);
			try {
				Console.WriteLine(Language.Parse(code).ToString());
				return null;
			}
			catch (ParserException ex) {
				return ex.GetType();
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.FileSource), new object?[] {ParserAlgorithm.CanonicalLR, false}, Category = "CLR")]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.FileSource), new object?[] {ParserAlgorithm.GeneralizedLR, false}, Category = "GLR")]
		public Type? FileTest(string filePath, ParserAlgorithm algorithm, bool checkConflicts = false) => LiteralTest(File.ReadAllText(filePath), algorithm, checkConflicts);
	}
}