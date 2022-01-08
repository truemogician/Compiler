using System.Collections.Generic;
using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public abstract class SyntaxComponent {
		protected SyntaxComponent(SyntaxTreeNode node) => Node = node;

		internal SyntaxTreeNode Node { get; }

		internal Dictionary<string, object?> Properties { get; } = new();

		internal SemanticError CreateError(SemanticErrorType type) => new(type, Node);

		internal SemanticError CreateError(SemanticErrorType type, string message) => new(type, Node) { Message = message };

		internal T? GetProperty<T>(string key) => (T?)Properties[key];

		internal void SetProperty<T>(string key, T? value) => Properties[key] = value;
	}
}