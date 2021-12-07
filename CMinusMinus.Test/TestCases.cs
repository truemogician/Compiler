using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CMinusMinus.Test {
	public static class TestCases {
		public static string[] Literals { get; } = {
			@"int main(){}"
		};

		public static string[] Files { get; } = Directory.GetFiles("samples", "*.cmm");

		public static TestCaseData[] LiteralSource(ParserAlgorithm algorithm, bool? checkConflicts)
			=> Literals.Select(
					literal => {
						var testCase = checkConflicts is null ? new TestCaseData(literal, algorithm) : new TestCaseData(literal, algorithm, checkConflicts.Value);
						return testCase.Returns(null);
					}
				)
				.ToArray();

		public static TestCaseData[] FileSource(ParserAlgorithm algorithm, bool? checkConflicts)
			=> Files.Select(
					literal => {
						var testCase = checkConflicts is null ? new TestCaseData(literal, algorithm) : new TestCaseData(literal, algorithm, checkConflicts.Value);
						return testCase.Returns(null);
					}
				)
				.ToArray();
	}
}