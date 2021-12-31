using System.Collections.Generic;
using System.Linq;
using Analyzer;
using Parser;
using TrueMogician.Extensions.Enumerable;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class VariableDeclaration {
		public VariableDeclaration(IEnumerable<SyntaxTreeNode> nodes) {
			var nds = nodes.AsArray();
			var i = 0;
			for (; i < nds.Length && nds[i].Value.Lexeme?.GetNameAsEnum<LexemeType>() != LexemeType.Identifier; ++i) { }
			if (i == nds.Length)
				throw new UnexpectedSyntaxNodeException();
			Type = new FullType(nds[..i]);
			Name = nds[i++].Value.AsToken.Value;
			if (i < nds.Length) {
				ThrowHelper.IsTerminal(nds[i++], "=");
				if (i == nds.Length)
					throw new UnexpectedSyntaxNodeException();
				DefaultValue = nds[i++];
				if (i != nds.Length)
					throw new UnexpectedSyntaxNodeException();
			}
		}

		public FullType Type { get; }

		public string Name { get; }

		public Expression? DefaultValue { get; }

		public static IEnumerable<VariableDeclaration> FromDeclarationStatement(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.DeclarationStatement);
			var i = 0;
			for (; node.Children[i].Value.Lexeme?.GetNameAsEnum<LexemeType>() == LexemeType.Keyword; ++i) { }
			var prefix = node.Children[..i];
			int prev = i;
			do {
				for (; i < node.Children.Count && node.Children[i].Value is var v && (!v.IsTerminal || v.Lexeme!.GetNameAsEnum<LexemeType>() is not (LexemeType.Separator or LexemeType.Delimiter)); ++i) { }
				if (i == node.Children.Count - 1 && node.Children[i].Value.Lexeme!.GetNameAsEnum<LexemeType>() == LexemeType.Delimiter)
					break;
				if (i >= node.Children.Count)
					throw new UnexpectedSyntaxNodeException { Node = node };
				yield return new VariableDeclaration(prefix.Concat(node.Children[prev..i]));
				prev = ++i;
			} while (true);
		}
	}
}