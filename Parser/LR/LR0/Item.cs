using System;

namespace Parser.LR.LR0 {
	public class Item<TNonterminal, TToken> : IItem<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		public Item(ProductionRule<TNonterminal, TToken> productionRule, int marker) {
			ProductionRule = productionRule;
			Marker = marker;
		}

		public ProductionRule<TNonterminal, TToken> ProductionRule { get; }

		public int Marker { get; }
	}
}