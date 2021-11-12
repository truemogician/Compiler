using System;

namespace Parser.LR.CLR {
	public class Item : ItemBase {
		public Item(ProductionRule productionRule, int marker, Terminal lookahead) : base(productionRule, marker) => Lookahead = lookahead;

		public Terminal Lookahead { get; }
	}
}