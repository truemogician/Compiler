using System;

namespace Parser.LR {
	public interface IItem<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		public ProductionRule<TNonterminal, TToken> ProductionRule { get; }

		public int Marker { get; }
	}
}