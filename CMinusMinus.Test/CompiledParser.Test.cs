using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Timers;
using Microsoft.Toolkit.Uwp.Notifications;
using NUnit.Framework;
using Parser;
using Parser.LR;
using Parser.LR.CLR;

namespace CMinusMinus.Test {
	public class CompiledParserTests {
		[TestCase("Title", "line1", "line2")]
		public void SendNotification(string title, params string[] contents) {
			var builder = new ToastContentBuilder().AddText(title);
			foreach (var content in contents)
				builder.AddText(content);
			try {
				builder.Show();
			}
			catch { }
		}

		[TestCase(5)]
		public void SaveTest(int reportPeriod) {
			var startTime = DateTime.Now;
			Console.WriteLine($"Time started: {startTime}");
			var language = new CMinusMinus();
			static void Log(string message) => Debug.WriteLine($"{DateTime.Now:s} {message}");
			var itemSetsTimer = new Timer(reportPeriod * 1000) {AutoReset = true};
			var itemSetsCount = 0;
			IReadOnlyDictionary<ItemSet<Item>, Dictionary<Terminal, IAction>>? table = null;
			itemSetsTimer.Elapsed += (_, _) => Log($"Calculated item sets: {language.RawParser?.ParsingTable.ItemSets?.Count}");
			language.RawParser!.StartItemSetsCalculation += (_, _) => {
				Log("Item sets calculation started");
				itemSetsTimer.Start();
			};
			language.RawParser!.CompleteItemSetsCalculation += (_, args) => {
				itemSetsTimer.Stop();
				Log("Item sets calculation completed");
				itemSetsCount = args.Value!.Count;
			};
			var tableTimer = new Timer(reportPeriod * 1000) {AutoReset = true};
			tableTimer.Elapsed += (_, _) => Log($"Table calculation progress: {table!.Count}/{itemSetsCount}({100 * table!.Count / (decimal)itemSetsCount:##.00}%)");
			language.RawParser!.StartTableCalculation += (_, _) => {
				table = language.RawParser!.ParsingTable.ActionTable!.RawTable;
				Log("Table calculation started");
				tableTimer.Start();
			};
			language.RawParser!.CompleteTableCalculation += (_, _) => {
				tableTimer.Stop();
				Log("Table calculation completed");
			};
			language.InitializeRawParser();
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