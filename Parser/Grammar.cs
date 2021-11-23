using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexer;
using TrueMogician.Extensions.Enumerable;

#nullable enable
namespace Parser {
	public class Grammar : ISet<ProductionRule> {
		private readonly Dictionary<Nonterminal, HashSet<ProductionRule>> _rules = new();

		private readonly Dictionary<Lexeme, HashSet<TerminalCount>> _terminals = new();

		public Grammar(Nonterminal initialState) => InitialState = initialState;

		public Grammar(Grammar grammar) {
			foreach (var (nonterminal, rules) in grammar._rules)
				_rules.Add(nonterminal, new HashSet<ProductionRule>(rules));
			foreach (var (token, set) in grammar._terminals)
				_terminals.Add(token, new HashSet<TerminalCount>(set));
			InitialState = grammar.InitialState;
		}

		public IEnumerable<Nonterminal> SourceNonterminals => _rules.Keys;

		public IEnumerable<Nonterminal> Nonterminals => _rules.SelectMany(pair => pair.Value.SelectMany(pr => pr.InvolvedNonterminals).Append(pair.Key)).Distinct();

		public IEnumerable<Terminal> Terminals => _terminals.Values.SelectMany(l => l).Select(tc => tc.Terminal);

		public Nonterminal InitialState { get; set; }

		public IReadOnlySet<ProductionRule> this[Nonterminal index] => _rules.ContainsKey(index) ? _rules[index] : throw new KeyNotFoundException();

		internal Dictionary<Nonterminal, HashSet<ProductionRule>> ProductionRules => _rules;

		public int Count => _rules.Sum(pair => pair.Value.Count);

		public bool IsReadOnly => false;

		public IEnumerator<ProductionRule> GetEnumerator() => _rules.Values.SelectMany(rules => rules).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(ProductionRule rule) => ((ISet<ProductionRule>)this).Add(rule);

		public void ExceptWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (_rules.ContainsKey(group.Key))
					_rules[group.Key].ExceptWith(group);
		}

		public void IntersectWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (_rules.ContainsKey(group.Key))
					_rules[group.Key].IntersectWith(group);
		}

		public bool IsProperSubsetOf(IEnumerable<ProductionRule> other) {
			var arr = other.ToArray();
			return arr.Length < Count && IsSubsetOf(arr);
		}

		public bool IsProperSupersetOf(IEnumerable<ProductionRule> other) {
			var arr = other.ToArray();
			return arr.Length > Count && IsSupersetOf(arr);
		}

		public bool IsSubsetOf(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).All(group => _rules.ContainsKey(group.Key) && _rules[group.Key].IsSubsetOf(group));

		public bool IsSupersetOf(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).All(group => _rules.ContainsKey(group.Key) && _rules[group.Key].IsSupersetOf(group));

		public bool Overlaps(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).Any(group => _rules.ContainsKey(group.Key) && _rules[group.Key].Overlaps(group));

		public bool SetEquals(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).All(group => _rules.ContainsKey(group.Key) && _rules[group.Key].SetEquals(group));

		public void SymmetricExceptWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (_rules.ContainsKey(group.Key))
					_rules[group.Key].SymmetricExceptWith(group);
		}

		public void UnionWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (!_rules.ContainsKey(group.Key))
					_rules[group.Key] = new HashSet<ProductionRule>(group);
				else
					_rules[group.Key].UnionWith(group);
		}

		bool ISet<ProductionRule>.Add(ProductionRule rule) {
			if (!_rules.ContainsKey(rule.Nonterminal))
				_rules[rule.Nonterminal] = new HashSet<ProductionRule>();
			bool result = _rules[rule.Nonterminal].Add(rule);
			if (result)
				foreach (var terminal in rule.InvolvedTerminals) {
					var token = terminal.Lexeme;
					if (!_terminals.ContainsKey(token))
						_terminals[token] = new HashSet<TerminalCount>();
					if (_terminals[token].TryGetValue(terminal, out var tc))
						++tc.Count;
					else
						_terminals[token].Add(terminal);
				}
			return result;
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
					var token = terminal.Lexeme;
					_terminals[token].TryGetValue(terminal, out var tc);
					--tc!.Count;
					if (tc.Count == 0)
						_terminals[token].Remove(tc);
				}
			}
			return result;
		}

		public void Add(Nonterminal nonterminal, IEnumerable<SentenceForm> productions) => productions.Each(s => Add(new ProductionRule(nonterminal, s)));

		public void Add(Nonterminal nonterminal, params SentenceForm[] productions) => Add(nonterminal, productions.AsEnumerable());

		public void Add(Nonterminal nonterminal, RegularSentenceForm regularSentenceForm) => regularSentenceForm.GenerateGrammar(nonterminal).Each(Add);

		public bool Remove(Nonterminal nonterminal) => _rules.Remove(nonterminal);

		public Terminal? Match(Token token, bool checkAmbiguity = false) {
			if (!_terminals.ContainsKey(token.Lexeme))
				return null;
			return (checkAmbiguity
				? _terminals[token.Lexeme].SingleOrDefault(tc => tc.Terminal.Match(token))
				: _terminals[token.Lexeme].FirstOrDefault(tc => tc.Terminal.Match(token)))?.Terminal;
		}

		public void Simplify() {
			//Remove self productions
			foreach (var (src, prs) in _rules)
				prs.Where(pr => pr.Length == 1 && !pr[0].IsTerminal && pr[0].AsNonterminal == src).Each(pr => prs.Remove(pr));
			//Remove unreachable productions
			var queue = new Queue<Nonterminal>();
			var set = new HashSet<Nonterminal>(SourceNonterminals);
			queue.Enqueue(InitialState);
			while (queue.Count > 0) {
				var cur = queue.Dequeue();
				set.Remove(cur);
				foreach (var nt in _rules[cur].SelectMany(pr => pr.InvolvedNonterminals).Distinct())
					if (set.Contains(nt))
						queue.Enqueue(nt);
			}
			foreach (var nt in set)
				_rules.Remove(nt);
			var srcs = SourceNonterminals.ToList();
			do {
				var removed = false;
				foreach (var (src, prs) in _rules) {
					foreach (var pr in prs.Where(pr => pr.Production.Any(s => !s.IsTerminal && !srcs.Contains(s.AsNonterminal))))
						prs.Remove(pr);
					if (prs.Count == 0) {
						removed = true;
						_rules.Remove(src);
					}
				}
				if (!removed)
					break;
			} while (true);
			//Remove unterminatable productions
			foreach (var (src, prs) in _rules)
				if (prs.Any(pr => pr.IsEmpty || pr.Production.All(s => s.IsTerminal)))
					queue.Enqueue(src);
			set = new HashSet<Nonterminal>(SourceNonterminals);
			while (queue.Count > 0) {
				while (queue.Count > 0)
					set.Remove(queue.Dequeue());
				foreach (var nt in set.Where(nt => _rules[nt].Any(pr => pr.Production.All(s => s.IsTerminal || !set.Contains(s.AsNonterminal)))))
					queue.Enqueue(nt);
			}
			if (set.Count > 0) {
				set.Each(nt => _rules.Remove(nt));
				foreach (var prs in _rules.Values)
					prs.Where(pr => pr.Production.Any(s => !s.IsTerminal && set.Contains(s.AsNonterminal))).Each(pr => prs.Remove(pr));
			}
		}

		public void RemoveEmptyProductionRules() {
			foreach (var (src, rules) in _rules) {
				if (src.Equals(InitialState))
					continue;
				var merged = false;
				foreach (var rule in rules.Where(pr => pr.IsEmpty)) {
					if (!merged) {
						MergeAndRemove(rule);
						merged = true;
					}
					rules.Remove(rule);
				}
			}
		}

		public void MergeAndRemove(Nonterminal nonterminal) {
			if (!_rules.ContainsKey(nonterminal))
				throw new KeyNotFoundException($"Nonterminal {nonterminal} not found in grammar");
			if (nonterminal == InitialState)
				throw new InvalidOperationException("Initial nonterminal cannot be merged");
			foreach (var (src, prs) in _rules) {
				if (src == nonterminal)
					continue;
				var targets = prs.Where(pr => pr.Production.Contains(nonterminal)).ToArray();
				foreach (var pr in targets) {
					prs.UnionWith(Substitute(pr.Production, nonterminal).Select(s => new ProductionRule(src, s)));
					prs.Remove(pr);
				}
			}
			_rules.Remove(nonterminal);
		}

		public void MergeAndRemove(ProductionRule productionRule) {
			var nt = productionRule.Nonterminal;
			if (!Contains(productionRule))
				throw new KeyNotFoundException($"Production rule {productionRule} not found in grammar");
			foreach (var (src, rules) in _rules) {
				if (src.Equals(nt))
					continue;
				var targets = rules.Where(pr => pr.Production.Any(s => s == nt)).ToArray();
				foreach (var rule in targets)
					rules.UnionWith(Substitute(rule.Production, productionRule).Select(s => new ProductionRule(src, s)));
			}
			_rules[nt].Remove(productionRule);
		}

		public void Replace(Nonterminal oldNonterminal, Nonterminal newNonterminal) {
			foreach (var rule in this)
				for (var i = 0; i < rule.Length; ++i)
					if (rule[i] == oldNonterminal)
						rule[i] = newNonterminal;
		}

		private IEnumerable<SentenceForm> Substitute(SentenceForm sentence, Nonterminal nonterminal, int position = 0) {
			while (position < sentence.Count && sentence[position] is var cur && (cur.IsTerminal || cur.AsNonterminal != nonterminal))
				++position;
			if (position == sentence.Count) {
				yield return sentence;
				yield break;
			}
			foreach (var stc in from pr in _rules[nonterminal]
				let newSentence = new SentenceForm(sentence) {[position..(position + 1)] = pr.Production}
				from stc in Substitute(newSentence, nonterminal, position + pr.Length)
				select stc)
				yield return stc;
		}

		private IEnumerable<SentenceForm> Substitute(SentenceForm sentence, ProductionRule pr, int position = 0, bool changed = false) {
			var nt = pr.Nonterminal;
			while (position < sentence.Count && sentence[position] is var cur && (cur.IsTerminal || cur.AsNonterminal != nt))
				++position;
			if (position == sentence.Count) {
				if (changed)
					yield return sentence;
				yield break;
			}
			foreach (var stc in Substitute(sentence, pr, position + 1, changed))
				yield return stc;
			foreach (var stc in Substitute(new SentenceForm(sentence) {[position..(position + 1)] = pr.Production}, pr, position + pr.Length, true))
				yield return stc;
		}

		private class TerminalCount : IEquatable<TerminalCount> {
			private TerminalCount(Terminal terminal) => Terminal = terminal;

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