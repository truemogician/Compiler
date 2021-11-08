using System;

namespace Parser.CanonicalLR {
	public class ParsingTable<TNonterminal, TTerminal> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		internal ParsingTable() { }

		public ParsingTable(Grammar<TNonterminal, TTerminal> grammar) => throw new NotImplementedException();

		public ActionTable<TNonterminal, TTerminal> ActionTable { get; } = new();

		public GotoTable<TNonterminal, TTerminal> GotoTable { get; } = new();
	}
}