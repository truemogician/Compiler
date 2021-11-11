using System;

namespace Parser.LR.CLR {
	public class ItemSet<TNonterminal, TToken> : ItemSetBase<TNonterminal, TToken, Item<TNonterminal, TToken>> where TNonterminal : struct, Enum where TToken : struct, Enum { }
}