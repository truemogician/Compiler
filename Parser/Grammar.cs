using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parser {
	public class Grammar<TNonterminal, TToken> : ICollection<ProductionRule<TNonterminal, TToken>> where TNonterminal : struct, Enum where TToken : struct, Enum {
		private readonly List<ProductionRule<TNonterminal, TToken>> _rules = new();

		public int Count => _rules.Count;

		public bool IsReadOnly => false;

		public IEnumerator<ProductionRule<TNonterminal, TToken>> GetEnumerator() => _rules.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(ProductionRule<TNonterminal, TToken> item) => _rules.Add(item);

		public void Clear() => _rules.Clear();

		public bool Contains(ProductionRule<TNonterminal, TToken> item) => _rules.Contains(item);

		public void CopyTo(ProductionRule<TNonterminal, TToken>[] array, int arrayIndex) => _rules.CopyTo(array, arrayIndex);

		public bool Remove(ProductionRule<TNonterminal, TToken> item) => _rules.Remove(item);

		public void AddProductionRule(TNonterminal nonTerminal, IEnumerable<SentenceForm<TNonterminal, TToken>> productions) {
			foreach (var sentence in productions)
				_rules.Add(new ProductionRule<TNonterminal, TToken>(nonTerminal, sentence));
		}

		public void AddProductionRule(TNonterminal nonTerminal, params SentenceForm<TNonterminal, TToken>[] productions) => AddProductionRule(nonTerminal, productions.AsEnumerable());
	}
}