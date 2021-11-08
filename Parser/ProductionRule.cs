using System;

namespace Parser {
	public class ProductionRule<TNonterminal, TTerminal> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		public ProductionRule(TNonterminal nonTerminal, SentenceForm<TNonterminal, TTerminal> sentenceForm) {
			NonTerminal = nonTerminal;
			SentenceForm = sentenceForm;
		}

		public TNonterminal NonTerminal { get; init; }

		public SentenceForm<TNonterminal, TTerminal> SentenceForm { get; init; }
	}
}