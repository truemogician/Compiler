using System;
using System.IO;
using NUnit.Framework;
using Parser;
using Microsoft.Toolkit.Uwp.Notifications;

namespace CMinusMinus.Test {
	public class CompiledParserTests {
		[TestCase("Title", "line1", "line2")]
		public void SendNotification(string title, params string[] contents) {
			var builder = new ToastContentBuilder().AddText(title);
			foreach (var content in contents)
				builder.AddText(content);
			builder.Show();
		}

		[Test]
		public void SaveTest() {
			var startTime = DateTime.Now;
			Console.WriteLine($"Time started: {startTime}");
			var language = new CMinusMinus();
			var compiled = language.RawParser!.Compile();
			compiled.Save("cmm.ptb");
			Console.WriteLine($"Time cost: {DateTime.Now - startTime}");
			SendNotification("Parsing Table Compiled and Saved!", $"Time cost: {DateTime.Now - startTime}");
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
		[TestCase(@"samples/full.cmm", ExpectedResult = null)]
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
		[TestCase(@"samples/full.cmm", ExpectedResult = null)]
		public Type? LoadAndRunFileTest(string filePath) => LoadAndRunLiteralTest(File.ReadAllText(filePath));
	}
}