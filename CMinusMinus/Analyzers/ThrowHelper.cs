using System;
using System.Collections.Generic;
using System.Linq;
using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers {
	internal static class ThrowHelper {
		internal static void IsNonterminal(SyntaxTreeNode node, params NonterminalType[] types) {
			if (node.Value.IsTerminal)
				throw new UnexpectedSyntaxNodeException("Not a nonterminal") { Node = node };
			if (types.Length == 0)
				return;
			if (node.Value.AsNonterminal.GetNameAsEnum<NonterminalType>() is { } type && types.Contains(type))
				return;
			throw new UnexpectedSyntaxNodeException($"Expecting nonterminal(s) {string.Join(",", types.Select(Enum.GetName))}, but {node.Value.AsNonterminal.Name} received") { Node = node };
		}

		internal static void IsTerminal(SyntaxTreeNode node, params LexemeType[] types) {
			if (!node.Value.IsTerminal)
				throw new UnexpectedSyntaxNodeException("Not a terminal") { Node = node };
			if (types.Length == 0)
				return;
			if (node.Value.Lexeme!.GetNameAsEnum<LexemeType>() is { } type && types.Contains(type))
				return;
			throw new UnexpectedSyntaxNodeException($"Expecting terminal type(s) {string.Join(",", types.Select(Enum.GetName))}, but {node.Value.Lexeme.Name} received") { Node = node };
		}

		internal static void IsTerminal(SyntaxTreeNode node, string value) {
			if (!node.Value.IsTerminal)
				throw new UnexpectedSyntaxNodeException("Not a terminal") { Node = node };
			if (node.Value.AsToken.Value is var v && v != value)
				throw new UnexpectedSyntaxNodeException($"Expecting terminal \"{value}\", but {v} received") { Node = node };
		}

		internal static void ChildrenCountIs(SyntaxTreeNode node, int count) {
			if (node.Children.Count != count)
				throw new UnexpectedSyntaxNodeException($"Expect to have {count} children nodes, but {node.Children.Count} received") { Node = node };
		}
	}
}