using System;

namespace Parser.LR.CLR {
	public class ParsingTable<TNonterminal, TToken> : ParsingTable<TNonterminal, TToken, IItem<TNonterminal, TToken>> where TNonterminal : struct, Enum where TToken : struct, Enum {
		public ParsingTable(Grammar<TNonterminal, TToken> grammar) : base(grammar) { }
	}
}