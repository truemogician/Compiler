using System;
using System.Collections.Generic;
using System.Linq;
using Lexer;
using TrueMogician.Exceptions;

#nullable enable
namespace Parser {
	public abstract class ParserException : ExceptionWithDefaultMessage {
		protected ParserException(string? message = null, Exception? innerException = null) : base(message, innerException) { }
	}

	public class ParserConstructingException : ParserException {
		public ParserConstructingException(string? message = null, Exception? innerException = null) : base(message, innerException) { }

		protected override string DefaultMessage => "Error occurred when constructing the parser";
	}

	public class ParsingException : ParserException {
		public ParsingException(string? message = null, Exception? innerException = null) : base(message, innerException) { }

		public ParsingException(IEnumerable<Lexeme> lexemes, int? position = null, string? message = null, Exception? innerException = null) : base(message, innerException) {
			Lexemes = lexemes;
			Position = position;
		}

		public IEnumerable<Lexeme>? Lexemes { get; }

		public int? Position { get; }

		public Grammar? Grammar { get; init; }

		public Stack<SyntaxTreeNode>? CurrentStack { get; init; }

		protected override string DefaultMessage => $"Error occurred{(Position is null ? "" : $" at {Position}")} when parsing lexemes";
	}

	public class TerminalNotMatchedException : ParsingException {
		public TerminalNotMatchedException(IEnumerable<Lexeme> lexemes, int position, string? message = null, Exception? innerException = null) : base(lexemes, position, message, innerException) { }

		public Lexeme NotMatchedLexeme => Lexemes!.ElementAt(Position!.Value);

		protected override string DefaultMessage => $"Lexeme \"{NotMatchedLexeme}\"(position: {Position}) matches no terminal in grammar";
	}

	public class NotRecognizedException : ParsingException {
		public NotRecognizedException(IEnumerable<Lexeme> lexemes, int? position = null, string? message = null, Exception? innerException = null) : base(lexemes, position, message, innerException) { }

		protected override string DefaultMessage => "Code cannot be recognized by grammar";
	}
}