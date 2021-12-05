using System;
using System.Collections.Generic;
using Lexer;

namespace Parser.LR {
	public class ParsingTableConstructingException : ParserConstructingException {
		public ParsingTableConstructingException(string? message = null, Exception? innerException = null) : base(message, innerException) { }

		public Grammar? Grammar { get; init; }

		protected override string DefaultMessage => "Error occurred when constructing parsing table";
	}

	public class ActionConflictException<TItem> : ParsingTableConstructingException where TItem : ItemBase {
		public ActionConflictException(string? message = null, Exception? innerException = null) : base(message, innerException) { }

		public ItemSet<TItem>? State { get; init; }

		public Terminal? Terminal { get; init; }

		public IEnumerable<IAction>? ConflictingActions { get; init; }

		protected override string DefaultMessage => "Conflicting actions detected";
	}

	public class UnexpectedActionException<TItem, TAction> : ParsingException where TItem : ItemBase {
		public UnexpectedActionException(ParsingTableBase<TItem, TAction> parsingTable, string? message = null, Exception? innerException = null) : base(message, innerException) => ParsingTable = parsingTable;

		public UnexpectedActionException(ParsingTableBase<TItem, TAction> parsingTable, IEnumerable<Token> tokens, int? position = null, string? message = null, Exception? innerException = null) : base(tokens, position, message, innerException) => ParsingTable = parsingTable;

		public ParsingTableBase<TItem, TAction> ParsingTable { get; }

		protected override string DefaultMessage => $"Unexpected action retrieved from parsing table{(Position is null ? "" : $" at {Position}")}";
	}
}