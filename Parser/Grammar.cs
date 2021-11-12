using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Parser {
	public class Grammar : ICollection<ProductionRule> {
		private readonly List<ProductionRule> _rules = new();

		public Grammar(Nonterminal initialState) => InitialState = initialState;

		public int Count => _rules.Count;

		public bool IsReadOnly => false;

		public IEnumerable<Nonterminal> Nonterminals => _rules.SelectMany(r => r.InvolvedNonterminals).Distinct();

		public IEnumerable<Terminal> Terminals => _rules.SelectMany(r => r.InvolvedTerminals).Distinct();

		public Nonterminal InitialState { get; set; }

		public IEnumerator<ProductionRule> GetEnumerator() => _rules.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(ProductionRule rule) => _rules.Add(rule);

		public void Clear() => _rules.Clear();

		public bool Contains(ProductionRule rule) => _rules.Contains(rule);

		public void CopyTo(ProductionRule[] array, int arrayIndex) => _rules.CopyTo(array, arrayIndex);

		public bool Remove(ProductionRule rule) => _rules.Remove(rule);

		public void AddProductionRule(Nonterminal nonTerminal, IEnumerable<SentenceForm> productions) {
			foreach (var sentence in productions)
				_rules.Add(new ProductionRule(nonTerminal, sentence));
		}

		public void AddProductionRule(Nonterminal nonTerminal, params SentenceForm[] productions) => AddProductionRule(nonTerminal, productions.AsEnumerable());
	}
}