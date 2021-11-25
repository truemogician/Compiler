using System;
using System.Collections.Generic;
using System.Linq;
using Lexer;
using TrueMogician.Extensions.Enumerable;

#nullable enable
namespace Parser.LR {
	public class CompiledParsingTable {
		private readonly List<Terminal> _terminals;

		private readonly Dictionary<Lexeme, List<(Terminal, int)>> _groupedTerminals;

		private readonly List<Nonterminal> _nonterminals;

		private readonly List<(int NonterminalIndex, int Length)> _productionRules;

		private readonly int[,] _table;

		private CompiledParsingTable(IEnumerable<Terminal> terminals, IEnumerable<Nonterminal> nonterminals, IEnumerable<(int, int)> productionRules, int stateCount) {
			_terminals = terminals.AsList();
			int idx = _terminals.FindIndex(t => t.Equals(Terminal.Terminator));
			if (idx == -1)
				throw new Exception("Terminator not found in terminals");
			var tmp = _terminals[^1];
			_terminals[^1] = Terminal.Terminator;
			_terminals[idx] = tmp;
			_groupedTerminals = new Dictionary<Lexeme, List<(Terminal, int)>>();
			for (var i = 0; i < _terminals.Count; ++i) {
				var t = _terminals[i];
				if (!_groupedTerminals.ContainsKey(t.Lexeme))
					_groupedTerminals[t.Lexeme] = new List<(Terminal, int)>();
				_groupedTerminals[t.Lexeme].Add((t, i));
			}
			_nonterminals = nonterminals.AsList();
			_productionRules = productionRules.AsList();
			_table = new int[stateCount, _terminals.Count + _nonterminals.Count];
		}

		/// <summary>
		/// List of terminals. Terminator is always the last element.
		/// </summary>
		public IReadOnlyList<Terminal> Terminals => _terminals;

		/// <summary>
		/// List of nonterminals.
		/// </summary>
		public IReadOnlyList<Nonterminal> Nonterminals => _nonterminals;

		public IReadOnlyList<(int NonterminalIndex, int Length)> ProductionRules => _productionRules;

		/// <param name="stateIndex">Index of the state</param>
		/// <param name="symbolIndex">Index of the symbol. When less than Terminal.Count, the index indicate a terminal; Otherwise a nonterminal.</param>
		/// <returns>
		/// <para>If <paramref name="symbolIndex"/> indicates a terminal,
		/// the first item of return value will indicate the action type,
		/// and the second item stores the index of action-related content,
		/// which means the next state of a shift action, or the production rule to use of a reduce action.
		/// The second item of an accept action or error action should be 0, but it is not guaranteed.</para>
		/// <para>If <paramref name="symbolIndex"/> indicates a nonterminal,
		/// the first item of return value will always be null,
		/// while the second item stores the index of the state to go to.
		/// If no goto-state presents, the second item will be -1</para>
		/// </returns>
		public (ActionType? Action, int Index) this[int stateIndex, int symbolIndex] {
			get {
				if (stateIndex < 0 || stateIndex > _table.GetLength(0))
					throw new ArgumentOutOfRangeException(nameof(stateIndex));
				if (symbolIndex < 0 || symbolIndex > _terminals.Count + _nonterminals.Count)
					throw new ArgumentOutOfRangeException(nameof(symbolIndex));
				int value = _table[stateIndex, symbolIndex];
				return symbolIndex >= _terminals.Count ? (null, value - 1) : ((ActionType)(value & 0b11)!, value >> 2);
			}
			private set => _table[stateIndex, symbolIndex] = value.Action is null ? value.Index < 0 ? 0 : value.Index + 1 : (value.Index << 2) | (int)value.Action.Value;
		}

		public static CompiledParsingTable FromParsingTable<TItem>(ParsingTable<TItem> parsingTable) where TItem : ItemBase {
			var terminals = parsingTable.Grammar.Terminals.ToList();
			var tIndices = terminals.ToIndexDictionary();
			var nonterminals = parsingTable.Grammar.SourceNonterminals.ToList();
			var ntIndices = nonterminals.ToIndexDictionary();
			var productionRules = parsingTable.Grammar.Select(pr => (ntIndices[pr.Nonterminal], pr.Length)).ToList();
			var prIndices = parsingTable.Grammar.ToIndexDictionary();
			var stateIndices = parsingTable.ItemSets.ToIndexDictionary();
			int tmp = stateIndices[parsingTable.ItemSets.InitialState];
			stateIndices[parsingTable.ItemSets.InitialState] = 0;
			stateIndices[parsingTable.ItemSets.First()] = tmp;
			var result = new CompiledParsingTable(terminals, nonterminals, productionRules, parsingTable.ItemSets.Count);
			foreach (var (state, actions) in parsingTable.ActionTable.Table) {
				foreach (var (terminal, action) in actions)
					result[stateIndices[state], tIndices[terminal]] = (action.Type, action switch {
						ShiftAction<TItem> sa => stateIndices[sa.NextState],
						ReduceAction ra       => prIndices[ra.ProductionRule],
						_                     => 0
					});
			}
			foreach (var (state, states) in parsingTable.GotoTable.Table) {
				foreach (var (nt, st) in states)
					result[stateIndices[state], ntIndices[nt] + terminals.Count] = (null, st is null ? -1 : stateIndices[st]);
			}
			return result;
		}

		public static CompiledParsingTable Load(string path) => throw new NotImplementedException();

		public void Save(string path) => throw new NotImplementedException();

		public int? Match(Token token, bool checkAmbiguity = false) {
			if (!_groupedTerminals.ContainsKey(token.Lexeme))
				return null;
			return (checkAmbiguity
				? _groupedTerminals[token.Lexeme].SingleOrDefault(t => t.Item1.Match(token))
				: _groupedTerminals[token.Lexeme].FirstOrDefault(t => t.Item1.Match(token))).Item2;
		}
	}
}