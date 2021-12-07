using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using NUnit.Framework;
using Parser;
using Parser.LR;
using Parser.LR.CLR;
using TrueMogician.Extensions.Enumerable;

namespace CMinusMinus.Test {
	public class CompiledParserTests {
		public static CMinusMinus Language { get; } = new();

		[TestCase(ParserAlgorithm.CanonicalLR, false, 5)]
		[TestCase(ParserAlgorithm.CanonicalLR, true, 5)]
		[TestCase(ParserAlgorithm.GeneralizedLR, false, 5)]
		public void SaveTest(ParserAlgorithm algorithm, bool checkConflicts = false, int reportPeriod = 60) {
			static void Log(string message) => Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {message}");
			var itemSetsTimer = new Timer(reportPeriod * 1000) {AutoReset = true};
			var itemSetsCount = 0;
			if (algorithm == ParserAlgorithm.CanonicalLR) {
				IReadOnlyDictionary<ItemSet<Item>, IReadOnlyDictionary<Terminal, IAction>>? table = null;
				itemSetsTimer.Elapsed += (_, _) => Log($"Calculated item sets: {Language.CLRParser?.ParsingTable.ItemSets?.Count}");
				Language.CLRParser!.StartItemSetsCalculation += (_, _) => {
					Log("Item sets calculation started");
					itemSetsTimer.Start();
				};
				Language.CLRParser!.CompleteItemSetsCalculation += (_, args) => {
					itemSetsTimer.Stop();
					Log("Item sets calculation completed");
					itemSetsCount = args.Value!.Count;
				};
				var tableTimer = new Timer(reportPeriod * 1000) {AutoReset = true};
				tableTimer.Elapsed += (_, _) => Log($"Table calculation progress: {table!.Count}/{itemSetsCount}({100 * table!.Count / (decimal)itemSetsCount:##.00}%)");
				Language.CLRParser!.StartTableCalculation += (_, _) => {
					table = Language.CLRParser!.ParsingTable.ActionTable!.RawTable;
					Log("Table calculation started");
					tableTimer.Start();
				};
				Language.CLRParser!.CompleteTableCalculation += (_, _) => {
					tableTimer.Stop();
					Log("Table calculation completed");
				};
			}
			else {
				IReadOnlyDictionary<ItemSet<Item>, IReadOnlyDictionary<Terminal, List<IAction>>>? table = null;
				itemSetsTimer.Elapsed += (_, _) => Log($"Calculated item sets: {Language.GLRParser?.ParsingTable.ItemSets?.Count}");
				Language.GLRParser!.StartItemSetsCalculation += (_, _) => {
					Log("Item sets calculation started");
					itemSetsTimer.Start();
				};
				Language.GLRParser!.CompleteItemSetsCalculation += (_, args) => {
					itemSetsTimer.Stop();
					Log("Item sets calculation completed");
					itemSetsCount = args.Value!.Count;
				};
				var tableTimer = new Timer(reportPeriod * 1000) {AutoReset = true};
				tableTimer.Elapsed += (_, _) => Log($"Table calculation progress: {table!.Count}/{itemSetsCount}({100 * table!.Count / (decimal)itemSetsCount:##.00}%)");
				Language.GLRParser!.StartTableCalculation += (_, _) => {
					table = Language.GLRParser!.ParsingTable.ActionTable!.RawTable;
					Log("Table calculation started");
					tableTimer.Start();
				};
				Language.GLRParser!.CompleteTableCalculation += (_, _) => {
					tableTimer.Stop();
					Log("Table calculation completed");
				};
			}
			var startTime = DateTime.Now;
			Log("Started");
			Language.CreateParser(algorithm);
			Log("Parser created");
			Language.InitializeParser(algorithm);
			Language.CompileParser(algorithm);
			if (algorithm == ParserAlgorithm.CanonicalLR)
				Language.CompiledCLRParser!.Save("cmm.ptb");
			else
				Language.CompiledGLRParser!.Save("cmm.ptb");
			Console.WriteLine($"Time cost: {DateTime.Now - startTime}");
			Utilities.SendNotification("Parsing Table Compiled and Saved!", $"Time cost: {DateTime.Now - startTime}");
		}

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.LiteralSource), new object?[] {ParserAlgorithm.CanonicalLR, false}, Category = "CLR")]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.LiteralSource), new object?[] {ParserAlgorithm.GeneralizedLR, false}, Category = "GLR")]
		public Type? CompileAndRunLiteralTest(string code, ParserAlgorithm algorithm, bool checkConflicts = false) {
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
		public Type? CompileAndRunFileTest(string filePath, ParserAlgorithm algorithm, bool checkConflicts = false) => CompileAndRunLiteralTest(File.ReadAllText(filePath), algorithm, checkConflicts);

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.LiteralSource), new object?[] {ParserAlgorithm.CanonicalLR, null}, Category = "CLR")]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.LiteralSource), new object?[] {ParserAlgorithm.GeneralizedLR, null}, Category = "GLR")]
		public Type? LoadAndRunLiteralTest(string code, ParserAlgorithm algorithm) {
			Language.LoadCompiledParser(algorithm, "cmm.ptb");
			Language.SelectParser(algorithm, true);
			try {
				Console.WriteLine(Language.Parse(code).ToString());
				return null;
			}
			catch (ParserException ex) {
				if (ex is NotRecognizedException {CurrentStack: { }} e) {
					foreach (var node in e.CurrentStack.Reverse())
						Console.WriteLine(node.ToString());
					if (e.Tokens is not null && e.Position is not null)
						Console.WriteLine($"Next token: {e.Tokens.AsArray()[e.Position.Value]}");
				}
				return ex.GetType();
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.FileSource), new object?[] {ParserAlgorithm.CanonicalLR, null}, Category = "CLR")]
		[TestCaseSource(typeof(TestCases), nameof(TestCases.FileSource), new object?[] {ParserAlgorithm.GeneralizedLR, null}, Category = "GLR")]
		public Type? LoadAndRunFileTest(string filePath, ParserAlgorithm algorithm) => LoadAndRunLiteralTest(File.ReadAllText(filePath), algorithm);
	}
}