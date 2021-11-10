using System;
using System.Collections.Generic;
using Lexer;

namespace Parser.LR.LR1 {
	// ReSharper disable once InconsistentNaming
	public class CanonicalLRParser<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		public CanonicalLRParser(Grammar<TNonterminal, TToken> grammar) => ParsingTable = new ParsingTable<TNonterminal, TToken>(grammar);

		public ParsingTable<TNonterminal, TToken> ParsingTable { get; }

		public AbstractSyntaxTree<TNonterminal, TToken> Parse(IEnumerable<Lexeme<TToken>> lexemes) => throw new NotImplementedException();
	}
}
