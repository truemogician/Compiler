using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lexer;
using TrueMogician.Extensions.Enumerable;

namespace Parser.LR.GLR {
	public class CompiledParsingTable {
		public class ActionTableWrapper {
			internal readonly List<int>?[,] Table;

			internal ActionTableWrapper(List<int>?[,] table) => Table = table;

			/// <summary>
			/// </summary>
			/// <param name="stateIndex">Index of the state</param>
			/// <param name="terminalIndex">Index of the terminal</param>
			/// <returns>
			///     The first item of an item in the return value will indicate the action type,
			///     and the second stores the index of action-related content,
			///     which means the next state of a shift action, or the production rule to use of a reduce action.
			///     The second item of an accept action or error action should be 0, but it is not guaranteed.
			/// </returns>
			public (ActionType Action, int Index)[]? this[int stateIndex, int terminalIndex] {
				get {
					if (stateIndex < 0 || stateIndex > Table.GetLength(0))
						throw new ArgumentOutOfRangeException(nameof(stateIndex));
					if (terminalIndex < 0 || terminalIndex > Table.GetLength(1))
						throw new ArgumentOutOfRangeException(nameof(terminalIndex));
					var value = Table[stateIndex, terminalIndex];
					return value?.Select(v => ((ActionType)(v & 0b11), v >> 2)).ToArray();
				}
				internal set => Table[stateIndex, terminalIndex] = value?.Select(v => (v.Index << 2) | (int)v.Action).ToList();
			}
		}

		public class GotoTableWrapper {
			internal readonly int[,] Table;

			internal GotoTableWrapper(int[,] table) => Table = table;

			/// <param name="stateIndex">Index of the state</param>
			/// <param name="nonterminalIndex">Index of the nonterminal</param>
			/// <returns>
			///     The index of the state to go to.
			///     If no goto-state presents, the result will be -1
			/// </returns>
			public int this[int stateIndex, int nonterminalIndex] {
				get {
					if (stateIndex < 0 || stateIndex > Table.GetLength(0))
						throw new ArgumentOutOfRangeException(nameof(stateIndex));
					if (nonterminalIndex < 0 || nonterminalIndex > Table.GetLength(1))
						throw new ArgumentOutOfRangeException(nameof(nonterminalIndex));
					return Table[stateIndex, nonterminalIndex] - 1;
				}
				internal set => Table[stateIndex, nonterminalIndex] = value < 0 ? 0 : value + 1;
			}
		}

		private readonly Dictionary<Lexeme, List<(Terminal, int)>> _groupedTerminals;

		private readonly List<Nonterminal> _nonterminals;

		private readonly List<(int NonterminalIndex, int Length)> _productionRules;

		private readonly List<Terminal> _terminals;

		private CompiledParsingTable(IEnumerable<Terminal> terminals, IEnumerable<Nonterminal> nonterminals, IEnumerable<(int, int)> productionRules, List<int>?[,] actionTable, int[,] gotoTable) {
			_terminals = terminals.AsList();
			_groupedTerminals = new Dictionary<Lexeme, List<(Terminal, int)>>();
			for (var i = 0; i < _terminals.Count; ++i) {
				var t = _terminals[i];
				if (!_groupedTerminals.ContainsKey(t.Lexeme))
					_groupedTerminals[t.Lexeme] = new List<(Terminal, int)>();
				_groupedTerminals[t.Lexeme].Add((t, i));
			}
			_nonterminals = nonterminals.AsList();
			_productionRules = productionRules.AsList();
			ActionTable = new ActionTableWrapper(actionTable);
			GotoTable = new GotoTableWrapper(gotoTable);
		}

		private CompiledParsingTable(ICollection<Terminal> terminals, ICollection<Nonterminal> nonterminals, IEnumerable<(int, int)> productionRules, int stateCount)
			: this(terminals, nonterminals, productionRules, new List<int>?[stateCount, terminals.Count + 1], new int[stateCount, nonterminals.Count]) { }

		public ActionTableWrapper ActionTable { get; }

		public GotoTableWrapper GotoTable { get; }

		public IReadOnlyList<Terminal> Terminals => _terminals;

		public IReadOnlyList<Nonterminal> Nonterminals => _nonterminals;

		public IReadOnlyList<(int NonterminalIndex, int Length)> ProductionRules => _productionRules;

		public static CompiledParsingTable FromParsingTable<TItem>(ParsingTableBase<TItem, List<IAction>> parsingTable) where TItem : ItemBase {
			if (!parsingTable.Initialized)
				throw new ArgumentException("Parsing table not initialized", nameof(parsingTable));
			var terminals = parsingTable.Grammar.Terminals.ToList();
			if (terminals.Contains(Terminal.Terminator))
				throw new ArgumentException("Terminator should not present in terminal collection");
			var tIndices = terminals.ToIndexDictionary();
			var nonterminals = parsingTable.Grammar.SourceNonterminals.ToList();
			var ntIndices = nonterminals.ToIndexDictionary();
			var productionRules = parsingTable.Grammar.Select(pr => (ntIndices[pr.Nonterminal], pr.Length)).ToList();
			var prIndices = parsingTable.Grammar.ToIndexDictionary();
			var stateIndices = parsingTable.ItemSets!.ToIndexDictionary();
			int tmp = stateIndices[parsingTable.ItemSets!.InitialState];
			stateIndices[parsingTable.ItemSets.InitialState] = 0;
			stateIndices[parsingTable.ItemSets.First()] = tmp;
			var result = new CompiledParsingTable(terminals, nonterminals, productionRules, parsingTable.ItemSets.Count);
			foreach (var (state, actions) in parsingTable.ActionTable!.RawTable) {
				foreach (var (terminal, action) in actions) {
					int idx = terminal.Equals(Terminal.Terminator) ? terminals.Count : tIndices[terminal];
					result.ActionTable[stateIndices[state], idx] = action.Select(
							act => (act.Type, act switch {
								ShiftAction<TItem> sa => stateIndices[sa.NextState],
								ReduceAction ra       => prIndices[ra.ProductionRule],
								_                     => 0
							})
						)
						.ToArray();
				}
			}
			foreach (var (state, states) in parsingTable.GotoTable!.RawTable) {
				foreach (var (nt, st) in states)
					result.GotoTable[stateIndices[state], ntIndices[nt] + terminals.Count + 1] = st is null ? -1 : stateIndices[st];
			}
			return result;
		}

		public static CompiledParsingTable Load(string path) {
			var reader = new StringReader(File.ReadAllText(path));
			bool ReadBool() {
				var ch = (char)reader!.Read();
				return ch != '0' && (ch == '1' ? true : throw new Exception());
			}
			try {
				int[] counts = reader.ReadLine()!.Split(' ').Select(int.Parse).ToArray();
				string line;
				var separatorMatcher = new Regex(@"(?<!\\),", RegexOptions.Compiled);
				var lexemes = new Lexeme[counts[0]];
				for (var i = 0; i < counts[0]; ++i) {
					bool useRegex = ReadBool();
					line = reader.ReadLine()!;
					int idx = separatorMatcher.Match(line).Index;
					string name = line[..idx].Replace(@"\,", ",");
					lexemes[i] = useRegex ? new Lexeme(name, new Regex(line[(idx + 1)..])) : new Lexeme(name, line[(idx + 1)..]);
				}
				var terminals = new Terminal[counts[1]];
				for (var i = 0; i < counts[1]; ++i) {
					int idx = reader.ReadInteger()!.Value;
					if (reader.Peek() == ',') {
						reader.Read();
						bool useRegex = ReadBool();
						line = reader.ReadLine()!;
						terminals[i] = useRegex ? new Terminal(lexemes[idx], new Regex(line)) : new Terminal(lexemes[idx], line);
					}
					else {
						terminals[i] = new Terminal(lexemes[idx]);
						reader.ReadLine();
					}
				}
				var nonterminals = new Nonterminal[counts[2]];
				for (var i = 0; i < counts[2]; ++i) {
					bool tmp = ReadBool();
					nonterminals[i] = new Nonterminal(reader.ReadLine()!, tmp);
				}
				var productionRules = new (int NonterminalIndex, int Length)[counts[3]];
				for (var i = 0; i < counts[3]; ++i) {
					int idx = reader.ReadInteger()!.Value;
					if (reader.Read() != ',')
						throw new Exception();
					int length = reader.ReadInteger()!.Value;
					productionRules[i] = (idx, length);
					reader.ReadLine();
				}
				var actionTable = new List<int>?[counts[4], counts[1] + 1];
				for (var i = 0; i < counts[4]; ++i) {
					for (var j = 0; j < counts[1] + 1; ++j) {
						if (reader.Peek() == ',')
							actionTable[i, j] = null;
						else {
							var list = new List<int>();
							while (true) {
								list.Add(reader.ReadInteger()!.Value);
								if (reader.Peek() == ',')
									break;
								reader.Read();
							}
							actionTable[i, j] = list;
						}
						reader.Read();
					}
					reader.ReadLine();
				}
				var gotoTable = new int[counts[4], counts[2]];
				for (var i = 0; i < counts[4]; ++i) {
					for (var j = 0; j < counts[2]; ++j) {
						gotoTable[i, j] = reader.Peek() == ',' ? 0 : reader.ReadInteger()!.Value;
						reader.Read();
					}
					reader.ReadLine();
				}
				return new CompiledParsingTable(terminals, nonterminals, productionRules, actionTable, gotoTable);
			}
			catch (Exception exception) {
				throw new FormatException("Wrong table file format", exception);
			}
		}

		public void Save(string path) {
			var builder = new StringBuilder();
			builder.AppendLine(string.Join(' ', new[] {_groupedTerminals.Count, _terminals.Count, _nonterminals.Count, _productionRules.Count, ActionTable.Table.GetLength(0)}));
			foreach (var lexeme in _groupedTerminals.Keys) {
				builder.Append(lexeme.UseRegex ? '1' : '0');
				builder.Append(lexeme.Name.Replace(",", @"\,"));
				builder.Append(',');
				builder.AppendLine(lexeme.UseRegex ? lexeme.Pattern[4..^1] : lexeme.Pattern);
			}
			var lexemeIndices = _groupedTerminals.Keys.ToIndexDictionary();
			foreach (var terminal in _terminals) {
				builder.Append(lexemeIndices[terminal.Lexeme]);
				if (terminal.Pattern is not null) {
					builder.Append(',');
					builder.Append(terminal.UseRegex ? '1' : '0');
					builder.Append(terminal.Pattern);
				}
				builder.AppendLine();
			}
			foreach ((var name, bool temporary) in _nonterminals) {
				builder.Append(temporary ? '1' : '0');
				builder.AppendLine(name);
			}
			foreach (var (idx, length) in _productionRules)
				builder.AppendLine($"{idx},{length}");
			for (var i = 0; i < ActionTable.Table.GetLength(0); ++i) {
				for (var j = 0; j < ActionTable.Table.GetLength(1); ++j) {
					if (ActionTable.Table[i, j] is { } value)
						builder.Append(string.Join(';', value));
					builder.Append(',');
				}
				builder.AppendLine();
			}
			for (var i = 0; i < GotoTable.Table.GetLength(0); ++i) {
				for (var j = 0; j < GotoTable.Table.GetLength(1); ++j) {
					if (GotoTable.Table[i, j] is var value and >= 0)
						builder.Append(value);
					builder.Append(',');
				}
				builder.AppendLine();
			}
			File.WriteAllText(path, builder.ToString());
		}

		public int? Match(Token token, bool checkAmbiguity = false) {
			if (!_groupedTerminals.ContainsKey(token.Lexeme))
				return null;
			return (checkAmbiguity
				? _groupedTerminals[token.Lexeme].SingleOrDefault(t => t.Item1.Match(token))
				: _groupedTerminals[token.Lexeme].FirstOrDefault(t => t.Item1.Match(token))).Item2;
		}
	}
}