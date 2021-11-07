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

	public class NoMatchException : LexerException {
		public int? Position { get; }

		public NoMatchException() { }

		public NoMatchException(string code, int position) : base(code, $"No token match code at {position}") => Position = position;
	}

	public class AmbiguityException<T> : LexerException where T : Enum {
		public int? Position { get; }

		public IEnumerable<T>? TokenTypes { get; }

		public AmbiguityException() { }

		public AmbiguityException(string code, int position, IEnumerable<T> tokenTypes) : base(code, $"Multiple tokens match code at {position}") {
			Position = position;
			TokenTypes = tokenTypes;
		}
	}
}