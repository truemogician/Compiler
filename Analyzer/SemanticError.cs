using Microsoft.Extensions.Primitives;
using Parser;

namespace Analyzer {
	public class SemanticError {
		public SemanticError(SemanticErrorType type) => Type = type;

		public SemanticError(SemanticErrorType type, SyntaxTreeNode node) : this(type) => Range = node.CodeRange;

		public SemanticErrorType Type { get; }

		public StringSegment? Range { get; init; }
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