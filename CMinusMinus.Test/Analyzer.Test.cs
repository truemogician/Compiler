using System;
using System.IO;
using CMinusMinus.Analyzers.SyntaxComponents;
using NUnit.Framework;

namespace CMinusMinus.Test {
	public class AnalyzerTests {
		public static CMinusMinus Language { get; } = new();

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.LiteralSource), new object?[] { ParserAlgorithm.GeneralizedLR, null }, Category = "GLR")]
		public Type? LoadAndRunLiteralTest(string code, ParserAlgorithm algorithm) {
			Language.LoadCompiledParser(algorithm, CompiledParserTests.TableFilePath);
			Language.SelectParser(algorithm, true);
			try {
				foreach (var error in Language.Analyze(code))
					Console.WriteLine(error.ToString());
				return null;
			}
			catch (Exception ex) {
				return ex.GetType();
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.FileSource), new object?[] { ParserAlgorithm.GeneralizedLR, null }, Category = "GLR")]
		public Type? LoadAndRunFileTest(string filePath, ParserAlgorithm algorithm) => LoadAndRunLiteralTest(File.ReadAllText(filePath), algorithm);

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.FileSource), new object?[] { ParserAlgorithm.GeneralizedLR, null }, Category = "GLR")]
		public Type? SyntaxComponentTest(string filePath, ParserAlgorithm algorithm) {
			string code = File.ReadAllText(filePath);
			Language.LoadCompiledParser(algorithm, CompiledParserTests.TableFilePath);
			Language.SelectParser(algorithm, true);
			var tree = Language.Parse(code);
			tree.Clean();
			try {
				var program = new Program(tree);
				Console.WriteLine(program);
			}
			catch (Exception ex) {
				return ex.GetType();
			}
			return null;
		}
	}
}