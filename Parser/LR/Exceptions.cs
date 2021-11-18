using System;
using System.Collections.Generic;
using Lexer;

#nullable enable
namespace Parser.LR {
	public class ParsingTableConstructingException : ParserConstructingException {
		public ParsingTableConstructingException(Grammar grammar, string? message = null, Exception? innerException = null) : base(message, innerException) => Grammar = grammar;

		public Grammar Grammar { get; }

		protected override string DefaultMessage => "Error occurred when constructing parsing table";
	}

	public class UnexpectedActionException<TItem> : ParsingException where TItem : ItemBase {
		public UnexpectedActionException(ParsingTable<TItem> parsingTable, string? message = null, Exception? innerException = null) : base(message, innerException) => ParsingTable = parsingTable;

		public UnexpectedActionException(ParsingTable<TItem> parsingTable, IEnumerable<Lexeme> lexemes, int? position = null, string? message = null, Exception? innerException = null) : base(lexemes, position, message, innerException) => ParsingTable = parsingTable;

		public ParsingTable<TItem> ParsingTable { get; }

		protected override string DefaultMessage => $"Unexpected action retrieved from parsing table{(Position is null ? "" : $" at {Position}")}";
	}
}