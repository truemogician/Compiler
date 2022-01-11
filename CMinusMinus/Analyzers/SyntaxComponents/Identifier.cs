using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class Identifier : SyntaxComponent {
		public Identifier(SyntaxTreeNode node) : base(node) {
			ThrowHelper.IsTerminal(node, LexemeType.Identifier);
			Name = node.GetTokenValue()!;
		}

		public string Name { get; }

		public override string ToString() => Name;

		public static implicit operator string(Identifier identifier) => identifier.Name;
	}
}