using Microsoft.Extensions.Primitives;
using Parser;

namespace Analyzer {
	public class SemanticError {
		private string? _message;

		public SemanticError(SemanticErrorType type) => Type = type;

		public SemanticError(SemanticErrorType type, SyntaxTreeNode node) : this(type) => Range = node.CodeRange;

		public SemanticErrorType Type { get; }

		public StringSegment? Range { get; init; }

		public string? Message {
			get => _message ?? Type.DefaultMessage;
			set => _message = value;
		}
	}

	public record SemanticErrorType(string Code, ErrorLevel Level, string Analyzer) {
		public string? DefaultMessage { get; init; }
	}

	public enum ErrorLevel : byte {
		Error,

		Warning,

		Information
	}
}