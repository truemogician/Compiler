﻿namespace Parser {
	public class AbstractSyntaxTree {
		public AbstractSyntaxTree(SyntaxTreeNode root) => Root = root;

		public SyntaxTreeNode Root { get; }

		public static implicit operator AbstractSyntaxTree(SyntaxTreeNode node) => new(node);

		public string ToString(bool skipTempNonterminal) => Root.ToString(0, skipTempNonterminal);

		public override string ToString() => ToString(true);
	}
}