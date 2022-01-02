using System;
using System.Collections.Generic;

namespace Lexer {
	public class LexerException : Exception {
		public LexerException() { }

		public LexerException(string code) => Code = code;

		public LexerException(string code, string? message = null, Exception? innerException = null) : base(message, innerException) => Code = code;

		public string? Code { get; }
	}

	public class LexemeNotMatchedException : LexerException {
		public LexemeNotMatchedException() { }

		public LexemeNotMatchedException(string code, int position) : base(code, $"No lexeme matches code at {position}") => Position = position;

		public int? Position { get; }
	}

	public class AmbiguityException : LexerException {
		public AmbiguityException() { }

		public AmbiguityException(string code, int position, IEnumerable<Lexeme> tokens) : base(code, $"Multiple tokens match code at {position}") {
			Position = position;
			Tokens = tokens;
		}

		public int? Position { get; }

		public IEnumerable<Lexeme>? Tokens { get; }
	}
}