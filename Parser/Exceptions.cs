using System;
using System.Collections.Generic;
using System.Linq;
using Lexer;
using TrueMogician.Exceptions;

namespace Parser {
	public abstract class ParserException : ExceptionWithDefaultMessage {
		protected ParserException(string? message = null, Exception? innerException = null) : base(message, innerException) { }
	}

	public class ParserNotInitializedException : ParserException {
		public ParserNotInitializedException(string? message = null, Exception? innerException = null) : base(message, innerException) { }

		protected override string DefaultMessage => "Parser not initialized";
	}

	public class ParserConstructingException : ParserException {
		public ParserConstructingException(string? message = null, Exception? innerException = null) : base(message, innerException) { }

		protected override string DefaultMessage => "Error occurred when constructing the parser";
	}

	public class ParsingException : ParserException {
		public ParsingException(string? message = null, Exception? innerException = null) : base(message, innerException) { }

		public ParsingException(IEnumerable<Token> tokens, int? position = null, string? message = null, Exception? innerException = null) : base(message, innerException) {
			Tokens = tokens;
			Position = position;
		}

		public IEnumerable<Token>? Tokens { get; }

		public int? Position { get; }

		public Grammar? Grammar { get; init; }

		public Stack<SyntaxTreeNode>? CurrentStack { get; init; }

		protected override string DefaultMessage => $"Error occurred{(Position is null ? "" : $" at {Position}")} when parsing tokens";
	}

	public class TerminalNotMatchedException : ParsingException {
		public TerminalNotMatchedException(IEnumerable<Token> tokens, int position, string? message = null, Exception? innerException = null) : base(tokens, position, message, innerException) { }

		public Token NotMatchedLexeme => Tokens!.ElementAt(Position!.Value);

		protected override string DefaultMessage => $"Lexeme \"{NotMatchedLexeme}\"(position: {Position}) matches no terminal in grammar";
	}

	public class NotRecognizedException : ParsingException {
		public NotRecognizedException(IEnumerable<Token> tokens, int? position = null, string? message = null, Exception? innerException = null) : base(tokens, position, message, innerException) { }

		protected override string DefaultMessage => "Code cannot be recognized by grammar";
	}
}