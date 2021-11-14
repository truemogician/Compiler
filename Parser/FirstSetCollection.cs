using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TrueMogician.Exceptions;

#nullable enable
namespace Parser {
	public class FirstSetCollection : IReadOnlyDictionary<Nonterminal, HashSet<Terminal?>> {
		private readonly HashSet<Nonterminal> _emptiableNonterminals = new();

		private readonly Dictionary<Nonterminal, HashSet<Terminal?>> _firstSets = new();

		public FirstSetCollection(Grammar grammar) {
			SourceGrammar = grammar;
			FindEmptiableNonterminals();
			var caller = new HashSet<Nonterminal>();
			foreach (var nonterminal in grammar.SourceNonterminals)
				if (!_firstSets.ContainsKey(nonterminal)) {
					Calculate(nonterminal, ref caller);
					caller.Clear();
				}
		}

		public Grammar SourceGrammar { get; }

		public HashSet<Terminal?> this[Terminal key] => new() {key};

		public HashSet<Terminal?> this[Symbol key] => key.IsTerminal ? this[key.AsTerminal] : this[key.AsNonterminal];

		public HashSet<Terminal?> this[SentenceForm sentence] => sentence.Count > 0 ? this[sentence[0]] : throw new ArgumentException("Empty symbol has no first set");

		public int Count => _firstSets.Count;

		public IEnumerable<Nonterminal> Keys => _firstSets.Keys;

		public IEnumerable<HashSet<Terminal?>> Values => _firstSets.Values;

		public IEnumerator<KeyValuePair<Nonterminal, HashSet<Terminal?>>> GetEnumerator() => _firstSets.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public bool ContainsKey(Nonterminal key) => _firstSets.ContainsKey(key);

		#pragma warning disable 8601
		public bool TryGetValue(Nonterminal key, out HashSet<Terminal?> value) => _firstSets.TryGetValue(key, out value);
		#pragma warning restore 8601

		public HashSet<Terminal?> this[Nonterminal key] => _firstSets[key];

		private void FindEmptiableNonterminals() {
			var nonterminalSets = new Dictionary<Nonterminal, HashSet<Nonterminal>[]>();
			var queue = new Queue<Nonterminal>();
			foreach (var nonterminal in SourceGrammar.SourceNonterminals) {
				if (SourceGrammar[nonterminal].Any(pr => pr.IsEmpty))
					queue.Enqueue(nonterminal);
				nonterminalSets[nonterminal] = SourceGrammar[nonterminal].Where(pr => pr.Production.All(s => !s.IsTerminal)).Select(pr => new HashSet<Nonterminal>(pr.Production.Nonterminals)).ToArray();
			}
			while (queue.Count > 0) {
				for (int i = queue.Count; i > 0; --i)
					_emptiableNonterminals.Add(queue.Dequeue());
				foreach (var (nonterminal, sets) in nonterminalSets)
					if (!_emptiableNonterminals.Contains(nonterminal) && sets.Any(s => s.IsSubsetOf(_emptiableNonterminals)))
						queue.Enqueue(nonterminal);
			}
		}

		private void Calculate(Nonterminal nonterminal, ref HashSet<Nonterminal> caller) {
			if (_firstSets.ContainsKey(nonterminal))
				return;
			var result = new HashSet<Terminal?>();
			if (_emptiableNonterminals.Contains(nonterminal))
				result.Add(null);
			foreach (var pr in SourceGrammar[nonterminal])
				if (!pr.IsEmpty) {
					if (pr[0].IsTerminal)
						result.Add(pr[0].AsTerminal);
					else
						for (var i = 0; i < pr.Length && !pr[i].IsTerminal; ++i) {
							var nt = pr[i].AsNonterminal;
							if (caller.Contains(nt))
								throw new InfiniteLoopException();
							if (_firstSets.ContainsKey(nt))
								result.UnionWith(_firstSets[nt]);
							else {
								bool success = caller.Add(nonterminal);
								Calculate(nt, ref caller);
								result.UnionWith(_firstSets[nt]);
								if (success)
									caller.Remove(nonterminal);
							}
							if (!_emptiableNonterminals.Contains(nt))
								break;
						}
				}
			_firstSets[nonterminal] = result;
		}
	}
}