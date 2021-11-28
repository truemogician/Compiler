using System.Collections.Generic;
using Lexer;

namespace Parser {
	public abstract class ParserBase : IParser {
		protected ParserBase(Grammar grammar) => Grammar = grammar;

		public Grammar Grammar { get; }

		public abstract AbstractSyntaxTree Parse(IEnumerable<Token> tokens);

		public abstract void Initialize();
	}
}