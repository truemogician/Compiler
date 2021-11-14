using System;
using System.Collections.Generic;
using Lexer;

#nullable enable
namespace Parser.LR {
	public class ParserException : Exception {
		public ParserException() { }

		public ParserException(IEnumerable<Lexeme> lexemes) => Lexemes = lexemes;

		public ParserException(IEnumerable<Lexeme> lexemes, string? message = null, Exception? innerException = null) : base(message, innerException) => Lexemes = lexemes;

		public IEnumerable<Lexeme>? Lexemes { get; }
	}
}