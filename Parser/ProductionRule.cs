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

		public Symbol this[int index] => Production[index];
	}
}