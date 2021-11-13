namespace Parser.LR.CLR {
	public record Item(ProductionRule ProductionRule, int Marker, Terminal Lookahead) : ItemBase(ProductionRule, Marker);
}