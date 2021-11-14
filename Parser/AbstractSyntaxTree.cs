using System;

namespace Parser {
	public class AbstractSyntaxTree {
		public AbstractSyntaxTree(SyntaxTreeNode root) => Root = root;

		public SyntaxTreeNode Root { get; }

		public static implicit operator AbstractSyntaxTree(SyntaxTreeNode node) => new(node);

		public override string ToString() => Root.ToString();
	}
}