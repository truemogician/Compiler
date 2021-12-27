using System.Collections.Generic;
using System.Linq;
using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class FunctionDeclaration {
		public FunctionDeclaration(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.FunctionDeclaration);
			var children = node.Children.Select(n => n.Value).ToArray();
			var i = 0;
			for (; i < children.Length && children[i].Lexeme?.GetNameAsEnum<LexemeType>() != LexemeType.Identifier; ++i) { }
			if (i >= children.Length)
				throw new UnexpectedSyntaxNodeException { Node = node };
			ReturnType = new FullType(node.Children[..i]);
			Name = children[i++].AsToken.Value;
			ThrowHelper.IsTerminal(node.Children[i++], LexemeType.LeftParenthesis);
			int j = i;
			var parameters = new List<Parameter>();
			for (; i < children.Length && children[i].Lexeme?.GetNameAsEnum<LexemeType>() is var type && type != LexemeType.RightParenthesis; ++i)
				if (type == LexemeType.Identifier) {
					ThrowHelper.IsTerminal(node.Children[i + 1], LexemeType.Separator, LexemeType.RightParenthesis);
					parameters.Add(new Parameter(new FullType(node.Children[j..i]), children[i].AsToken.Value));
					j = i + 2;
				}
			Parameters = parameters;
			if (i != children.Length - 2)
				throw new UnexpectedSyntaxNodeException { Node = node };
			Body = new Block(node.Children[i + 1]);
		}

		public string Name { get; }

		public FullType ReturnType { get; }

		public IReadOnlyList<Parameter> Parameters { get; }

		public Block Body { get; }

		public static implicit operator FunctionDeclaration(SyntaxTreeNode node) => new(node);

		public record Parameter(FullType Type, string Name);
	}
}