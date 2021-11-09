using System;

namespace Parser.CanonicalLR {
	public class ParsingTable<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		internal ParsingTable() { }

		public ParsingTable(Grammar<TNonterminal, TToken> grammar) => throw new NotImplementedException();

		public ActionTable<TNonterminal, TToken> ActionTable { get; } = new();

		public GotoTable<TNonterminal, TToken> GotoTable { get; } = new();
	}
}