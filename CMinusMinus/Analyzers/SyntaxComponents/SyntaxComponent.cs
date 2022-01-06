using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public abstract class SyntaxComponent {
		internal SyntaxTreeNode Node { get; }

		protected SyntaxComponent(SyntaxTreeNode node) => Node = node;

		internal SemanticError CreateError(SemanticErrorType type) => new(type, Node);
	}
}