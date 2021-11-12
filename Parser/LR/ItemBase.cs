using System;

namespace Parser.LR {
	public abstract class ItemBase {
		protected ItemBase(ProductionRule productionRule, int marker) {
			ProductionRule = productionRule;
			Marker = marker;
		}

		public ProductionRule ProductionRule { get; }

		public int Marker { get; }
	}
}