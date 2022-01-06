using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class Identifier : SyntaxComponent {
		public Identifier(SyntaxTreeNode node) : base(node) {
			ThrowHelper.IsTerminal(node, LexemeType.Identifier);
			Name = node.GetTokenValue()!;
		}

		public string Name { get; }

		public static implicit operator string(Identifier identifier) => identifier.Name;
	}
}