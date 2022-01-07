using System.Linq;
using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class FunctionDefinition : SyntaxComponent {
		public FunctionDefinition(SyntaxTreeNode node) : base(node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.FunctionDefinition);
			var children = node.Children.Select(n => n.Value).ToArray();
			var i = 0;
			for (; i < children.Length && children[i].Lexeme?.GetNameAsEnum<LexemeType>() != LexemeType.Identifier; ++i) { }
			if (i >= children.Length)
				throw new UnexpectedSyntaxNodeException { Node = node };
			Name = new Identifier(children[i]);
			Type = new FunctionType(new FullType(node.Children[..i]), node.Children[(i + 1)..]);
			Body = new Block(node.Children[^1]);
		}

		public Identifier Name { get; }

		public FunctionType Type { get; }

		public Block Body { get; }

		public static implicit operator FunctionDefinition(SyntaxTreeNode node) => new(node);
	}
}