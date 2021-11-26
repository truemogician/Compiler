using System.Collections.Generic;
using Lexer;

#nullable enable
namespace Parser {
	public abstract class ParserBase : IParser {
		// ReSharper disable once VirtualMemberCallInConstructor
		protected ParserBase(Grammar grammar) {
			Grammar = grammar;
			Initialize(grammar);
		}

		public Grammar Grammar { get; }

		public abstract AbstractSyntaxTree Parse(IEnumerable<Token> tokens);

		protected abstract void Initialize(Grammar grammar);
	}
}