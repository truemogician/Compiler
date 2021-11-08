using System;

namespace Parser.CanonicalLR {
	// ReSharper disable once InconsistentNaming
	public class CanonicalLRParser<TNonterminal, TTerminal> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		public CanonicalLRParser(Grammar<TNonterminal, TTerminal> grammar) => ParsingTable = new ParsingTable<TNonterminal, TTerminal>(grammar);

		public ParsingTable<TNonterminal, TTerminal> ParsingTable { get; }

		public AbstractSyntaxTree<TNonterminal, TTerminal> Parse(IEquatable<TTerminal> lexemes) => throw new NotImplementedException();
	}
}
