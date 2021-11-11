using System;

namespace Parser.LR.CLR {
	public class Item<TNonterminal, TToken> : IItem<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		public Item(ProductionRule<TNonterminal, TToken> productionRule, int marker, Terminal<TToken> lookahead) {
			ProductionRule = productionRule;
			Marker = marker;
			Lookahead = lookahead;
		}

		public ProductionRule<TNonterminal, TToken> ProductionRule { get; }

		public int Marker { get; }

		public Terminal<TToken> Lookahead { get; }
	}
}