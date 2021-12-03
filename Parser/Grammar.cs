using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexer;
using TrueMogician.Extensions.Enumerable;
using QuikGraph;

namespace Parser {
	public class Grammar : ISet<ProductionRule> {
		private sealed record NonterminalPair(Nonterminal First, Nonterminal Second) {
			public bool Equals(NonterminalPair? other) {
				if (ReferenceEquals(this, other))
					return true;
				return First.Equals(other!.First) && Second.Equals(other!.Second) || First.Equals(other!.Second) && Second.Equals(other!.First);
			}

			public override int GetHashCode() => First.GetHashCode() + Second.GetHashCode();
		}

		private readonly Dictionary<Lexeme, HashSet<TerminalCount>> _terminals = new();

		private readonly Dictionary<Nonterminal, HashSet<ProductionRule>> _productionRules = new();

		public Grammar(Nonterminal initialState) => InitialState = initialState;

		public Grammar(Grammar grammar) {
			foreach (var (nonterminal, rules) in grammar._productionRules)
				_productionRules.Add(nonterminal, new HashSet<ProductionRule>(rules));
			foreach (var (token, set) in grammar._terminals)
				_terminals.Add(token, new HashSet<TerminalCount>(set));
			InitialState = grammar.InitialState;
		}

		public IEnumerable<Nonterminal> SourceNonterminals => _productionRules.Keys;

		public IEnumerable<Nonterminal> Nonterminals => _productionRules.SelectMany(pair => pair.Value.SelectMany(pr => pr.InvolvedNonterminals).Append(pair.Key)).Distinct();

		public IEnumerable<Terminal> Terminals => _terminals.Values.SelectMany(l => l).Select(tc => tc.Terminal);

		public IReadOnlyDictionary<Nonterminal, HashSet<ProductionRule>> ProductionRules => _productionRules;

		public Nonterminal InitialState { get; set; }

		public IReadOnlySet<ProductionRule> this[Nonterminal index] => _productionRules.ContainsKey(index) ? _productionRules[index] : throw new KeyNotFoundException();

		public int Count => _productionRules.Sum(pair => pair.Value.Count);

		public bool IsReadOnly => false;

		public IEnumerator<ProductionRule> GetEnumerator() => _productionRules.Values.SelectMany(rules => rules).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(ProductionRule rule) => ((ISet<ProductionRule>)this).Add(rule);

		public void ExceptWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (_productionRules.ContainsKey(group.Key))
					_productionRules[group.Key].ExceptWith(group);
		}

		public void IntersectWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (_productionRules.ContainsKey(group.Key))
					_productionRules[group.Key].IntersectWith(group);
		}

		public bool IsProperSubsetOf(IEnumerable<ProductionRule> other) {
			var arr = other.ToArray();
			return arr.Length < Count && IsSubsetOf(arr);
		}

		public bool IsProperSupersetOf(IEnumerable<ProductionRule> other) {
			var arr = other.ToArray();
			return arr.Length > Count && IsSupersetOf(arr);
		}

		public bool IsSubsetOf(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).All(group => _productionRules.ContainsKey(group.Key) && _productionRules[group.Key].IsSubsetOf(group));

		public bool IsSupersetOf(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).All(group => _productionRules.ContainsKey(group.Key) && _productionRules[group.Key].IsSupersetOf(group));

		public bool Overlaps(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).Any(group => _productionRules.ContainsKey(group.Key) && _productionRules[group.Key].Overlaps(group));

		public bool SetEquals(IEnumerable<ProductionRule> other) => other.GroupBy(pr => pr.Nonterminal).All(group => _productionRules.ContainsKey(group.Key) && _productionRules[group.Key].SetEquals(group));

		public void SymmetricExceptWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (_productionRules.ContainsKey(group.Key))
					_productionRules[group.Key].SymmetricExceptWith(group);
		}

		public void UnionWith(IEnumerable<ProductionRule> other) {
			foreach (var group in other.GroupBy(pr => pr.Nonterminal))
				if (!_productionRules.ContainsKey(group.Key))
					_productionRules[group.Key] = new HashSet<ProductionRule>(group);
				else
					_productionRules[group.Key].UnionWith(group);
		}

		bool ISet<ProductionRule>.Add(ProductionRule rule) {
			if (!_productionRules.ContainsKey(rule.Nonterminal))
				_productionRules[rule.Nonterminal] = new HashSet<ProductionRule>();
			bool result = _productionRules[rule.Nonterminal].Add(rule);
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

		public void Clear() => _productionRules.Clear();

		public bool Contains(ProductionRule rule) => _productionRules.ContainsKey(rule.Nonterminal) && _productionRules[rule.Nonterminal].Contains(rule);

		public void CopyTo(ProductionRule[] array, int arrayIndex) => throw new NotSupportedException();

		public bool Remove(ProductionRule rule) {
			bool result = _productionRules.ContainsKey(rule.Nonterminal) && _productionRules[rule.Nonterminal].Remove(rule);
			if (result) {
				if (_productionRules[rule.Nonterminal].Count == 0)
					_productionRules.Remove(rule.Nonterminal);
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

		public bool Remove(Nonterminal nonterminal) => _productionRules.Remove(nonterminal);

		public Terminal? Match(Token token, bool checkAmbiguity = false) {
			if (!_terminals.ContainsKey(token.Lexeme))
				return null;
			return (checkAmbiguity
				? _terminals[token.Lexeme].SingleOrDefault(tc => tc.Terminal.Match(token))
				: _terminals[token.Lexeme].FirstOrDefault(tc => tc.Terminal.Match(token)))?.Terminal;
		}

		public void Simplify() {
			//Remove self productions
			foreach (var (src, prs) in _productionRules)
				prs.Where(pr => pr.Length == 1 && !pr[0].IsTerminal && pr[0].AsNonterminal == src).Each(pr => prs.Remove(pr));
			//Remove unreachable productions
			var queue = new Queue<Nonterminal>();
			var set = new HashSet<Nonterminal>(SourceNonterminals);
			queue.Enqueue(InitialState);
			while (queue.Count > 0) {
				var cur = queue.Dequeue();
				set.Remove(cur);
				foreach (var nt in _productionRules[cur].SelectMany(pr => pr.InvolvedNonterminals).Distinct())
					if (set.Contains(nt) && !queue.Contains(nt))
						queue.Enqueue(nt);
			}
			foreach (var nt in set)
				_productionRules.Remove(nt);
			var srcs = SourceNonterminals.ToList();
			do {
				var removed = false;
				foreach (var (src, prs) in _productionRules) {
					foreach (var pr in prs.Where(pr => pr.Production.Any(s => !s.IsTerminal && !srcs.Contains(s.AsNonterminal))))
						prs.Remove(pr);
					if (prs.Count == 0) {
						removed = true;
						_productionRules.Remove(src);
					}
				}
				if (!removed)
					break;
			} while (true);
			//Remove unterminatable productions
			foreach (var (src, prs) in _productionRules)
				if (prs.Any(pr => pr.IsEmpty || pr.Production.All(s => s.IsTerminal)))
					queue.Enqueue(src);
			set = new HashSet<Nonterminal>(SourceNonterminals);
			while (queue.Count > 0) {
				while (queue.Count > 0)
					set.Remove(queue.Dequeue());
				foreach (var nt in set.Where(nt => _productionRules[nt].Any(pr => pr.Production.All(s => s.IsTerminal || !set.Contains(s.AsNonterminal)))))
					queue.Enqueue(nt);
			}
			if (set.Count > 0) {
				set.Each(nt => _productionRules.Remove(nt));
				foreach (var prs in _productionRules.Values)
					prs.Where(pr => pr.Production.Any(s => !s.IsTerminal && set.Contains(s.AsNonterminal))).Each(pr => prs.Remove(pr));
			}
		}

		public void RemoveEmptyProductionRules() {
			foreach (var (src, rules) in _productionRules) {
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

		public void MergeEquivalentNonterminals(bool preserveNonTempNonterminals = true) {
			static bool IsSentencesEquivalent(SentenceForm stc1, SentenceForm stc2, out HashSet<NonterminalPair>? dependencies) {
				if (stc1.Count != stc2.Count)
					goto NotMatched;
				dependencies = new HashSet<NonterminalPair>();
				foreach (var (sym1, sym2) in stc1.IndexJoin(stc2)) {
					if (sym1.IsTerminal != sym2.IsTerminal || sym1.IsTerminal && !sym1.AsTerminal.Equals(sym2.AsTerminal))
						goto NotMatched;
					if (!sym1.IsTerminal) {
						Nonterminal n1 = sym1.AsNonterminal, n2 = sym2.AsNonterminal;
						if (!n1.Equals(n2))
							dependencies.Add(new NonterminalPair(n1, n2));
					}
				}
				return true;
			NotMatched:
				dependencies = null;
				return false;
			}
			var rules = _productionRules.Select(
					p => (p.Key, p.Value.GroupBy(pr => pr.Length, pr => pr.Production).Select(g => (g.Key, g.ToArray())).ToArray())
				)
				.ToArray();
			var dependencyGraph = new BidirectionalGraph<NonterminalPair, Edge<NonterminalPair>>(false);
			var unreachableVertex = new NonterminalPair(new Nonterminal(), new Nonterminal());
			dependencyGraph.AddVertex(unreachableVertex);
			for (var i = 0; i < rules.Length; ++i) {
				var (nt1, groups1) = rules[i];
				for (int j = i + 1; j < rules.Length; ++j) {
					var (nt2, groups2) = rules[j];
					if (groups1.Length != groups2.Length)
						continue;
					var dependencies = new HashSet<NonterminalPair>();
					foreach (var ((length1, sentences1), (length2, sentences2)) in groups1.IndexJoin(groups2)) {
						if (length1 != length2 || sentences1.Length != sentences2.Length)
							goto Continue;
						for (var k = 0; k < sentences1.Length; ++k) {
							int l;
							for (l = k; l < sentences2.Length; ++l)
								if (IsSentencesEquivalent(sentences1[k], sentences2[l], out var dp)) {
									dependencies.UnionWith(dp!);
									break;
								}
							if (l == sentences2.Length)
								goto Continue;
							if (l != k) {
								var tmp = sentences2[l];
								sentences2[l] = sentences2[k];
								sentences2[k] = tmp;
							}
						}
					}
					var cur = new NonterminalPair(nt1, nt2);
					if (dependencyGraph.ContainsVertex(cur))
						dependencyGraph.ClearOutEdges(cur);
					else
						dependencyGraph.AddVertex(cur);
					foreach (var pair in dependencies) {
						if (!dependencyGraph.ContainsVertex(pair)) {
							dependencyGraph.AddVertex(pair);
							dependencyGraph.AddEdge(new Edge<NonterminalPair>(pair, unreachableVertex));
						}
						dependencyGraph.AddEdge(new Edge<NonterminalPair>(cur, pair));
					}
				Continue: ;
				}
			}
			var visited = new HashSet<NonterminalPair>();
			var queue = new Queue<NonterminalPair>();
			visited.Add(unreachableVertex);
			queue.Enqueue(unreachableVertex);
			while (queue.Count > 0) {
				var cur = queue.Dequeue();
				var vertices = dependencyGraph.InEdges(cur).Select(e => e.Source).ToArray();
				dependencyGraph.RemoveVertex(cur);
				foreach (var next in vertices)
					if (!visited.Contains(next)) {
						visited.Add(next);
						queue.Enqueue(next);
					}
			}
			var equivalentGraph = new UndirectedGraph<Nonterminal, Edge<Nonterminal>>(false);
			foreach (var (first, second) in dependencyGraph.Vertices)
				equivalentGraph.AddVerticesAndEdge(new Edge<Nonterminal>(first, second));
			var substituteMap = new Dictionary<Nonterminal, Nonterminal>();
			var tagged = new HashSet<Nonterminal>();
			foreach (var nt in equivalentGraph.Vertices) {
				if (tagged.Contains(nt))
					continue;
				var group = new List<Nonterminal>();
				var q = new Queue<Nonterminal>();
				q.Enqueue(nt);
				while (q.Count > 0) {
					var cur = q.Dequeue();
					tagged.Add(cur);
					group.Add(cur);
					foreach (var next in equivalentGraph.AdjacentEdges(cur).Select(e => e.Source.Equals(cur) ? e.Target : e.Source))
						if (!tagged.Contains(next))
							q.Enqueue(next);
				}
				var (temps, nontemps) = group.Split(n => n.Temporary);
				if (preserveNonTempNonterminals && temps.Count > 1) {
					var target = temps[0];
					foreach (var n in temps.Skip(1))
						substituteMap[n] = target;
				}
				else if (!preserveNonTempNonterminals) {
					var target = nontemps.Concat(temps).First();
					foreach (var n in nontemps.Concat(temps).Skip(1))
						substituteMap[n] = target;
				}
			}
			substituteMap.Keys.Each(nt => Remove(nt));
			foreach (var rule in this)
				for (var i = 0; i < rule.Length; ++i) {
					if (rule[i].IsTerminal)
						continue;
					var nt = rule[i].AsNonterminal;
					if (substituteMap.ContainsKey(nt))
						rule[i] = substituteMap[nt];
				}
		}

		public void MergeAndRemove(Nonterminal nonterminal) {
			if (!_productionRules.ContainsKey(nonterminal))
				throw new KeyNotFoundException($"Nonterminal {nonterminal} not found in grammar");
			if (nonterminal == InitialState)
				throw new InvalidOperationException("Initial nonterminal cannot be merged");
			foreach (var (src, prs) in _productionRules) {
				if (src == nonterminal)
					continue;
				var targets = prs.Where(pr => pr.Production.Contains(nonterminal)).ToArray();
				foreach (var pr in targets) {
					prs.UnionWith(Substitute(pr.Production, nonterminal).Select(s => new ProductionRule(src, s)));
					prs.Remove(pr);
				}
			}
			_productionRules.Remove(nonterminal);
		}

		public void MergeAndRemove(ProductionRule productionRule) {
			var nt = productionRule.Nonterminal;
			if (!Contains(productionRule))
				throw new KeyNotFoundException($"Production rule {productionRule} not found in grammar");
			foreach (var (src, rules) in _productionRules) {
				if (src.Equals(nt))
					continue;
				var targets = rules.Where(pr => pr.Production.Any(s => s == nt)).ToArray();
				foreach (var rule in targets)
					rules.UnionWith(Substitute(rule.Production, productionRule).Select(s => new ProductionRule(src, s)));
			}
			_productionRules[nt].Remove(productionRule);
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
			foreach (var stc in from pr in _productionRules[nonterminal]
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