using System;

namespace Parser.LR {
	public abstract record ItemBase(ProductionRule ProductionRule, int Marker) {
		public Symbol NextSymbol => Marker == ProductionRule.Production.Count ? Terminal.Terminator : ProductionRule[Marker];
	}
}