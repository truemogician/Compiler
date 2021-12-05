using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lexer;
using TrueMogician.Extensions.Enumerable;

namespace Parser.LR.CLR {
	public class CompiledParsingTable {
		private readonly Dictionary<Lexeme, List<(Terminal, int)>> _groupedTerminals;

		private readonly List<Nonterminal> _nonterminals;

		private readonly List<(int NonterminalIndex, int Length)> _productionRules;

		private readonly int[,] _table;

		private readonly List<Terminal> _terminals;

		private CompiledParsingTable(IEnumerable<Terminal> terminals, IEnumerable<Nonterminal> nonterminals, IEnumerable<(int, int)> productionRules, int[,] table) {
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
			_table = table;
		}

		private CompiledParsingTable(ICollection<Terminal> terminals, ICollection<Nonterminal> nonterminals, IEnumerable<(int, int)> productionRules, int stateCount)
			: this(terminals, nonterminals, productionRules, new int[stateCount, terminals.Count + nonterminals.Count + 1]) { }

		public IReadOnlyList<Terminal> Terminals => _terminals;

		public IReadOnlyList<Nonterminal> Nonterminals => _nonterminals;

		public IReadOnlyList<(int NonterminalIndex, int Length)> ProductionRules => _productionRules;

		/// <param name="stateIndex">Index of the state</param>
		/// <param name="symbolIndex">
		///     Index of the symbol.
		///     When less than or equal to Terminal.Count, the index indicate a terminal
		///     (specifically, when equal to Terminal.Count, it indicate the Terminator);
		///     Otherwise a nonterminal.
		/// </param>
		/// <returns>
		///     <para>
		///         If <paramref name="symbolIndex" /> indicates a terminal,
		///         the first item of return value will indicate the action type,
		///         and the second item stores the index of action-related content,
		///         which means the next state of a shift action, or the production rule to use of a reduce action.
		///         The second item of an accept action or error action should be 0, but it is not guaranteed.
		///     </para>
		///     <para>
		///         If <paramref name="symbolIndex" /> indicates a nonterminal,
		///         the first item of return value will always be null,
		///         while the second item stores the index of the state to go to.
		///         If no goto-state presents, the second item will be -1
		///     </para>
		/// </returns>
		public (ActionType? Action, int Index) this[int stateIndex, int symbolIndex] {
			get {
				if (stateIndex < 0 || stateIndex > _table.GetLength(0))
					throw new ArgumentOutOfRangeException(nameof(stateIndex));
				if (symbolIndex < 0 || symbolIndex > _terminals.Count + _nonterminals.Count + 1)
					throw new ArgumentOutOfRangeException(nameof(symbolIndex));
				int value = _table[stateIndex, symbolIndex];
				return symbolIndex > _terminals.Count ? (null, value - 1) : ((ActionType)(value & 0b11)!, value >> 2);
			}
			private set => _table[stateIndex, symbolIndex] = value.Action is null ? value.Index < 0 ? 0 : value.Index + 1 : (value.Index << 2) | (int)value.Action.Value;
		}

		public static CompiledParsingTable FromParsingTable<TItem>(ParsingTableBase<TItem, IAction> parsingTable) where TItem : ItemBase {
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
					result[stateIndices[state], idx] = (action.Type, action switch {
						ShiftAction<TItem> sa => stateIndices[sa.NextState],
						ReduceAction ra       => prIndices[ra.ProductionRule],
						_                     => 0
					});
				}
			}
			foreach (var (state, states) in parsingTable.GotoTable!.RawTable) {
				foreach (var (nt, st) in states)
					result[stateIndices[state], ntIndices[nt] + terminals.Count + 1] = (null, st is null ? -1 : stateIndices[st]);
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
				var table = new int[counts[4], counts[1] + counts[2] + 1];
				for (var i = 0; i < counts[4]; ++i) {
					for (var j = 0; j < counts[1] + counts[2] + 1; ++j) {
						table[i, j] = reader.Peek() == ',' ? 0 : reader.ReadInteger()!.Value;
						reader.Read();
					}
					reader.ReadLine();
				}
				return new CompiledParsingTable(terminals, nonterminals, productionRules, table);
			}
			catch (Exception exception) {
				throw new FormatException("Wrong table file format", exception);
			}
		}

		public void Save(string path) {
			var builder = new StringBuilder();
			builder.AppendLine(string.Join(' ', new[] {_groupedTerminals.Count, _terminals.Count, _nonterminals.Count, _productionRules.Count, _table.GetLength(0)}));
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
			for (var i = 0; i < _table.GetLength(0); ++i) {
				for (var j = 0; j < _table.GetLength(1); ++j) {
					if (_table[i, j] != 0)
						builder.Append(_table[i, j]);
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