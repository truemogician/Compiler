using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Parser {
	public class Grammar<TNonterminal, TToken> : ICollection<ProductionRule<TNonterminal, TToken>> where TNonterminal : struct, Enum where TToken : struct, Enum {
		private readonly List<ProductionRule<TNonterminal, TToken>> _rules = new();

		public Grammar(TNonterminal initialState) => InitialState = initialState;

		public int Count => _rules.Count;

		public bool IsReadOnly => false;

		public IEnumerable<TNonterminal> Nonterminals => _rules.SelectMany(r => r.InvolvedNonterminals).Distinct();

		public IEnumerable<Terminal<TToken>> Terminals => _rules.SelectMany(r => r.InvolvedTerminals).Distinct();

		public TNonterminal InitialState { get; set; }

		public IEnumerator<ProductionRule<TNonterminal, TToken>> GetEnumerator() => _rules.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(ProductionRule<TNonterminal, TToken> rule) => _rules.Add(rule);

		public void Clear() => _rules.Clear();

		public bool Contains(ProductionRule<TNonterminal, TToken> rule) => _rules.Contains(rule);

		public void CopyTo(ProductionRule<TNonterminal, TToken>[] array, int arrayIndex) => _rules.CopyTo(array, arrayIndex);

		public bool Remove(ProductionRule<TNonterminal, TToken> rule) => _rules.Remove(rule);

		public void AddProductionRule(TNonterminal nonTerminal, IEnumerable<SentenceForm<TNonterminal, TToken>> productions) {
			foreach (var sentence in productions)
				_rules.Add(new ProductionRule<TNonterminal, TToken>(nonTerminal, sentence));
		}

		public void AddProductionRule(TNonterminal nonTerminal, params SentenceForm<TNonterminal, TToken>[] productions) => AddProductionRule(nonTerminal, productions.AsEnumerable());
	}
}