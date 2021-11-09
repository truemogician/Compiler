using System;

namespace Parser {
	public class AbstractSyntaxTree<TNonterminal, TTerminal> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		public AbstractSyntaxTree(SyntaxTreeNode<TNonterminal, TTerminal> root) => Root = root;

		public SyntaxTreeNode<TNonterminal, TTerminal> Root { get; }

		public static explicit operator AbstractSyntaxTree<TNonterminal, TTerminal>(SyntaxTreeNode<TNonterminal, TTerminal> node) => new(node);

		public override string ToString() => Root.ToString(0);
	}
}