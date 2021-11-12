using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser {
	public class ProductionRule {
		public ProductionRule(Nonterminal nonTerminal, SentenceForm sentenceForm) {
			NonTerminal = nonTerminal;
			Production = sentenceForm;
		}

		public Nonterminal NonTerminal { get; init; }

		public SentenceForm Production { get; init; }

		public IEnumerable<Nonterminal> InvolvedNonterminals {
			get {
				yield return NonTerminal;
				foreach (var n in Production.Nonterminals)
					if (!n.Equals(NonTerminal))
						yield return n;
			}
		}

		public IEnumerable<Terminal> InvolvedTerminals => Production.Terminals;
	}
}