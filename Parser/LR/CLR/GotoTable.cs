using System;

namespace Parser.LR.CLR {
	public class GotoTable<TNonterminal, TToken> : GotoTable<TNonterminal, TToken, Item<TNonterminal, TToken>> where TNonterminal : struct, Enum where TToken : struct, Enum { }
}