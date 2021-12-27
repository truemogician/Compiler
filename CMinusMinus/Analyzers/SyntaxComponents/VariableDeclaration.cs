using System;
using System.Collections.Generic;
using Analyzer;
using Parser;
using TrueMogician.Extensions.Enumerable;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class VariableDeclaration {
		public VariableDeclaration(IEnumerable<SyntaxTreeNode> nodes) {
			var nds = nodes.AsArray();
			var i = 0;
			for (; i < nds.Length && nds[i].Value.TerminalInstance?.Terminal.Lexeme.Name != nameof(LexemeType.Identifier); ++i) { }
			if (i == nds.Length)
				throw new UnexpectedSyntaxNodeException();
			Type = new FullType(nds[..i]);
			Name = nds[i++].Value.AsTerminalInstance.Token.Value;
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

		public static IEnumerable<VariableDeclaration> FromStatement(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.DeclarationStatement);
			throw new NotImplementedException();
		}
	}
}