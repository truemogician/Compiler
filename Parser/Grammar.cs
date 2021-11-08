using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parser {
	public class Grammar<TNonterminal, TTerminal> : ICollection<ProductionRule<TNonterminal, TTerminal>> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		private readonly List<ProductionRule<TNonterminal, TTerminal>> _rules = new();

		public int Count => _rules.Count;

		public bool IsReadOnly => false;

		public IEnumerator<ProductionRule<TNonterminal, TTerminal>> GetEnumerator() => _rules.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(ProductionRule<TNonterminal, TTerminal> item) => _rules.Add(item);

		public void Clear() => _rules.Clear();

		public bool Contains(ProductionRule<TNonterminal, TTerminal> item) => _rules.Contains(item);

		public void CopyTo(ProductionRule<TNonterminal, TTerminal>[] array, int arrayIndex) => _rules.CopyTo(array, arrayIndex);

		public bool Remove(ProductionRule<TNonterminal, TTerminal> item) => _rules.Remove(item);

		public void AddProductionRule(TNonterminal nonTerminal, IEnumerable<SentenceForm<TNonterminal, TTerminal>> sentenceForms) {
			foreach (var sentence in sentenceForms)
				_rules.Add(new ProductionRule<TNonterminal, TTerminal>(nonTerminal, sentence));
		}

		public void AddProductionRule(TNonterminal nonTerminal, params SentenceForm<TNonterminal, TTerminal>[] sentenceForms) => AddProductionRule(nonTerminal, sentenceForms.AsEnumerable());
	}
}