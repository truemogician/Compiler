using System;
using System.Collections.Generic;

#nullable enable
namespace Parser {
	public record ProductionRule(Nonterminal Nonterminal, SentenceForm Production) {
		public IEnumerable<Nonterminal> InvolvedNonterminals {
			get {
				yield return Nonterminal;
				foreach (var n in Production.Nonterminals)
					if (!n.Equals(Nonterminal))
						yield return n;
			}
		}

		public IEnumerable<Terminal> InvolvedTerminals => Production.Terminals;

		public int Length => Production.Count;

		public bool IsEmpty => Production.Equals(SentenceForm.Empty);

		public Symbol this[int index] {
			get => Production[index];
			internal set => Production[index] = value;
		}

		public Symbol this[Index index] {
			get => Production[index];
			internal set => Production[index] = value;
		}

		public SentenceForm this[Range range] {
			get => Production[range];
			internal set => Production[range] = value;
		}

		public override string ToString() => $"<{Nonterminal}> -> {Production}";
	}
}