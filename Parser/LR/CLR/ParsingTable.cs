using System;

namespace Parser.LR.CLR {
	public class ParsingTable : ParsingTable<ItemBase> {
		public ParsingTable(Grammar grammar) : base(grammar) { }
	}
}