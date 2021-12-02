using System;

namespace Parser {
	public class SyntaxTree : IFormattable {
		public SyntaxTree(SyntaxTreeNode root) => Root = root;

		public SyntaxTreeNode Root { get; }

		public static implicit operator SyntaxTree(SyntaxTreeNode node) => new(node);

		public string ToString(bool skipTempNonterminal) => Root.ToString(0, skipTempNonterminal);

		public override string ToString() => ToString(true);

		public string ToString(string? format, IFormatProvider? formatProvider) => Root.ToString(format, formatProvider);
	}
}