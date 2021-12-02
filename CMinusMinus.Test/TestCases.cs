using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CMinusMinus.Test {
	public static class TestCases {
		public static TestCaseData[] LiteralSource { get; } = {
			new(@"int main(){}") {ExpectedResult = null}
		};

		public static TestCaseData[] FileSource { get; } = Directory.GetFiles("samples", "*.cmm").Select(path => new TestCaseData(path) {ExpectedResult = null}).ToArray();
	}
}