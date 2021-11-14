using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexer;

#nullable enable
namespace Parser {
	public class Grammar : ICollection<ProductionRule> {
		private readonly Dictionary<Nonterminal, List<ProductionRule>> _rules = new();

		private readonly Dictionary<Token, HashSet<TerminalCount>> _terminals = new();

		public Grammar(Nonterminal initialState) => InitialState = initialState;

		public Grammar(Grammar grammar) {
			foreach (var (nonterminal, rules) in grammar._rules)
				_rules.Add(nonterminal, new List<ProductionRule>(rules));
			InitialState = grammar.InitialState;
		}

		public IEnumerable<Nonterminal> SourceNonterminals => _rules.Keys;

		public IEnumerable<Nonterminal> Nonterminals => _rules.SelectMany(pair => pair.Value.SelectMany(pr => pr.InvolvedNonterminals).Append(pair.Key)).Distinct();

		public IEnumerable<Terminal> Terminals => _terminals.Values.SelectMany(l => l).Select(tc => tc.Terminal);

		public Nonterminal InitialState { get; set; }

		public IReadOnlyList<ProductionRule> this[Nonterminal index] => _rules.ContainsKey(index) ? _rules[index] : throw new KeyNotFoundException();

		public int Count => _rules.Count;

		public bool IsReadOnly => false;

		public IEnumerator<ProductionRule> GetEnumerator() => _rules.Values.SelectMany(rules => rules).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(ProductionRule rule) {
			if (!_rules.ContainsKey(rule.Nonterminal))
				_rules[rule.Nonterminal] = new List<ProductionRule>();
			_rules[rule.Nonterminal].Add(rule);
			foreach (var terminal in rule.InvolvedTerminals) {
				var token = terminal.Token;
				if (!_terminals.ContainsKey(token))
					_terminals[token] = new HashSet<TerminalCount>();
				if (_terminals[token].TryGetValue(terminal, out var tc))
					++tc.Count;
				else
					_terminals[token].Add(terminal);
			}
		}

		public void Clear() => _rules.Clear();

		public bool Contains(ProductionRule rule) => _rules.ContainsKey(rule.Nonterminal) && _rules[rule.Nonterminal].Contains(rule);

		public void CopyTo(ProductionRule[] array, int arrayIndex) => throw new NotSupportedException();

		public bool Remove(ProductionRule rule) {
			bool result = _rules.ContainsKey(rule.Nonterminal) && _rules[rule.Nonterminal].Remove(rule);
			if (result) {
				if (_rules[rule.Nonterminal].Count == 0)
					_rules.Remove(rule.Nonterminal);
				foreach (var terminal in rule.InvolvedTerminals) {
					var token = terminal.Token;
					_terminals[token].TryGetValue(terminal, out var tc);
					--tc!.Count;
					if (tc.Count == 0)
						_terminals[token].Remove(tc);
				}
			}
			return result;
		}

		public void AddProductionRule(Nonterminal nonterminal, IEnumerable<SentenceForm> productions) {
			if (!_rules.ContainsKey(nonterminal))
				_rules.Add(nonterminal, new List<ProductionRule>());
			_rules[nonterminal].AddRange(productions.Select(p => new ProductionRule(nonterminal, p)));
		}

		public void AddProductionRule(Nonterminal nonterminal, params SentenceForm[] productions) => AddProductionRule(nonterminal, productions.AsEnumerable());

		public Terminal? Match(Lexeme lexeme, bool checkAmbiguity = false) {
			if (!_terminals.ContainsKey(lexeme.Token))
				return null;
			return (checkAmbiguity
				? _terminals[lexeme.Token].SingleOrDefault(tc => tc.Terminal.Match(lexeme))
				: _terminals[lexeme.Token].FirstOrDefault(tc => tc.Terminal.Match(lexeme)))?.Terminal;
		}

		public void Simplify() => throw new NotImplementedException();

		private class TerminalCount : IEquatable<TerminalCount> {
			public TerminalCount(Terminal terminal) => Terminal = terminal;

			public Terminal Terminal { get; }

			public int Count { get; set; }

			public bool Equals(TerminalCount? other) {
				if (other is null)
					return false;
				return ReferenceEquals(this, other) || Terminal.Equals(other.Terminal);
			}

			public static implicit operator TerminalCount(Terminal terminal) => new(terminal);

			public override bool Equals(object? obj) {
				if (obj is null)
					return false;
				if (ReferenceEquals(this, obj))
					return true;
				return obj.GetType() == GetType() && Equals((TerminalCount)obj);
			}

			public override int GetHashCode() => Terminal.GetHashCode();
		}
	}
}