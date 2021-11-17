using System;

#nullable enable
namespace Parser.LR {
	public abstract record ItemBase {
		protected ItemBase(ProductionRule productionRule, int marker) {
			if (marker > productionRule.Length)
				throw new ArgumentOutOfRangeException(nameof(marker));
			ProductionRule = productionRule;
			Marker = marker;
		}

		public ProductionRule ProductionRule { get; }

		public int Marker { get; }

		public Symbol? NextSymbol => Marker == ProductionRule.Length ? null : ProductionRule[Marker];

		public override string ToString() => $"{ProductionRule.Nonterminal} -> {ProductionRule.Production[..Marker]}●{ProductionRule.Production[Marker..]}";
	}
}