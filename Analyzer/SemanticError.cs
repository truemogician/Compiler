using System;
using System.Linq;
using System.Text;
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

		public override string ToString() {
			var builder = new StringBuilder();
			builder.Append(Enum.GetName(Type.Level));
			builder.Append(" " + Type.Code);
			if (Range.HasValue) {
				string prefix = Range.Value.Buffer[..Range.Value.Offset];
				int line = prefix.Count(c => c == '\n') + 1;
				int col = Range.Value.Offset - prefix.LastIndexOf('\n');
				builder.Append($" ({line}, {col})");
			}
			builder.Append(" " + Message);
			return builder.ToString();
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