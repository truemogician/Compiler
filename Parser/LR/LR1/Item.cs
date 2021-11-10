using System;

namespace Parser.LR.LR1 {
	public class Item<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		public Item(TNonterminal nonTerminal, SentenceForm<TNonterminal, TToken> sentenceForm) : base(nonTerminal, sentenceForm) { }

		public ProductionRule<TNonterminal, TToken> ProductionRule { get; }

		public int Marker { get; }

		public Terminal<TToken> Lookahead { get; }
	}
}