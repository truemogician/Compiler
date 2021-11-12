using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Lexer;

#nullable enable
namespace Parser.LR.CLR {
	public class Parser {
		public Parser(Grammar grammar) => ParsingTable = new ParsingTable(grammar);

		public ParsingTable ParsingTable { get; }

		public AbstractSyntaxTree Parse(IEnumerable<Lexeme> lexemes) {
			foreach (var lexeme in lexemes) { }
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryParse(IEnumerable<Lexeme> lexemes, out AbstractSyntaxTree? ast) {
			try {
				ast = Parse(lexemes);
				return true;
			}
			catch {
				ast = null;
				return false;
			}
		}
	}
}
