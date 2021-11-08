using System;
using System.Collections.Generic;

namespace Parser {
	public class SyntaxTreeNode<TNonterminal, TTerminal> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		public Component<TNonterminal, TTerminal> Value { get; init; }

		public SyntaxTreeNode<TNonterminal, TTerminal> Parent { get; set; }

		public List<SyntaxTreeNode<TNonterminal, TTerminal>> Children { get; } = new();

		public bool IsLeaf => Value.IsTerminal;
	}
}