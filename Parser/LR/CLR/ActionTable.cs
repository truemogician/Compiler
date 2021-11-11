using System;

namespace Parser.LR.CLR {
	public class ActionTable<TNonterminal, TToken> : ActionTable<TNonterminal, TToken, Item<TNonterminal, TToken>> where TNonterminal : struct, Enum where TToken : struct, Enum { }

	public class ActionFactory<TNonterminal, TToken> : ActionFactory<TNonterminal, TToken, Item<TNonterminal, TToken>> where TNonterminal : struct, Enum where TToken : struct, Enum { }
}