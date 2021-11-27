using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexer;
using TrueMogician.Extensions.Enumerable;

namespace Parser {
	public class Grammar : ISet<ProductionRule> {
		private readonly Dictionary<Lexeme, HashSet<TerminalCount>> _terminals = new();

		public Grammar(Nonterminal initialState) => InitialState = initialState;

		public Grammar(Grammar grammar) {
			foreach (var (nonterminal, rules) in grammar.ProductionRules)
				ProductionRules.Add(nonterminal, new HashSet<ProductionRule>(rules));
			foreach (var (token, set) in grammar._terminals)
				_terminals.Add(token, new HashSet<TerminalCount>(set));
			InitialState = grammar.InitialState;
		}

		public IEnumerable<Nonterminal> SourceNonterminals => ProductionRules.Keys;

		public IEnumerable<Nonterminal> Nonterminals => ProductionRules.SelectMany(pair => pair.Value.SelectMany(pr => pr.InvolvedNonterminals).Append(pair.Key)).Distinct();

		public IEnumerable<Terminal> Terminals => _terminals.Values.SelectMany(l => l).Select(tc => tc.Terminal);

		public Nonterminal InitialState { get; set; }

		public IReadOnlySet<ProductionRule> this[Nonterminal index] => ProductionRules.ContainsKey(index) ? ProductionRules[index] : throw new KeyNotFoundException();

		internal Dictionary<Nonterminal, HashSet<ProductionRule>> ProductionRules { get; } = new();

		public int Count => ProductionRules.Sum(pair => pair.Value.Count);

		public bool IsReadOnly => false;

		public IEnumerator<ProductionRule> GetEnumerator() => ProductionRules.Values.SelectMany(rules => rules).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(ProductionRule rule) => ((ISet<ProductionRule>)this).Add(rule);

		public void ExceptWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (ProductionRules.ContainsKey(group.Key))
					ProductionRules[group.Key].ExceptWith(group);
		}

		public void IntersectWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (ProductionRules.ContainsKey(group.Key))
					ProductionRules[group.Key].IntersectWith(group);
		}

		public bool IsProperSubsetOf(IEnumerable<ProductionRule> other) {
			var arr = other.ToArray();
			return arr.Length < Count && IsSubsetOf(arr);
		}

		public bool IsProperSupersetOf(IEnumerable<ProductionRule> other) {
			var arr = other.ToArray();
			return arr.Length > Count && IsSupersetOf(arr);
		}

		public bool IsSubsetOf(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).All(group => ProductionRules.ContainsKey(group.Key) && ProductionRules[group.Key].IsSubsetOf(group));

		public bool IsSupersetOf(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).All(group => ProductionRules.ContainsKey(group.Key) && ProductionRules[group.Key].IsSupersetOf(group));

		public bool Overlaps(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).Any(group => ProductionRules.ContainsKey(group.Key) && ProductionRules[group.Key].Overlaps(group));

		public bool SetEquals(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).All(group => ProductionRules.ContainsKey(group.Key) && ProductionRules[group.Key].SetEquals(group));

		public void SymmetricExceptWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (ProductionRules.ContainsKey(group.Key))
					ProductionRules[group.Key].SymmetricExceptWith(group);
		}

		public void UnionWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (!ProductionRules.ContainsKey(group.Key))
					ProductionRules[group.Key] = new HashSet<ProductionRule>(group);
				else
					ProductionRules[group.Key].UnionWith(group);
		}

		bool ISet<ProductionRule>.Add(ProductionRule rule) {
			if (!ProductionRules.ContainsKey(rule.Nonterminal))
				ProductionRules[rule.Nonterminal] = new HashSet<ProductionRule>();
			bool result = ProductionRules[rule.Nonterminal].Add(rule);
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

		public void Clear() => ProductionRules.Clear();

		public bool Contains(ProductionRule rule) => ProductionRules.ContainsKey(rule.Nonterminal) && ProductionRules[rule.Nonterminal].Contains(rule);

		public void CopyTo(ProductionRule[] array, int arrayIndex) => throw new NotSupportedException();

		public bool Remove(ProductionRule rule) {
			bool result = ProductionRules.ContainsKey(rule.Nonterminal) && ProductionRules[rule.Nonterminal].Remove(rule);
			if (result) {
				if (ProductionRules[rule.Nonterminal].Count == 0)
					ProductionRules.Remove(rule.Nonterminal);
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

		public bool Remove(Nonterminal nonterminal) => ProductionRules.Remove(nonterminal);

		public Terminal? Match(Token token, bool checkAmbiguity = false) {
			if (!_terminals.ContainsKey(token.Lexeme))
				return null;
			return (checkAmbiguity
				? _terminals[token.Lexeme].SingleOrDefault(tc => tc.Terminal.Match(token))
				: _terminals[token.Lexeme].FirstOrDefault(tc => tc.Terminal.Match(token)))?.Terminal;
		}

		public void Simplify() {
			//Remove self productions
			foreach (var (src, prs) in ProductionRules)
				prs.Where(pr => pr.Length == 1 && !pr[0].IsTerminal && pr[0].AsNonterminal == src).Each(pr => prs.Remove(pr));
			//Remove unreachable productions
			var queue = new Queue<Nonterminal>();
			var set = new HashSet<Nonterminal>(SourceNonterminals);
			queue.Enqueue(InitialState);
			while (queue.Count > 0) {
				var cur = queue.Dequeue();
				set.Remove(cur);
				foreach (var nt in ProductionRules[cur].SelectMany(pr => pr.InvolvedNonterminals).Distinct())
					if (set.Contains(nt) && !queue.Contains(nt))
						queue.Enqueue(nt);
			}
			foreach (var nt in set)
				ProductionRules.Remove(nt);
			var srcs = SourceNonterminals.ToList();
			do {
				var removed = false;
				foreach (var (src, prs) in ProductionRules) {
					foreach (var pr in prs.Where(pr => pr.Production.Any(s => !s.IsTerminal && !srcs.Contains(s.AsNonterminal))))
						prs.Remove(pr);
					if (prs.Count == 0) {
						removed = true;
						ProductionRules.Remove(src);
					}
				}
				if (!removed)
					break;
			} while (true);
			//Remove unterminatable productions
			foreach (var (src, prs) in ProductionRules)
				if (prs.Any(pr => pr.IsEmpty || pr.Production.All(s => s.IsTerminal)))
					queue.Enqueue(src);
			set = new HashSet<Nonterminal>(SourceNonterminals);
			while (queue.Count > 0) {
				while (queue.Count > 0)
					set.Remove(queue.Dequeue());
				foreach (var nt in set.Where(nt => ProductionRules[nt].Any(pr => pr.Production.All(s => s.IsTerminal || !set.Contains(s.AsNonterminal)))))
					queue.Enqueue(nt);
			}
			if (set.Count > 0) {
				set.Each(nt => ProductionRules.Remove(nt));
				foreach (var prs in ProductionRules.Values)
					prs.Where(pr => pr.Production.Any(s => !s.IsTerminal && set.Contains(s.AsNonterminal))).Each(pr => prs.Remove(pr));
			}
		}

		public void RemoveEmptyProductionRules() {
			foreach (var (src, rules) in ProductionRules) {
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
			if (!ProductionRules.ContainsKey(nonterminal))
				throw new KeyNotFoundException($"Nonterminal {nonterminal} not found in grammar");
			if (nonterminal == InitialState)
				throw new InvalidOperationException("Initial nonterminal cannot be merged");
			foreach (var (src, prs) in ProductionRules) {
				if (src == nonterminal)
					continue;
				var targets = prs.Where(pr => pr.Production.Contains(nonterminal)).ToArray();
				foreach (var pr in targets) {
					prs.UnionWith(Substitute(pr.Production, nonterminal).Select(s => new ProductionRule(src, s)));
					prs.Remove(pr);
				}
			}
			ProductionRules.Remove(nonterminal);
		}

		public void MergeAndRemove(ProductionRule productionRule) {
			var nt = productionRule.Nonterminal;
			if (!Contains(productionRule))
				throw new KeyNotFoundException($"Production rule {productionRule} not found in grammar");
			foreach (var (src, rules) in ProductionRules) {
				if (src.Equals(nt))
					continue;
				var targets = rules.Where(pr => pr.Production.Any(s => s == nt)).ToArray();
				foreach (var rule in targets)
					rules.UnionWith(Substitute(rule.Production, productionRule).Select(s => new ProductionRule(src, s)));
			}
			ProductionRules[nt].Remove(productionRule);
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
			foreach (var stc in from pr in ProductionRules[nonterminal]
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