using System;
using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	internal static class ThrowHelper {
		internal static void IsNonterminal(SyntaxTreeNode node, NonterminalType? type = null) {
			var nt = node.Value.Nonterminal;
			if (nt is null)
				throw new UnexpectedSyntaxNodeException("Not a nonterminal") { Node = node };
			if (type is { } t && nt.Name != Enum.GetName(t))
				throw new UnexpectedSyntaxNodeException($"Expecting nonterminal {Enum.GetName(t)}, but {nt.Name} received") { Node = node };
		}

		internal static void IsTerminal(SyntaxTreeNode node, LexemeType? type = null) {
			var ti = node.Value.TerminalInstance;
			if (ti is null)
				throw new UnexpectedSyntaxNodeException("Not a terminal") { Node = node };
			if (type is { } t && ti.Terminal.Lexeme.Name != Enum.GetName(t))
				throw new UnexpectedSyntaxNodeException($"Expecting terminal {Enum.GetName(t)}, but {ti.Terminal.Lexeme.Name} received") { Node = node };
		}

		internal static void IsTerminal(SyntaxTreeNode node, string value) {
			var ti = node.Value.TerminalInstance;
			if (ti is null)
				throw new UnexpectedSyntaxNodeException("Not a terminal") { Node = node };
			if (ti.Token.Value is var v && v != value)
				throw new UnexpectedSyntaxNodeException($"Expecting terminal \"{value}\", but {v} received") { Node = node };
		}
	}
}