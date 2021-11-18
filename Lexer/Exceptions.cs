﻿using System;
using System.Collections.Generic;

#nullable enable
namespace Lexer {
	public class LexerException : Exception {
		public LexerException() { }

		public LexerException(string code) => Code = code;

		public LexerException(string code, string? message = null, Exception? innerException = null) : base(message, innerException) => Code = code;

		public string? Code { get; }
	}

	public class TokenNotMatchedException : LexerException {
		public TokenNotMatchedException() { }

		public TokenNotMatchedException(string code, int position) : base(code, $"No token match code at {position}") => Position = position;

		public int? Position { get; }
	}

	public class AmbiguityException : LexerException {
		public AmbiguityException() { }

		public AmbiguityException(string code, int position, IEnumerable<Token> tokens) : base(code, $"Multiple tokens match code at {position}") {
			Position = position;
			Tokens = tokens;
		}

		public int? Position { get; }

		public IEnumerable<Token>? Tokens { get; }
	}
}