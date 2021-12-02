namespace Parser {
	public class SyntaxTree {
		public SyntaxTree(SyntaxTreeNode root) => Root = root;

		public SyntaxTreeNode Root { get; }

		public static implicit operator SyntaxTree(SyntaxTreeNode node) => new(node);

		public string ToString(bool skipTempNonterminal) => Root.ToString(0, skipTempNonterminal);

		public override string ToString() => ToString(true);
	}
}