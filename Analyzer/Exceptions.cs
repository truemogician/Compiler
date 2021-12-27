using System;
using Parser;
using TrueMogician.Exceptions;

namespace Analyzer {
	public class UnexpectedSyntaxNodeException : ExceptionWithDefaultMessage {
		public UnexpectedSyntaxNodeException(string? message = null, Exception? innerException = null) : base(message, innerException) { }

		public SyntaxTreeNode? Node { get; init; }

		protected override string DefaultMessage => "Unexpected syntax node encountered during analyzing";
	}
}