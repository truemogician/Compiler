using System;
using System.Collections.Generic;
using Lexer;

namespace Parser.LR.CLR {
	public class Parser<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		public Parser(Grammar<TNonterminal, TToken> grammar) => ParsingTable = new ParsingTable<TNonterminal, TToken>(grammar);

		public ParsingTable<TNonterminal, TToken> ParsingTable { get; }

		public AbstractSyntaxTree<TNonterminal, TToken> Parse(IEnumerable<Lexeme<TToken>> lexemes) => throw new NotImplementedException();
	}
}
