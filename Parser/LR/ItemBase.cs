namespace Parser.LR {
	public abstract record ItemBase(ProductionRule ProductionRule, int Marker) {
		public Symbol NextSymbol => Marker == ProductionRule.Length ? Terminal.Terminator : ProductionRule[Marker];

		public override string ToString() => $"{ProductionRule.Nonterminal} -> {ProductionRule.Production[..Marker]}●{ProductionRule.Production[Marker..]}";
	}
}