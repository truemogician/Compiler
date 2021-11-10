using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser {
	public class ProductionRule<TNonterminal, TTerminal> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		public ProductionRule(TNonterminal nonTerminal, SentenceForm<TNonterminal, TTerminal> sentenceForm) {
			NonTerminal = nonTerminal;
			Production = sentenceForm;
		}

		public TNonterminal NonTerminal { get; init; }

		public SentenceForm<TNonterminal, TTerminal> Production { get; init; }

		public IEnumerable<TNonterminal> InvolvedNonterminals {
			get {
				yield return NonTerminal;
				foreach (var n in Production.Nonterminals)
					if (!n.Equals(NonTerminal))
						yield return n;
			}
		}

		public IEnumerable<Terminal<TTerminal>> InvolvedTerminals => Production.Terminals;
	}
}