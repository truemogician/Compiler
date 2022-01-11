using System;
using System.Linq;
using TrueMogician.Extensions.Collections.Tree;

namespace Parser {
	public class SyntaxTree : Tree<SyntaxTreeNode>, IFormattable {
		public SyntaxTree(SyntaxTreeNode root) : base(root) { }

		public string ToString(bool skipTempNonterminal) => Root.ToString(0, skipTempNonterminal);

		public override string ToString() => ToString(true);

		public string ToString(string? format, IFormatProvider? formatProvider) => Root.ToString(format, formatProvider);

		public void Clean() {
			if (Root.Value.Nonterminal is { Temporary: true })
				throw new InvalidOperationException("Root is a temporary node");
			foreach (var node in Root.Where(n => n.Value.Nonterminal?.Temporary == true).ToArray())
				SyntaxTreeNode.Unlink(node);
		}

		public static implicit operator SyntaxTree(SyntaxTreeNode node) => new(node);
	}
}