using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Parser {
	public class Grammar : ICollection<ProductionRule> {
		private readonly Dictionary<Nonterminal, List<ProductionRule>> _rules = new();

		public Grammar(Nonterminal initialState) => InitialState = initialState;

		public Grammar(Grammar grammar) {
			foreach (var (nonterminal, rules) in grammar._rules)
				_rules.Add(nonterminal, new List<ProductionRule>(rules));
			InitialState = grammar.InitialState;
		}

		public int Count => _rules.Count;

		public bool IsReadOnly => false;

		public IEnumerable<Nonterminal> SourceNonterminals => _rules.Keys;

		public IEnumerable<Nonterminal> Nonterminals => _rules.SelectMany(pair => pair.Value.SelectMany(pr => pr.InvolvedNonterminals).Append(pair.Key)).Distinct();

		public IEnumerable<Terminal> Terminals => _rules.SelectMany(pair => pair.Value.SelectMany(pr => pr.InvolvedTerminals)).Distinct();

		public Nonterminal? InitialState { get; set; }

		public IEnumerator<ProductionRule> GetEnumerator() => _rules.Values.SelectMany(rules => rules).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(ProductionRule rule) {
			if (!_rules.ContainsKey(rule.Nonterminal))
				_rules[rule.Nonterminal] = new List<ProductionRule>();
			_rules[rule.Nonterminal].Add(rule);
		}

		public void Clear() => _rules.Clear();

		public bool Contains(ProductionRule rule) => _rules.ContainsKey(rule.Nonterminal) && _rules[rule.Nonterminal].Contains(rule);

		public void CopyTo(ProductionRule[] array, int arrayIndex) => throw new NotSupportedException();

		public bool Remove(ProductionRule rule) {
			bool result = _rules.ContainsKey(rule.Nonterminal) && _rules[rule.Nonterminal].Remove(rule);
			if (result && _rules[rule.Nonterminal].Count == 0)
				_rules.Remove(rule.Nonterminal);
			return result;
		}

		public void AddProductionRule(Nonterminal nonterminal, IEnumerable<SentenceForm> productions) {
			if (!_rules.ContainsKey(nonterminal))
				_rules.Add(nonterminal, new List<ProductionRule>());
			_rules[nonterminal].AddRange(productions.Select(p => new ProductionRule(nonterminal, p)));
		}

		public void AddProductionRule(Nonterminal nonterminal, params SentenceForm[] productions) => AddProductionRule(nonterminal, productions.AsEnumerable());

		public void Simplify() => throw new NotImplementedException();

		public IReadOnlyList<ProductionRule> this[Nonterminal index] => _rules.ContainsKey(index) ? _rules[index] : throw new KeyNotFoundException();
	}
}