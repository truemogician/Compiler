using System;
using System.Collections.Generic;

#nullable enable
namespace Lexer {
	public class LexerException : Exception {
		public string? Code { get; }

		public LexerException() { }

		public LexerException(string code) => Code = code;

		public LexerException(string code, string? message = null, Exception? innerException = null) : base(message, innerException) => Code = code;
	}

	public class TokenNotMatchedException : LexerException {
		public int? Position { get; }

		public TokenNotMatchedException() { }

		public TokenNotMatchedException(string code, int position) : base(code, $"No token match code at {position}") => Position = position;
	}

	public class AmbiguityException : LexerException {
		public int? Position { get; }

		public IEnumerable<Token>? Tokens { get; }

		public AmbiguityException() { }

		public AmbiguityException(string code, int position, IEnumerable<Token> tokens) : base(code, $"Multiple tokens match code at {position}") {
			Position = position;
			Tokens = tokens;
		}
	}
}